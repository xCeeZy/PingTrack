using PingTrack.AppData;
using PingTrack.Model;
using PingTrack.View.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PingTrack.View.Pages
{
    public partial class PlayersPage : Page
    {
        #region Поля

        private PaginationService<PlayerGridItem> pagination;
        private List<PlayerGridItem> allPlayers;
        private bool isInitialized = false;

        #endregion

        #region Конструктор

        public PlayersPage()
        {
            InitializeComponent();

            pagination = new PaginationService<PlayerGridItem>(15);

            InitializeFilters();

            isInitialized = true;

            LoadPlayers();
        }

        #endregion

        #region Инициализация фильтров

        private void InitializeFilters()
        {
            List<Groups> groups = App.db.Groups.Where(g => g.IsDeleted == false).OrderBy(g => g.Group_Name).ToList();
            Groups allGroupsOption = new Groups { ID_Group = 0, Group_Name = "Все группы" };
            groups.Insert(0, allGroupsOption);

            GroupFilter.ItemsSource = groups;
            GroupFilter.DisplayMemberPath = "Group_Name";
            GroupFilter.SelectedIndex = 0;
            GroupFilter.SelectionChanged += Filter_SelectionChanged;
        }

        #endregion

        #region Загрузка данных

        private void LoadPlayers()
        {
            allPlayers = App.db.Players
                .Include("Groups")
                .Where(p => p.IsDeleted == false)
                .ToList()
                .OrderBy(p => p.Full_Name)
                .Select(p => new PlayerGridItem
                {
                    ID_Player = p.ID_Player,
                    Full_Name = p.Full_Name,
                    Birth_Date = p.Birth_Date,
                    Age = CalculateAge(p.Birth_Date),
                    Phone = string.IsNullOrEmpty(p.Phone) ? "-" : p.Phone.Trim(),
                    GroupName = p.Groups?.Group_Name ?? "-",
                    Groups = p.Groups
                })
                .ToList();

            ApplyFilters();
        }

        private int CalculateAge(DateTime birthDate)
        {
            DateTime today = DateTime.Today;
            int age = today.Year - birthDate.Year;
            if (birthDate.Date > today.AddYears(-age))
                age--;
            return age;
        }

        private void UpdateCountDisplay()
        {
            int displayedCount = pagination.GetCurrentPage().Count;
            int totalCount = allPlayers.Count;

            if (displayedCount == totalCount)
                CountTextBlock.Text = $"Всего игроков: {totalCount}";
            else
                CountTextBlock.Text = $"Показано: {displayedCount} из {totalCount}";
        }

        #endregion

        #region Фильтрация

        private void ApplyFilters()
        {
            if (!isInitialized || allPlayers == null)
                return;

            string searchText = SearchBox.Text?.Trim().ToLower() ?? string.Empty;
            Groups selectedGroup = GroupFilter.SelectedItem as Groups;

            IEnumerable<PlayerGridItem> filtered = allPlayers;

            if (selectedGroup != null && selectedGroup.ID_Group != 0)
                filtered = filtered.Where(p => p.GroupName == selectedGroup.Group_Name);

            if (!string.IsNullOrWhiteSpace(searchText) && searchText != "поиск по фио или телефону")
            {
                filtered = filtered.Where(p =>
                    p.Full_Name.ToLower().Contains(searchText) ||
                    (p.Phone != "-" && p.Phone.Contains(searchText)));
            }

            pagination.SetItems(filtered.ToList());
            UpdatePage();
            UpdateCountDisplay();
        }

        #endregion

        #region Обработчики событий поиска и фильтров

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void SearchBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (SearchBox.Text == "Поиск по ФИО или телефону")
            {
                SearchBox.Text = string.Empty;
                SearchBox.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1F2937"));
            }
        }

        private void SearchBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SearchBox.Text))
            {
                SearchBox.Text = "Поиск по ФИО или телефону";
                SearchBox.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6B7280"));
            }
        }

        private void Filter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        #endregion

        #region Пагинация

        private void UpdatePage()
        {
            List<PlayerGridItem> current = pagination.GetCurrentPage();
            PlayersDataGrid.ItemsSource = current;

            string pageText = pagination.TotalPages > 0
                ? $"Страница {pagination.CurrentPage} из {pagination.TotalPages}"
                : "Нет игроков";

            PageInfoText.Text = pageText;
            PrevPageButton.IsEnabled = pagination.HasPreviousPage;
            NextPageButton.IsEnabled = pagination.HasNextPage;
        }

        private void NextPageButton_Click(object sender, RoutedEventArgs e)
        {
            pagination.NextPage();
            UpdatePage();
        }

        private void PrevPageButton_Click(object sender, RoutedEventArgs e)
        {
            pagination.PreviousPage();
            UpdatePage();
        }

        #endregion

        #region Обработчики DataGrid

        private void PlayersDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PlayerGridItem selectedPlayer = PlayersDataGrid.SelectedItem as PlayerGridItem;
            bool hasSelection = selectedPlayer != null;

            EditPlayerButton.IsEnabled = hasSelection;
            DeletePlayerButton.IsEnabled = hasSelection;
        }

        private void PlayersDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            PlayerGridItem selectedItem = PlayersDataGrid.SelectedItem as PlayerGridItem;
            if (selectedItem == null)
                return;

            PlayerCardInfoWindow window = new PlayerCardInfoWindow(selectedItem.ID_Player);
            window.ShowDialog();
        }

        #endregion

        #region Обработчики кнопок

        private void AddPlayerButton_Click(object sender, RoutedEventArgs e)
        {
            AddEditPlayerWindow window = new AddEditPlayerWindow();
            if (window.ShowDialog() == true)
                LoadPlayers();
        }

        private void EditPlayerButton_Click(object sender, RoutedEventArgs e)
        {
            PlayerGridItem selectedItem = PlayersDataGrid.SelectedItem as PlayerGridItem;
            if (selectedItem == null)
            {
                Feedback.ShowWarning("Предупреждение", "Выберите игрока для редактирования.");
                return;
            }

            // Используем корректный поиск по ID_Player вместо FirstOrDefault без критерия
            string selectedName = selectedItem.Full_Name;
            Players player = App.db.Players.FirstOrDefault(p => p.ID_Player == selectedItem.ID_Player);
            if (player == null)
                return;

            AddEditPlayerWindow window = new AddEditPlayerWindow(player);
            if (window.ShowDialog() == true)
                LoadPlayers();
        }

        private void DeletePlayerButton_Click(object sender, RoutedEventArgs e)
        {
            PlayerGridItem selectedItem = PlayersDataGrid.SelectedItem as PlayerGridItem;
            if (selectedItem == null)
            {
                Feedback.ShowWarning("Предупреждение", "Выберите игрока для удаления.");
                return;
            }

            Players player = App.db.Players.FirstOrDefault(p => p.ID_Player == selectedItem.ID_Player);
            if (player == null)
                return;

            int attendanceCount = App.db.Attendance.Count(a => a.ID_Player == player.ID_Player);
            string warningText = attendanceCount > 0
                ? $"У этого игрока есть записи посещаемости ({attendanceCount} шт.).\nПри удалении игрока эти записи также будут удалены!\n\n"
                : string.Empty;

            bool confirm = Feedback.AskQuestion("Подтверждение удаления",
                $"{warningText}Вы уверены, что хотите удалить игрока \"{player.Full_Name}\"?\n\nЭто действие нельзя отменить.");

            if (!confirm)
                return;

            try
            {
                if (attendanceCount > 0)
                {
                    var attendanceRecords = App.db.Attendance.Where(a => a.ID_Player == player.ID_Player).ToList();
                    foreach (var record in attendanceRecords)
                    {
                        App.db.Attendance.Remove(record);
                    }
                }

                string playerName = player.Full_Name;
                App.db.Players.Remove(player);
                App.db.SaveChanges();

                ActionLogService.LogDelete("Players", selectedItem.ID_Player,
                    $"Удалён игрок: {playerName}. Связанных записей посещаемости удалено: {attendanceCount}.");

                Feedback.ShowSuccess("Успешно", "Игрок успешно удалён.");
                LoadPlayers();
            }
            catch (Exception ex)
            {
                Feedback.ShowError("Ошибка удаления", $"Не удалось удалить игрока.\n\n{ex.Message}");
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadPlayers();
            Feedback.ShowInfo("Обновление", "Список игроков обновлён.");
        }

        #endregion
    }

    #region Карточка игрока

    public class PlayerCardInfoWindow : Window
    {
        #region Поля

        private readonly int playerId;
        private TextBlock nameText;
        private TextBlock groupText;
        private TextBlock ageText;
        private TextBlock phoneText;
        private TextBlock medicalText;
        private TextBlock totalText;
        private TextBlock presentText;
        private TextBlock absentText;
        private TextBlock percentText;
        private DataGrid lastAttendanceGrid;

        #endregion

        #region Конструктор

        public PlayerCardInfoWindow(int selectedPlayerId)
        {
            playerId = selectedPlayerId;
            Title = "Карточка игрока";
            Width = 760;
            Height = 560;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            ResizeMode = ResizeMode.NoResize;
            Content = BuildContent();
            LoadPlayerInfo();
        }

        #endregion

        #region Построение интерфейса

        private UIElement BuildContent()
        {
            Grid root = new Grid { Margin = new Thickness(24) };
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(18) });
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(18) });
            root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(18) });
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            root.Children.Add(BuildHeader());
            Grid.SetRow(root.Children[root.Children.Count - 1], 0);

            root.Children.Add(BuildInfoBlock());
            Grid.SetRow(root.Children[root.Children.Count - 1], 2);

            root.Children.Add(BuildStatsBlock());
            Grid.SetRow(root.Children[root.Children.Count - 1], 4);

            Button closeButton = new Button
            {
                Content = "Закрыть",
                Width = 120,
                Height = 36,
                HorizontalAlignment = HorizontalAlignment.Right
            };
            closeButton.Click += (s, e) => Close();
            root.Children.Add(closeButton);
            Grid.SetRow(closeButton, 6);

            return root;
        }

        private Border BuildHeader()
        {
            StackPanel panel = new StackPanel();
            nameText = new TextBlock { FontSize = 24, FontWeight = FontWeights.Bold };
            groupText = new TextBlock { FontSize = 14, Margin = new Thickness(0, 6, 0, 0), Foreground = Brushes.Gray };
            panel.Children.Add(nameText);
            panel.Children.Add(groupText);

            return new Border
            {
                Padding = new Thickness(18),
                BorderBrush = Brushes.LightGray,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(10),
                Child = panel
            };
        }

        private Border BuildInfoBlock()
        {
            Grid grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition());

            ageText = CreateValueBlock(grid, "Возраст", 0);
            phoneText = CreateValueBlock(grid, "Телефон", 1);
            medicalText = CreateValueBlock(grid, "Мед. справка", 2);

            return new Border
            {
                Padding = new Thickness(18),
                BorderBrush = Brushes.LightGray,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(10),
                Child = grid
            };
        }

        private Border BuildStatsBlock()
        {
            Grid grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(12) });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(12) });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            TextBlock title = new TextBlock { Text = "Статистика посещаемости", FontSize = 18, FontWeight = FontWeights.Bold };
            grid.Children.Add(title);
            Grid.SetRow(title, 0);

            UniformGrid statsGrid = new UniformGrid { Columns = 4 };
            totalText = AddStatBlock(statsGrid, "Всего", Brushes.Black);
            presentText = AddStatBlock(statsGrid, "Посещено", Brushes.Green);
            absentText = AddStatBlock(statsGrid, "Пропущено", Brushes.Red);
            percentText = AddStatBlock(statsGrid, "Посещаемость", Brushes.Black);
            grid.Children.Add(statsGrid);
            Grid.SetRow(statsGrid, 2);

            lastAttendanceGrid = new DataGrid
            {
                AutoGenerateColumns = false,
                IsReadOnly = true,
                HeadersVisibility = DataGridHeadersVisibility.Column
            };
            lastAttendanceGrid.Columns.Add(new DataGridTextColumn { Header = "Дата", Binding = new Binding("Date"), Width = 120 });
            lastAttendanceGrid.Columns.Add(new DataGridTextColumn { Header = "Тренировка", Binding = new Binding("TrainingType"), Width = new DataGridLength(1, DataGridLengthUnitType.Star) });
            lastAttendanceGrid.Columns.Add(new DataGridTextColumn { Header = "Статус", Binding = new Binding("Status"), Width = 140 });
            grid.Children.Add(lastAttendanceGrid);
            Grid.SetRow(lastAttendanceGrid, 4);

            return new Border
            {
                Padding = new Thickness(18),
                BorderBrush = Brushes.LightGray,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(10),
                Child = grid
            };
        }

        #endregion

        #region Заполнение данных

        private void LoadPlayerInfo()
        {
            Players player = App.db.Players.Include("Groups").FirstOrDefault(p => p.ID_Player == playerId);
            if (player == null)
            {
                Feedback.ShowError("Ошибка", "Игрок не найден.");
                Close();
                return;
            }

            nameText.Text = player.Full_Name;
            groupText.Text = $"Группа: {player.Groups?.Group_Name ?? "-"}";
            ageText.Text = $"{CalculateAge(player.Birth_Date)} лет";
            phoneText.Text = string.IsNullOrWhiteSpace(player.Phone) ? "-" : player.Phone.Trim();
            medicalText.Text = player.Medical_Clearance_Date.HasValue ? player.Medical_Clearance_Date.Value.ToString("dd.MM.yyyy") : "Не указана";

            List<Attendance> attendances = App.db.Attendance
                .Include("Trainings")
                .Include("Trainings.Training_Types")
                .Where(a => a.ID_Player == player.ID_Player && a.IsDeleted == false && a.Trainings.IsDeleted == false)
                .OrderByDescending(a => a.Trainings.Date)
                .ToList();

            int total = attendances.Count;
            int present = attendances.Count(a => a.Is_Present);
            int absent = total - present;
            double percent = total > 0 ? Math.Round(present * 100.0 / total, 1) : 0;

            totalText.Text = total.ToString();
            presentText.Text = present.ToString();
            absentText.Text = absent.ToString();
            percentText.Text = $"{percent}%";

            lastAttendanceGrid.ItemsSource = attendances.Take(10).Select(a => new PlayerCardAttendanceItem
            {
                Date = a.Trainings.Date.ToString("dd.MM.yyyy"),
                TrainingType = a.Trainings.Training_Types?.Type_Name ?? "-",
                Status = a.Is_Present ? "Присутствовал" : "Отсутствовал"
            }).ToList();

            ActionLogService.LogView("Players", player.ID_Player, $"Открыта карточка игрока: {player.Full_Name}.");
        }

        #endregion

        #region Вспомогательные методы

        private TextBlock CreateValueBlock(Grid grid, string title, int column)
        {
            StackPanel panel = new StackPanel();
            panel.Children.Add(new TextBlock { Text = title, Foreground = Brushes.Gray, FontWeight = FontWeights.SemiBold });
            TextBlock valueText = new TextBlock { FontSize = 18, FontWeight = FontWeights.Bold, Margin = new Thickness(0, 4, 0, 0) };
            panel.Children.Add(valueText);
            grid.Children.Add(panel);
            Grid.SetColumn(panel, column);
            return valueText;
        }

        private TextBlock AddStatBlock(UniformGrid grid, string title, Brush valueBrush)
        {
            StackPanel panel = new StackPanel();
            panel.Children.Add(new TextBlock { Text = title, Foreground = Brushes.Gray });
            TextBlock valueText = new TextBlock { FontSize = 22, FontWeight = FontWeights.Bold, Foreground = valueBrush };
            panel.Children.Add(valueText);
            grid.Children.Add(panel);
            return valueText;
        }

        private int CalculateAge(DateTime birthDate)
        {
            DateTime today = DateTime.Today;
            int age = today.Year - birthDate.Year;
            if (birthDate.Date > today.AddYears(-age))
                age--;
            return age;
        }

        #endregion
    }

    public class PlayerCardAttendanceItem
    {
        public string Date { get; set; }
        public string TrainingType { get; set; }
        public string Status { get; set; }
    }

    #endregion

    #region Вспомогательный класс для отображения

    public class PlayerGridItem
    {
        public int ID_Player { get; set; }
        public string Full_Name { get; set; }
        public DateTime Birth_Date { get; set; }
        public int Age { get; set; }
        public string Phone { get; set; }
        public string GroupName { get; set; }
        public Groups Groups { get; set; }
    }

    #endregion
}
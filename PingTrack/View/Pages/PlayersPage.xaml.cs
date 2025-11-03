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
            LoadPlayers();
            isInitialized = true;
        }
        #endregion

        #region Инициализация фильтров
        private void InitializeFilters()
        {
            List<Groups> groups = App.db.Groups.OrderBy(g => g.Group_Name).ToList();
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
                SearchBox.Text = "";
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

            Players player = App.db.Players.FirstOrDefault(p => p.ID_Player == selectedItem.ID_Player);
            if (player == null)
                return;

            AddEditPlayerWindow window = new AddEditPlayerWindow(player);
            if (window.ShowDialog() == true)
                LoadPlayers();
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
                : "";

            bool confirm = Feedback.AskQuestion("Подтверждение удаления",
                $"{warningText}Вы уверены, что хотите удалить игрока \"{player.Full_Name}\"?\n\nЭто действие нельзя отменить.");

            if (!confirm)
                return;

            try
            {
                App.db.Players.Remove(player);
                App.db.SaveChanges();
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
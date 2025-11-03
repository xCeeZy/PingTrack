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
    public partial class JournalPage : Page
    {
        #region Поля
        private readonly string userRole;
        private PaginationService<JournalGridItem> pagination;
        private List<JournalGridItem> allRecords;
        private bool isInitialized = false;
        #endregion

        #region Конструктор
        public JournalPage(string role)
        {
            InitializeComponent();
            userRole = role;
            pagination = new PaginationService<JournalGridItem>(15);

            InitializeFilters();
            LoadJournal();
            ConfigureUIForRole();

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

            List<FilterOption> presenceOptions = new List<FilterOption>
            {
                new FilterOption { Value = -1, Display = "Все" },
                new FilterOption { Value = 1, Display = "Присутствовал" },
                new FilterOption { Value = 0, Display = "Отсутствовал" }
            };
            PresenceFilter.ItemsSource = presenceOptions;
            PresenceFilter.DisplayMemberPath = "Display";
            PresenceFilter.SelectedIndex = 0;
            PresenceFilter.SelectionChanged += Filter_SelectionChanged;
        }
        #endregion

        #region Настройка UI для роли
        private void ConfigureUIForRole()
        {
            if (userRole == "Студент")
            {
                AddButton.Visibility = Visibility.Collapsed;
                DeleteButton.Visibility = Visibility.Collapsed;
            }
        }
        #endregion

        #region Загрузка данных
        private void LoadJournal()
        {
            allRecords = App.db.Attendance
                .Include("Trainings")
                .Include("Players")
                .Include("Trainings.Training_Types")
                .Include("Players.Groups")
                .ToList()
                .OrderByDescending(a => a.Trainings.Date)
                .ThenBy(a => a.Players.Full_Name)
                .Select(a => new JournalGridItem
                {
                    ID_Record = a.ID_Record,
                    Date = a.Trainings.Date.ToString("dd.MM.yyyy"),
                    Player = a.Players.Full_Name,
                    Group = a.Players.Groups.Group_Name,
                    Training = a.Trainings.Training_Types.Type_Name,
                    IsPresent = a.Is_Present
                })
                .ToList();

            ApplyFilters();
        }

        private void UpdateCountDisplay()
        {
            int totalCount = allRecords.Count;
            CountTextBlock.Text = $"Всего записей: {totalCount}";
        }
        #endregion

        #region Фильтрация
        private void ApplyFilters()
        {
            if (!isInitialized || allRecords == null)
                return;

            string searchText = SearchBox.Text?.Trim().ToLower() ?? string.Empty;
            Groups selectedGroup = GroupFilter.SelectedItem as Groups;
            FilterOption selectedPresence = PresenceFilter.SelectedItem as FilterOption;

            IEnumerable<JournalGridItem> filtered = allRecords;

            if (selectedGroup != null && selectedGroup.ID_Group != 0)
                filtered = filtered.Where(r => r.Group == selectedGroup.Group_Name);

            if (selectedPresence != null && selectedPresence.Value != -1)
                filtered = filtered.Where(r => r.IsPresent == (selectedPresence.Value == 1));

            if (!string.IsNullOrWhiteSpace(searchText) && searchText != "поиск по игроку")
                filtered = filtered.Where(r => r.Player.ToLower().Contains(searchText));

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
            if (SearchBox.Text == "Поиск по игроку")
            {
                SearchBox.Text = "";
                SearchBox.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1F2937"));
            }
        }

        private void SearchBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SearchBox.Text))
            {
                SearchBox.Text = "Поиск по игроку";
                SearchBox.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6B7280"));
            }
        }

        private void Filter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }
        #endregion

        #region Навигация страниц
        private void UpdatePage()
        {
            List<JournalGridItem> current = pagination.GetCurrentPage();
            JournalDataGrid.ItemsSource = current;

            string pageText = pagination.TotalPages > 0
                ? $"Страница {pagination.CurrentPage} из {pagination.TotalPages}"
                : "Нет записей";

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
        private void JournalDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            JournalGridItem selectedRecord = JournalDataGrid.SelectedItem as JournalGridItem;
            bool hasSelection = selectedRecord != null;

            DeleteButton.IsEnabled = hasSelection && userRole != "Студент";
        }

        private void JournalDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (JournalDataGrid.SelectedItem == null)
                return;

            JournalGridItem selected = JournalDataGrid.SelectedItem as JournalGridItem;
            if (selected == null)
                return;

            Attendance record = App.db.Attendance.FirstOrDefault(x => x.ID_Record == selected.ID_Record);
            if (record == null)
                return;

            AddEditAttendanceWindow window = new AddEditAttendanceWindow(userRole, record);
            if (window.ShowDialog() == true)
                LoadJournal();
        }
        #endregion

        #region Обработчики кнопок
        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            AddEditAttendanceWindow window = new AddEditAttendanceWindow(userRole);
            if (window.ShowDialog() == true)
                LoadJournal();
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (JournalDataGrid.SelectedItem == null)
            {
                Feedback.ShowWarning("Предупреждение", "Выберите запись для удаления.");
                return;
            }

            JournalGridItem selected = JournalDataGrid.SelectedItem as JournalGridItem;
            if (selected == null)
                return;

            Attendance record = App.db.Attendance.FirstOrDefault(x => x.ID_Record == selected.ID_Record);
            if (record == null)
                return;

            bool confirm = Feedback.AskQuestion("Подтверждение удаления",
                $"Вы уверены, что хотите удалить запись о посещении?\n\nИгрок: {selected.Player}\nДата: {selected.Date}\n\nЭто действие нельзя отменить.");

            if (!confirm)
                return;

            try
            {
                App.db.Attendance.Remove(record);
                App.db.SaveChanges();
                Feedback.ShowSuccess("Успешно", "Запись успешно удалена.");
                LoadJournal();
            }
            catch (Exception ex)
            {
                Feedback.ShowError("Ошибка удаления", $"Не удалось удалить запись.\n\n{ex.Message}");
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadJournal();
            Feedback.ShowInfo("Обновление", "Журнал посещаемости обновлён.");
        }
        #endregion
    }

    #region Вспомогательные классы
    public class FilterOption
    {
        public int Value { get; set; }
        public string Display { get; set; }
    }

    public class JournalGridItem
    {
        public int ID_Record { get; set; }
        public string Date { get; set; }
        public string Player { get; set; }
        public string Group { get; set; }
        public string Training { get; set; }
        public bool IsPresent { get; set; }
    }
    #endregion
}
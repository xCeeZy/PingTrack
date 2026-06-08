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
            ConfigureUIForRole();

            isInitialized = true;

            LoadJournal();
        }

        #endregion

        #region Инициализация

        private void InitializeFilters()
        {
            InitializeGroupFilter();
            InitializePresenceFilter();
            InitializeDateFilters();
        }

        private void InitializeGroupFilter()
        {
            List<Groups> groups = App.db.Groups
                .Where(g => g.IsDeleted == false)
                .OrderBy(g => g.Group_Name)
                .ToList();

            Groups allGroupsOption = new Groups
            {
                ID_Group = 0,
                Group_Name = "Все группы"
            };

            groups.Insert(0, allGroupsOption);

            GroupFilter.ItemsSource = groups;
            GroupFilter.DisplayMemberPath = "Group_Name";
            GroupFilter.SelectedIndex = 0;

            GroupFilter.SelectionChanged -= Filter_SelectionChanged;
            GroupFilter.SelectionChanged += Filter_SelectionChanged;
        }

        private void InitializePresenceFilter()
        {
            List<FilterOption> presenceOptions = new List<FilterOption>
            {
                new FilterOption { Value = -1, Display = "Все" },
                new FilterOption { Value = 1, Display = "Присутствовал" },
                new FilterOption { Value = 0, Display = "Отсутствовал" }
            };

            PresenceFilter.ItemsSource = presenceOptions;
            PresenceFilter.DisplayMemberPath = "Display";
            PresenceFilter.SelectedIndex = 0;

            PresenceFilter.SelectionChanged -= Filter_SelectionChanged;
            PresenceFilter.SelectionChanged += Filter_SelectionChanged;
        }

        private void InitializeDateFilters()
        {
            StartDateFilter.SelectedDateChanged -= DateFilter_SelectedDateChanged;
            EndDateFilter.SelectedDateChanged -= DateFilter_SelectedDateChanged;

            StartDateFilter.SelectedDate = DateTime.Now.AddMonths(-1).Date;
            EndDateFilter.SelectedDate = DateTime.Now.Date;

            StartDateFilter.SelectedDateChanged += DateFilter_SelectedDateChanged;
            EndDateFilter.SelectedDateChanged += DateFilter_SelectedDateChanged;
        }

        private void ConfigureUIForRole()
        {
            if (userRole == "Игрок")
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
                .Where(a => a.IsDeleted == false
                         && a.Players.IsDeleted == false
                         && a.Trainings.IsDeleted == false)
                .ToList()
                .OrderByDescending(a => a.Trainings != null ? a.Trainings.Date : DateTime.MinValue)
                .ThenBy(a => a.Players != null ? a.Players.Full_Name : string.Empty)
                .Select(a => new JournalGridItem
                {
                    ID_Record = a.ID_Record,
                    DateValue = a.Trainings != null ? a.Trainings.Date.Date : DateTime.MinValue,
                    Date = a.Trainings != null ? a.Trainings.Date.ToString("dd.MM.yyyy") : "-",
                    Player = a.Players != null ? a.Players.Full_Name : "-",
                    Group = a.Players != null && a.Players.Groups != null ? a.Players.Groups.Group_Name : "-",
                    Training = a.Trainings != null && a.Trainings.Training_Types != null ? a.Trainings.Training_Types.Type_Name : "-",
                    IsPresent = a.Is_Present
                })
                .ToList();

            ApplyFilters();
        }

        private void UpdateCountDisplay(int filteredCount)
        {
            CountTextBlock.Text = $"Найдено записей: {filteredCount}";
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

            DateTime? startDate = StartDateFilter.SelectedDate;
            DateTime? endDate = EndDateFilter.SelectedDate;

            IEnumerable<JournalGridItem> filtered = allRecords;

            filtered = ApplyGroupFilter(filtered, selectedGroup);
            filtered = ApplyPresenceFilter(filtered, selectedPresence);
            filtered = ApplyDateFilter(filtered, startDate, endDate);
            filtered = ApplySearchFilter(filtered, searchText);

            List<JournalGridItem> filteredList = filtered.ToList();

            pagination.SetItems(filteredList);
            UpdatePage();
            UpdateCountDisplay(filteredList.Count);
        }

        private IEnumerable<JournalGridItem> ApplyGroupFilter(IEnumerable<JournalGridItem> records, Groups selectedGroup)
        {
            if (selectedGroup == null || selectedGroup.ID_Group == 0)
                return records;

            return records.Where(r => r.Group == selectedGroup.Group_Name);
        }

        private IEnumerable<JournalGridItem> ApplyPresenceFilter(IEnumerable<JournalGridItem> records, FilterOption selectedPresence)
        {
            if (selectedPresence == null || selectedPresence.Value == -1)
                return records;

            bool isPresent = selectedPresence.Value == 1;
            return records.Where(r => r.IsPresent == isPresent);
        }

        private IEnumerable<JournalGridItem> ApplyDateFilter(IEnumerable<JournalGridItem> records, DateTime? startDate, DateTime? endDate)
        {
            if (startDate.HasValue)
                records = records.Where(r => r.DateValue.Date >= startDate.Value.Date);

            if (endDate.HasValue)
                records = records.Where(r => r.DateValue.Date <= endDate.Value.Date);

            return records;
        }

        private IEnumerable<JournalGridItem> ApplySearchFilter(IEnumerable<JournalGridItem> records, string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText) || searchText == "поиск по игроку")
                return records;

            return records.Where(r => r.Player.ToLower().Contains(searchText));
        }

        #endregion

        #region Обработчики фильтров

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void SearchBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (SearchBox.Text == "Поиск по игроку")
            {
                SearchBox.Text = string.Empty;
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

        private void DateFilter_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        #endregion

        #region Навигация страниц

        private void UpdatePage()
        {
            List<JournalGridItem> currentPage = pagination.GetCurrentPage();
            JournalDataGrid.ItemsSource = currentPage;

            PageInfoText.Text = pagination.TotalPages > 0
                ? $"Страница {pagination.CurrentPage} из {pagination.TotalPages}"
                : "Нет записей";

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

        #region Обработчики таблицы

        private void JournalDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            JournalGridItem selectedRecord = JournalDataGrid.SelectedItem as JournalGridItem;
            DeleteButton.IsEnabled = selectedRecord != null && userRole != "Игрок";
        }

        private void JournalDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
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
            JournalGridItem selected = JournalDataGrid.SelectedItem as JournalGridItem;

            if (selected == null)
            {
                Feedback.ShowWarning("Предупреждение", "Выберите запись для удаления.");
                return;
            }

            Attendance record = App.db.Attendance.FirstOrDefault(x => x.ID_Record == selected.ID_Record);

            if (record == null)
                return;

            bool confirm = Feedback.AskQuestion(
                "Подтверждение удаления",
                $"Вы уверены, что хотите удалить запись о посещении?\n\nИгрок: {selected.Player}\nДата: {selected.Date}\n\nОна будет скрыта из статистики, но сохранится в логах.");

            if (!confirm)
                return;

            try
            {
                record.IsDeleted = true;
                App.db.SaveChanges();

                ActionLogService.LogDelete(
                    "Attendance",
                    record.ID_Record,
                    $"Удалена запись посещаемости. Игрок: {selected.Player}, дата: {selected.Date}, статус: {selected.PresenceText}.");

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

    #region Модели отображения

    public class FilterOption
    {
        public int Value { get; set; }
        public string Display { get; set; }
    }

    public class JournalGridItem
    {
        public int ID_Record { get; set; }
        public DateTime DateValue { get; set; }
        public string Date { get; set; }
        public string Player { get; set; }
        public string Group { get; set; }
        public string Training { get; set; }
        public bool IsPresent { get; set; }

        public string PresenceText
        {
            get
            {
                return IsPresent ? "Присутствовал" : "Отсутствовал";
            }
        }
    }

    #endregion
}
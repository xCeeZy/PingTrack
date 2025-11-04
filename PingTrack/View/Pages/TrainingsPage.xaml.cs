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
    public sealed class TrainingGridItem
    {
        public int ID_Training { get; set; }
        public string Date { get; set; }
        public string Time { get; set; }
        public string Group { get; set; }
        public string Coach { get; set; }
        public string Type { get; set; }
        public string Note { get; set; }
    }

    public partial class TrainingsPage : Page
    {
        #region Поля
        private List<TrainingGridItem> allTrainings;
        private bool isInitialized = false;
        #endregion

        #region Конструктор
        public TrainingsPage()
        {
            InitializeComponent();
            InitializeFilters();
            LoadTrainings();
            isInitialized = true;
        }
        #endregion

        #region Инициализация фильтров
        private void InitializeFilters()
        {
            StartDatePicker.SelectedDate = DateTime.Now.AddMonths(-1);
            EndDatePicker.SelectedDate = DateTime.Now.AddMonths(1);

            List<Groups> groups = App.db.Groups.OrderBy(g => g.Group_Name).ToList();
            Groups allGroupsOption = new Groups { ID_Group = 0, Group_Name = "Все группы" };
            groups.Insert(0, allGroupsOption);
            GroupFilter.ItemsSource = groups;
            GroupFilter.DisplayMemberPath = "Group_Name";
            GroupFilter.SelectedIndex = 0;
            GroupFilter.SelectionChanged += Filter_SelectionChanged;

            List<Users> coaches = App.db.Users
                .Include("Roles")
                .Where(u => u.Roles.Role_Name == "Тренер")
                .OrderBy(u => u.Full_Name)
                .ToList();
            Users allCoachesOption = new Users { ID_User = 0, Full_Name = "Все тренеры" };
            coaches.Insert(0, allCoachesOption);
            CoachFilter.ItemsSource = coaches;
            CoachFilter.DisplayMemberPath = "Full_Name";
            CoachFilter.SelectedIndex = 0;
            CoachFilter.SelectionChanged += Filter_SelectionChanged;

            List<Training_Types> types = App.db.Training_Types.OrderBy(t => t.Type_Name).ToList();
            Training_Types allTypesOption = new Training_Types { ID_Type = 0, Type_Name = "Все типы" };
            types.Insert(0, allTypesOption);
            TypeFilter.ItemsSource = types;
            TypeFilter.DisplayMemberPath = "Type_Name";
            TypeFilter.SelectedIndex = 0;
            TypeFilter.SelectionChanged += Filter_SelectionChanged;
        }
        #endregion

        #region Загрузка данных
        private void LoadTrainings()
        {
            allTrainings = App.db.Trainings
                .Include("Groups")
                .Include("Users")
                .Include("Training_Types")
                .ToList()
                .OrderByDescending(t => t.Date)
                .ThenBy(t => t.Time)
                .Select(t => new TrainingGridItem
                {
                    ID_Training = t.ID_Training,
                    Date = t.Date.ToString("dd.MM.yyyy"),
                    Time = t.Time.ToString(@"hh\:mm"),
                    Group = t.Groups != null ? t.Groups.Group_Name : "-",
                    Coach = t.Users != null ? t.Users.Full_Name : "-",
                    Type = t.Training_Types != null ? t.Training_Types.Type_Name : "-",
                    Note = string.IsNullOrEmpty(t.Note) ? "" : (t.Note.Length > 30 ? t.Note.Substring(0, 30) + "..." : t.Note)
                })
                .ToList();

            ApplyFilters();
        }

        private void UpdateCountDisplay()
        {
            int displayedCount = TrainingsDataGrid.Items.Count;
            int totalCount = allTrainings.Count;

            if (displayedCount == totalCount)
                CountTextBlock.Text = $"Всего тренировок: {totalCount}";
            else
                CountTextBlock.Text = $"Показано: {displayedCount} из {totalCount}";
        }
        #endregion

        #region Фильтрация
        private void ApplyFilters()
        {
            if (!isInitialized || allTrainings == null)
                return;

            Groups selectedGroup = GroupFilter.SelectedItem as Groups;
            Users selectedCoach = CoachFilter.SelectedItem as Users;
            Training_Types selectedType = TypeFilter.SelectedItem as Training_Types;

            IEnumerable<TrainingGridItem> filtered = allTrainings;

            if (StartDatePicker.SelectedDate.HasValue && EndDatePicker.SelectedDate.HasValue)
            {
                DateTime startDate = StartDatePicker.SelectedDate.Value;
                DateTime endDate = EndDatePicker.SelectedDate.Value;

                filtered = filtered.Where(t =>
                {
                    DateTime trainingDate = DateTime.ParseExact(t.Date, "dd.MM.yyyy", null);
                    return trainingDate >= startDate && trainingDate <= endDate;
                });
            }

            if (selectedGroup != null && selectedGroup.ID_Group != 0)
                filtered = filtered.Where(t => t.Group == selectedGroup.Group_Name);

            if (selectedCoach != null && selectedCoach.ID_User != 0)
                filtered = filtered.Where(t => t.Coach == selectedCoach.Full_Name);

            if (selectedType != null && selectedType.ID_Type != 0)
                filtered = filtered.Where(t => t.Type == selectedType.Type_Name);

            TrainingsDataGrid.ItemsSource = filtered.ToList();
            UpdateCountDisplay();
        }
        #endregion

        #region Обработчики фильтров
        private void DateFilter_Changed(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void Filter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }
        #endregion

        #region Обработчики DataGrid
        private void TrainingsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TrainingGridItem selectedTraining = TrainingsDataGrid.SelectedItem as TrainingGridItem;
            bool hasSelection = selectedTraining != null;

            EditTrainingButton.IsEnabled = hasSelection;
            DeleteTrainingButton.IsEnabled = hasSelection;

            if (hasSelection)
                SelectionInfoTextBlock.Text = $"Выбрана тренировка: {selectedTraining.Date} {selectedTraining.Time} - {selectedTraining.Group}";
            else
                SelectionInfoTextBlock.Text = "Выберите тренировку для редактирования или удаления";
        }

        private void TrainingsDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (TrainingsDataGrid.SelectedItem == null)
                return;

            TrainingGridItem selected = TrainingsDataGrid.SelectedItem as TrainingGridItem;
            if (selected == null)
                return;

            Trainings training = App.db.Trainings.FirstOrDefault(x => x.ID_Training == selected.ID_Training);
            if (training == null)
                return;

            AddEditTrainingWindow window = new AddEditTrainingWindow(training);
            if (window.ShowDialog() == true)
                LoadTrainings();
        }
        #endregion

        #region Обработчики кнопок
        private void AddTrainingButton_Click(object sender, RoutedEventArgs e)
        {
            AddEditTrainingWindow window = new AddEditTrainingWindow();
            if (window.ShowDialog() == true)
                LoadTrainings();
        }
        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            if (allTrainings == null || allTrainings.Count == 0)
            {
                Feedback.ShowWarning("Экспорт невозможен", "Нет данных для экспорта.");
                return;
            }

            List<int> trainingIds = allTrainings.Select(t => t.ID_Training).ToList();

            List<Trainings> trainingsToExport = App.db.Trainings
                .Include("Groups")
                .Include("Users")
                .Include("Training_Types")
                .Where(t => trainingIds.Contains(t.ID_Training))
                .ToList();

            ExportService.ExportTrainingsToText(trainingsToExport);
        }

        private void EditTrainingButton_Click(object sender, RoutedEventArgs e)
        {
            if (TrainingsDataGrid.SelectedItem == null)
            {
                Feedback.ShowWarning("Предупреждение", "Выберите тренировку для редактирования.");
                return;
            }

            TrainingGridItem selected = TrainingsDataGrid.SelectedItem as TrainingGridItem;
            if (selected == null)
                return;

            Trainings training = App.db.Trainings.FirstOrDefault(x => x.ID_Training == selected.ID_Training);
            if (training == null)
                return;

            AddEditTrainingWindow window = new AddEditTrainingWindow(training);
            if (window.ShowDialog() == true)
                LoadTrainings();
        }

        private void DeleteTrainingButton_Click(object sender, RoutedEventArgs e)
        {
            if (TrainingsDataGrid.SelectedItem == null)
            {
                Feedback.ShowWarning("Предупреждение", "Выберите тренировку для удаления.");
                return;
            }

            TrainingGridItem selected = TrainingsDataGrid.SelectedItem as TrainingGridItem;
            if (selected == null)
                return;

            Trainings training = App.db.Trainings.FirstOrDefault(x => x.ID_Training == selected.ID_Training);
            if (training == null)
                return;

            int attendanceCount = App.db.Attendance.Count(a => a.ID_Training == training.ID_Training);
            string warningText = attendanceCount > 0
                ? $"К этой тренировке привязано записей посещаемости: {attendanceCount}.\nПри удалении тренировки эти записи также будут удалены!\n\n"
                : "";

            bool confirm = Feedback.AskQuestion("Подтверждение удаления",
                $"{warningText}Вы уверены, что хотите удалить тренировку?\n\nДата: {selected.Date}\nВремя: {selected.Time}\nГруппа: {selected.Group}\n\nЭто действие нельзя отменить.");

            if (!confirm)
                return;

            try
            {
                App.db.Trainings.Remove(training);
                App.db.SaveChanges();
                Feedback.ShowSuccess("Успешно", "Тренировка успешно удалена.");
                LoadTrainings();
            }
            catch (Exception ex)
            {
                Feedback.ShowError("Ошибка удаления", $"Не удалось удалить тренировку.\n\n{ex.Message}");
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadTrainings();
            Feedback.ShowInfo("Обновление", "Расписание тренировок обновлено.");
        }
        #endregion
    }
}
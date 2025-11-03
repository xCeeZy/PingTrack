using PingTrack.AppData;
using PingTrack.Model;
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
    public partial class ReportsPage : Page
    {
        #region Конструктор
        public ReportsPage()
        {
            InitializeComponent();
            InitializeReportTypes();
            InitializeFilters();
        }
        #endregion

        #region Инициализация
        private void InitializeReportTypes()
        {
            ReportTypeComboBox.ItemsSource = ReportService.GetReportTypes();
            ReportTypeComboBox.DisplayMemberPath = "Name";
            ReportTypeComboBox.SelectedValuePath = "ID";
            ReportTypeComboBox.SelectedIndex = 0;
        }

        private void InitializeFilters()
        {
            List<Groups> groups = App.db.Groups.OrderBy(g => g.Group_Name).ToList();
            Groups allGroupsOption = new Groups { ID_Group = 0, Group_Name = "Все группы" };
            groups.Insert(0, allGroupsOption);
            GroupComboBox.ItemsSource = groups;
            GroupComboBox.SelectedIndex = 0;

            StartDatePicker.SelectedDate = DateTime.Now.AddMonths(-1);
            EndDatePicker.SelectedDate = DateTime.Now;
        }
        #endregion

        #region Генерация отчётов
        private void GenerateReportButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateFilters())
                return;

            ReportType selectedReport = ReportTypeComboBox.SelectedItem as ReportType;
            DateTime startDate = StartDatePicker.SelectedDate.Value;
            DateTime endDate = EndDatePicker.SelectedDate.Value;
            int? groupId = GroupComboBox.SelectedValue as int?;

            switch (selectedReport.ID)
            {
                case 1:
                    GeneratePlayerAttendanceReport(startDate, endDate, groupId);
                    break;
                case 2:
                    GenerateGroupAttendanceReport(startDate, endDate, groupId);
                    break;
                case 3:
                    GenerateActivityRatingReport(startDate, endDate, groupId);
                    break;
                case 4:
                    GenerateGeneralStatisticsReport(startDate, endDate);
                    break;
            }
        }

        private bool ValidateFilters()
        {
            if (StartDatePicker.SelectedDate == null || EndDatePicker.SelectedDate == null)
            {
                Feedback.ShowWarning("Ошибка", "Выберите период для формирования отчёта.");
                return false;
            }

            if (StartDatePicker.SelectedDate > EndDatePicker.SelectedDate)
            {
                Feedback.ShowWarning("Ошибка", "Дата начала не может быть позже даты окончания.");
                return false;
            }

            return true;
        }

        private void GeneratePlayerAttendanceReport(DateTime startDate, DateTime endDate, int? groupId)
        {
            DataGridHelper.ConfigureColumnsForPlayerAttendance(ReportsDataGrid);
            List<PlayerAttendanceReport> report = ReportService.GeneratePlayerAttendanceReport(startDate, endDate, groupId);
            ReportsDataGrid.ItemsSource = report;
            ReportTitleTextBlock.Text = $"Посещаемость по игрокам: найдено {report.Count} записей";

            if (report.Count == 0)
                Feedback.ShowInfo("Информация", "Нет данных за выбранный период.");
        }

        private void GenerateGroupAttendanceReport(DateTime startDate, DateTime endDate, int? groupId)
        {
            DataGridHelper.ConfigureColumnsForGroupAttendance(ReportsDataGrid);
            List<GroupAttendanceReport> report = ReportService.GenerateGroupAttendanceReport(startDate, endDate, groupId);
            ReportsDataGrid.ItemsSource = report;
            ReportTitleTextBlock.Text = $"Посещаемость по группам: найдено {report.Count} записей";

            if (report.Count == 0)
                Feedback.ShowInfo("Информация", "Нет данных за выбранный период.");
        }

        private void GenerateActivityRatingReport(DateTime startDate, DateTime endDate, int? groupId)
        {
            DataGridHelper.ConfigureColumnsForActivityRating(ReportsDataGrid);
            List<ActivityRatingReport> report = ReportService.GenerateActivityRatingReport(startDate, endDate, groupId);
            ReportsDataGrid.ItemsSource = report;
            ReportTitleTextBlock.Text = $"Рейтинг активности: найдено {report.Count} игроков";

            if (report.Count == 0)
                Feedback.ShowInfo("Информация", "Нет данных за выбранный период.");
        }

        private void GenerateGeneralStatisticsReport(DateTime startDate, DateTime endDate)
        {
            DataGridHelper.ConfigureColumnsForGeneralStatistics(ReportsDataGrid);
            List<GeneralStatisticsReport> report = ReportService.GenerateGeneralStatisticsReport(startDate, endDate);
            ReportsDataGrid.ItemsSource = report;
            ReportTitleTextBlock.Text = $"Общая статистика: найдено {report.Count} типов тренировок";

            if (report.Count == 0)
                Feedback.ShowInfo("Информация", "Нет данных за выбранный период.");
        }
        #endregion

        #region Обработчики событий
        private void ReportTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ReportsDataGrid.ItemsSource = null;
            ReportTitleTextBlock.Text = "Результаты отчёта";
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            ReportsDataGrid.ItemsSource = null;
            ReportTitleTextBlock.Text = "Результаты отчёта";
            ReportTypeComboBox.SelectedIndex = 0;
            GroupComboBox.SelectedIndex = 0;
            StartDatePicker.SelectedDate = DateTime.Now.AddMonths(-1);
            EndDatePicker.SelectedDate = DateTime.Now;
        }
        #endregion
    }
}
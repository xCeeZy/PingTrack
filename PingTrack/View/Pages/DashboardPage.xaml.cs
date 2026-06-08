using PingTrack.AppData;
using PingTrack.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
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
    public partial class DashboardPage : Page
    {
        #region Конструктор

        public DashboardPage()
        {
            InitializeComponent();
            LoadDashboard();
        }

        #endregion

        #region Загрузка панели

        private void LoadDashboard()
        {
            LoadStatistics();
            LoadRecentTrainings();
            LoadRiskPlayers();
            LoadAttendanceChart();
        }

        #endregion

        #region Статистика

        private void LoadStatistics()
        {
            int playersCount = App.db.Players.Count(p => p.IsDeleted == false);
            PlayersCountText.Text = playersCount.ToString();

            int groupsCount = App.db.Groups.Count(g => g.IsDeleted == false);
            GroupsCountText.Text = groupsCount.ToString();

            DateTime today = DateTime.Now.Date;
            int upcomingTrainings = App.db.Trainings.Count(t => t.Date >= today && t.IsDeleted == false);
            UpcomingTrainingsText.Text = upcomingTrainings.ToString();

            int totalAttendance = App.db.Attendance.Count(a => a.IsDeleted == false);
            int presentAttendance = App.db.Attendance.Count(a => a.IsDeleted == false && a.Is_Present);

            double averageAttendance = totalAttendance > 0
                ? presentAttendance * 100.0 / totalAttendance
                : 0.0;

            AverageAttendanceText.Text = string.Format("{0:F1}%", averageAttendance);
        }

        #endregion

        #region Последние тренировки

        private void LoadRecentTrainings()
        {
            List<TrainingDashboardItem> recentTrainings = App.db.Trainings
                .Include("Groups")
                .Include("Users")
                .Include("Training_Types")
                .Include("Attendance")
                .Where(t => t.IsDeleted == false)
                .OrderByDescending(t => t.Date)
                .ThenByDescending(t => t.Time)
                .Take(10)
                .ToList()
                .Select(t => new TrainingDashboardItem
                {
                    DateTime = string.Format("{0:dd.MM.yyyy} {1:hh\\:mm}", t.Date, t.Time),
                    Group = t.Groups?.Group_Name ?? "-",
                    Type = t.Training_Types?.Type_Name ?? "-",
                    Coach = t.Users?.Full_Name ?? "-",
                    Attendance = GetAttendanceInfo(t)
                })
                .ToList();

            RecentTrainingsGrid.ItemsSource = recentTrainings;
        }

        private string GetAttendanceInfo(Trainings training)
        {
            if (training.Attendance == null || training.Attendance.Count == 0)
                return "Нет данных";

            int present = training.Attendance.Count(a => a.Is_Present && a.IsDeleted == false);
            int total = training.Attendance.Count(a => a.IsDeleted == false);

            if (total == 0)
                return "Нет данных";

            return string.Format("{0} из {1}", present, total);
        }

        #endregion

        #region Игроки в зоне риска

        private void LoadRiskPlayers()
        {
            List<PlayerRiskInfo> riskPlayers = PlayerStatisticsService.GetAtRiskPlayers();
            RiskPlayersGrid.ItemsSource = riskPlayers;
        }

        #endregion

        #region Динамика посещаемости

        private void LoadAttendanceChart()
        {
            List<AttendanceChartItem> chartData = new List<AttendanceChartItem>();
            DateTime now = DateTime.Now;

            for (int i = 5; i >= 0; i--)
            {
                DateTime monthDate = now.AddMonths(-i);
                DateTime startOfMonth = new DateTime(monthDate.Year, monthDate.Month, 1);
                DateTime endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

                List<Attendance> monthAttendances = App.db.Attendance
                    .Include("Trainings")
                    .Where(a => a.Trainings.Date >= startOfMonth
                             && a.Trainings.Date <= endOfMonth
                             && a.IsDeleted == false
                             && a.Trainings.IsDeleted == false)
                    .ToList();

                int totalMarks = monthAttendances.Count;
                int presentMarks = monthAttendances.Count(a => a.Is_Present);

                double percent = totalMarks > 0
                    ? presentMarks * 100.0 / totalMarks
                    : 0.0;

                string monthName = GetMonthName(monthDate);
                double barWidth = CalculateBarWidth(percent);

                chartData.Add(new AttendanceChartItem
                {
                    MonthName = monthName,
                    PercentText = string.Format("{0:F1}%", percent),
                    DetailText = string.Format("Посещаемость: {0} из {1} отметок", presentMarks, totalMarks),
                    BarWidth = barWidth
                });
            }

            AttendanceChartItems.ItemsSource = chartData;
        }

        private string GetMonthName(DateTime date)
        {
            string monthName = date.ToString("MMMM yyyy", new CultureInfo("ru-RU"));
            return char.ToUpper(monthName[0]) + monthName.Substring(1);
        }

        private double CalculateBarWidth(double percent)
        {
            const double maxWidth = 600.0;
            return percent > 0 ? percent / 100.0 * maxWidth : 1;
        }

        #endregion

        #region Обработчики событий

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadDashboard();
            Feedback.ShowInfo("Обновление", "Статистика обновлена.");
        }

        #endregion
    }

    #region Модели отображения

    public class TrainingDashboardItem
    {
        public string DateTime { get; set; }
        public string Group { get; set; }
        public string Type { get; set; }
        public string Coach { get; set; }
        public string Attendance { get; set; }
    }

    #endregion
}
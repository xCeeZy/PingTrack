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
    public partial class DashboardPage : Page
    {
        #region Конструктор
        public DashboardPage()
        {
            InitializeComponent();
            LoadDashboard();
        }
        #endregion

        #region Загрузка данных дашборда
        private void LoadDashboard()
        {
            LoadStatistics();
            LoadRecentTrainings();
            LoadRiskPlayers();
            LoadAttendanceChart();
        }

        private void LoadStatistics()
        {
            int playersCount = App.db.Players.Count();
            PlayersCountText.Text = playersCount.ToString();

            int groupsCount = App.db.Groups.Count();
            GroupsCountText.Text = groupsCount.ToString();

            DateTime today = DateTime.Now.Date;
            int upcomingTrainings = App.db.Trainings.Count(t => t.Date >= today);
            UpcomingTrainingsText.Text = upcomingTrainings.ToString();

            double averageAttendance = 0.0;
            int totalAttendance = App.db.Attendance.Count();

            if (totalAttendance > 0)
            {
                int presentCount = App.db.Attendance.Count(a => a.Is_Present);
                averageAttendance = (double)presentCount / totalAttendance * 100.0;
            }

            AverageAttendanceText.Text = string.Format("{0:F1}%", averageAttendance);
        }

        private void LoadRecentTrainings()
        {
            List<TrainingDashboardItem> recentTrainings = App.db.Trainings
                .Include("Groups")
                .Include("Users")
                .Include("Training_Types")
                .Include("Attendance")
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

            int present = training.Attendance.Count(a => a.Is_Present);
            int total = training.Attendance.Count;

            return string.Format("{0} из {1}", present, total);
        }

        private void LoadRiskPlayers()
        {
            List<PlayerRiskInfo> riskPlayers = PlayerStatisticsService.GetAtRiskPlayers();
            RiskPlayersGrid.ItemsSource = riskPlayers;
        }

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
                    .Where(a => a.Trainings.Date >= startOfMonth && a.Trainings.Date <= endOfMonth)
                    .ToList();

                int totalCount = monthAttendances.Count;
                int presentCount = monthAttendances.Count(a => a.Is_Present);
                double percent = totalCount > 0 ? (double)presentCount / totalCount * 100.0 : 0;

                string monthName = monthDate.ToString("MMMM yyyy", new System.Globalization.CultureInfo("ru-RU"));
                monthName = char.ToUpper(monthName[0]) + monthName.Substring(1);

                double maxWidth = 600.0;
                double barWidth = percent > 0 ? (percent / 100.0) * maxWidth : 1;

                chartData.Add(new AttendanceChartItem
                {
                    MonthName = monthName,
                    PercentText = string.Format("{0:F1}%", percent),
                    DetailText = string.Format("Присутствовали: {0} из {1} занятий", presentCount, totalCount),
                    BarWidth = barWidth
                });
            }

            AttendanceChartItems.ItemsSource = chartData;
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

    #region Вспомогательные классы для отображения
    public class TrainingDashboardItem
    {
        public string DateTime { get; set; }
        public string Group { get; set; }
        public string Type { get; set; }
        public string Coach { get; set; }
        public string Attendance { get; set; }
    }

    public class AttendanceChartItem
    {
        public string MonthName { get; set; }
        public string PercentText { get; set; }
        public string DetailText { get; set; }
        public double BarWidth { get; set; }
    }
    #endregion
}
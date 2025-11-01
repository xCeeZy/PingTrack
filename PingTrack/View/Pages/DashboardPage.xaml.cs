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
        public DashboardPage()
        {
            InitializeComponent();
            LoadDashboard();
        }

        private void LoadDashboard()
        {
            int playersCount = App.db.Players.Count();
            PlayersCountText.Text = playersCount.ToString();

            DateTime today = DateTime.Now.Date;
            int upcomingTrainings = App.db.Trainings.Count(t => t.Date >= today);
            UpcomingTrainingsText.Text = upcomingTrainings.ToString();

            double averageAttendance = 0.0;
            if (App.db.Attendance.Any())
                averageAttendance = App.db.Attendance.Average(a => a.Is_Present ? 1.0 : 0.0) * 100.0;

            AverageAttendanceText.Text = string.Format("{0:F1}%", averageAttendance);

            var recent = App.db.Trainings
                .OrderByDescending(t => t.Date)
                .ThenByDescending(t => t.Time)
                .Take(10)
                .ToList()
                .Select(t => new
                {
                    Date = t.Date.ToString("dd.MM.yyyy") + " " + t.Time.ToString(@"hh\:mm"),
                    Group = t.Groups != null ? t.Groups.Group_Name : "-",
                    Type = t.Training_Types != null ? t.Training_Types.Type_Name : "-",
                    Coach = t.Users != null ? t.Users.Login : "-"
                })
                .ToList();

            RecentTrainingsGrid.ItemsSource = recent;
        }
    }
}
using PingTrack.AppData;
using PingTrack.View.Pages;
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

namespace PingTrack.View.Windows
{
    public partial class MainWindow : Window
    {
        private readonly string userRole;
        private readonly string userName;
        private readonly int currentUserId;

        public MainWindow(string role, string login, int userId)
        {
            InitializeComponent();
            userRole = role;
            userName = login;
            currentUserId = userId;

            UserNameText.Text = login;
            RoleNameText.Text = role;
            if (!string.IsNullOrEmpty(login)) UserInitial.Text = login.Substring(0, 1).ToUpper();

            ApplyRolePermissions();
            CheckSystemRisksAsync();

            if (userRole == "Игрок") MainFrame.Navigate(new PlayerDashboardPage(login));
            else MainFrame.Navigate(new DashboardPage());
        }

        private async void CheckSystemRisksAsync()
        {
            if (userRole == "Администратор" || userRole == "Тренер")
            {
                var expiredCount = await Task.Run(() =>
                    App.db.Players.Count(p => p.IsDeleted == false && p.Medical_Clearance_Date < System.DateTime.Now));

                if (expiredCount > 0)
                {
                    Feedback.ShowWarning("Контроль рисков", $"Внимание! У {expiredCount} игроков просрочены медицинские справки.");
                }
            }
        }

        private void ApplyRolePermissions()
        {
            SidebarBorder.Visibility = userRole == "Игрок" ? Visibility.Collapsed : Visibility.Visible;
            if (userRole == "Игрок") SidebarColumn.Width = new GridLength(0);

            bool isAdmin = userRole == "Администратор";
            LogsBtn.Visibility = isAdmin ? Visibility.Visible : Visibility.Collapsed;
            UsersBtn.Visibility = isAdmin ? Visibility.Visible : Visibility.Collapsed;
            ReportsBtn.Visibility = (isAdmin || userRole == "Тренер") ? Visibility.Visible : Visibility.Collapsed;
        }

        private void LogsBtn_Click(object sender, RoutedEventArgs e) => MainFrame.Navigate(new LogsPage());
        private void DashboardBtn_Click(object sender, RoutedEventArgs e) => MainFrame.Navigate(new DashboardPage());
        private void PlayersBtn_Click(object sender, RoutedEventArgs e) => MainFrame.Navigate(new PlayersPage());
        private void GroupsBtn_Click(object sender, RoutedEventArgs e) => MainFrame.Navigate(new GroupsPage());
        private void TrainingsBtn_Click(object sender, RoutedEventArgs e) => MainFrame.Navigate(new TrainingsPage());
        private void JournalBtn_Click(object sender, RoutedEventArgs e) => MainFrame.Navigate(new JournalPage(userRole));
        private void ReportsBtn_Click(object sender, RoutedEventArgs e) => MainFrame.Navigate(new ReportsPage());
        private void PlayerStatsBtn_Click(object sender, RoutedEventArgs e) => MainFrame.Navigate(new PlayerStatsPage());
        private void UsersBtn_Click(object sender, RoutedEventArgs e) => MainFrame.Navigate(new UsersPage());

        private void LogoutBtn_Click(object sender, RoutedEventArgs e)
        {
            if (Feedback.AskQuestion("Подтверждение", "Выйти из системы?"))
            {
                AuthenticationService.Logout();
                new LoginWindow().Show();
                Close();
            }
        }
    }
}
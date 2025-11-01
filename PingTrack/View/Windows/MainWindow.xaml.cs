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

        public MainWindow(string role, string login)
        {
            InitializeComponent();
            userRole = role;
            userName = login;
            RoleText.Text = "Роль: " + role;
            UserText.Text = "Пользователь: " + login;
            ApplyRolePermissions();
        }

        private void ApplyRolePermissions()
        {
            if (userRole == "Студент")
            {
                PlayersBtn.Visibility = Visibility.Collapsed;
                GroupsBtn.Visibility = Visibility.Collapsed;
                TrainingsBtn.Visibility = Visibility.Collapsed;
                ReportsBtn.Visibility = Visibility.Collapsed;
                UsersBtn.Visibility = Visibility.Collapsed;
            }

            if (userRole == "Тренер")
            {
                ReportsBtn.Visibility = Visibility.Collapsed;
                UsersBtn.Visibility = Visibility.Collapsed;
            }

            if (userRole == "Администратор")
            {
                UsersBtn.Visibility = Visibility.Visible;
            }
        }

        private void DashboardBtn_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new DashboardPage());
        }

        private void PlayersBtn_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new PlayersPage());
        }

        private void GroupsBtn_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new GroupsPage());
        }

        private void TrainingsBtn_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new TrainingsPage());
        }

        private void JournalBtn_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new JournalPage(userRole));
        }

        private void ReportsBtn_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new ReportsPage());
        }

        private void UsersBtn_Click(object sender, RoutedEventArgs e)
        {
            if (userRole == "Администратор")
                MainFrame.Navigate(new UsersPage());
            else
                Feedback.ShowWarning("Доступ запрещён", "Раздел доступен только администраторам.");
        }

        private void LogoutBtn_Click(object sender, RoutedEventArgs e)
        {
            AuthenticationService.Logout();
            new LoginWindow().Show();
            Close();
        }
    }
}

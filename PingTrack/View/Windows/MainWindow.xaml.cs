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
            }

            if (userRole == "Тренер")
            {
                ReportsBtn.Visibility = Visibility.Collapsed;
            }
        }

        private void DashboardBtn_Click(object sender, RoutedEventArgs e)
        {
            DashboardPage page = new DashboardPage();
            MainFrame.Navigate(page);
        }

        private void PlayersBtn_Click(object sender, RoutedEventArgs e)
        {
            PlayersPage page = new PlayersPage();
            MainFrame.Navigate(page);
        }

        private void GroupsBtn_Click(object sender, RoutedEventArgs e)
        {
            GroupsPage page = new GroupsPage();
            MainFrame.Navigate(page);
        }

        private void TrainingsBtn_Click(object sender, RoutedEventArgs e)
        {
            TrainingsPage page = new TrainingsPage();
            MainFrame.Navigate(page);
        }

        private void JournalBtn_Click(object sender, RoutedEventArgs e)
        {
            JournalPage page = new JournalPage(userRole);
            MainFrame.Navigate(page);
        }

        private void ReportsBtn_Click(object sender, RoutedEventArgs e)
        {
            ReportsPage page = new ReportsPage();
            MainFrame.Navigate(page);
        }
    }
}

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
        #region Поля
        private readonly string userRole;
        private readonly string userName;
        #endregion

        #region Конструктор
        public MainWindow(string role, string login)
        {
            InitializeComponent();
            userRole = role;
            userName = login;

            UserNameText.Text = login;
            RoleNameText.Text = role;

            if (!string.IsNullOrEmpty(login))
                UserInitial.Text = login.Substring(0, 1).ToUpper();

            ApplyRolePermissions();
            MainFrame.Navigate(new DashboardPage());
        }
        #endregion

        #region Применение прав доступа
        private void ApplyRolePermissions()
        {
            if (userRole == "Студент")
            {
                PlayersBtn.Visibility = Visibility.Collapsed;
                GroupsBtn.Visibility = Visibility.Collapsed;
                TrainingsBtn.Visibility = Visibility.Collapsed;
                PlayerStatsBtn.Visibility = Visibility.Collapsed;
                ReportsBtn.Visibility = Visibility.Collapsed;
                UsersBtn.Visibility = Visibility.Collapsed;
            }

            if (userRole == "Тренер")
            {
                PlayerStatsBtn.Visibility = Visibility.Visible;
                ReportsBtn.Visibility = Visibility.Collapsed;
                UsersBtn.Visibility = Visibility.Collapsed;
            }

            if (userRole == "Администратор")
            {
                PlayerStatsBtn.Visibility = Visibility.Visible;
                ReportsBtn.Visibility = Visibility.Visible;
                UsersBtn.Visibility = Visibility.Visible;
            }
        }
        #endregion

        #region Навигация
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

        private void PlayerStatsBtn_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new PlayerStatsPage());
        }

        private void UsersBtn_Click(object sender, RoutedEventArgs e)
        {
            if (userRole == "Администратор")
                MainFrame.Navigate(new UsersPage());
            else
                Feedback.ShowWarning("Доступ запрещён", "Раздел доступен только администраторам.");
        }
        #endregion

        #region Выход
        private void LogoutBtn_Click(object sender, RoutedEventArgs e)
        {
            bool confirm = Feedback.AskQuestion("Подтверждение", "Вы действительно хотите выйти?");
            if (confirm)
            {
                AuthenticationService.Logout();
                LoginWindow loginWindow = new LoginWindow();
                loginWindow.Show();
                Close();
            }
        }
        #endregion
    }
}
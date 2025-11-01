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
using System.Windows.Shapes;

namespace PingTrack.View.Windows
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string login = LoginTextBox.Text.Trim();
            string password = PasswordBox.Password.Trim();

            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
            {
                Feedback.ShowWarning("Ошибка", "Введите логин и пароль.");
                return;
            }

            bool isAuthenticated = AuthenticationService.Login(login, password);

            if (!isAuthenticated)
            {
                Feedback.ShowError("Ошибка", "Неверный логин или пароль.");
                return;
            }

            string role = AuthenticationService.GetUserRole();
            string username = AuthenticationService.GetUserLogin();

            Feedback.ShowInfo("Добро пожаловать", "Вы вошли как " + role + ".");

            MainWindow main = new MainWindow(role, username);
            main.Show();
            Close();
        }
    }
}
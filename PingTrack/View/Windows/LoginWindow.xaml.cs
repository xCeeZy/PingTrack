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
        private bool isPasswordVisible = false;

        #region Конструктор
        public LoginWindow()
        {
            InitializeComponent();
        }
        #endregion

        #region Кнопка входа
        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string login = LoginTextBox.Text.Trim();
            string password = isPasswordVisible ? VisiblePasswordBox.Text.Trim() : PasswordBox.Password.Trim();

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
        #endregion

        #region Показ/скрытие пароля
        private void TogglePasswordButton_Click(object sender, RoutedEventArgs e)
        {
            if (isPasswordVisible)
            {
                PasswordBox.Password = VisiblePasswordBox.Text;
                VisiblePasswordBox.Visibility = Visibility.Collapsed;
                PasswordBox.Visibility = Visibility.Visible;
                EyeIcon.Source = new BitmapImage(new System.Uri("pack://application:,,,/Resources/Images/eye_closed.png"));
            }
            else
            {
                VisiblePasswordBox.Text = PasswordBox.Password;
                PasswordBox.Visibility = Visibility.Collapsed;
                VisiblePasswordBox.Visibility = Visibility.Visible;
                EyeIcon.Source = new BitmapImage(new System.Uri("pack://application:,,,/Resources/Images/eye_open.png"));
            }

            isPasswordVisible = !isPasswordVisible;
        }
        #endregion
    }
}
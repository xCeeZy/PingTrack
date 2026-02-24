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
        #region Поля
        private bool isPasswordVisible = false;
        #endregion

        #region Конструктор
        public LoginWindow()
        {
            InitializeComponent();

            LoginTextBox.Focus();

            LoginTextBox.KeyDown += InputField_KeyDown;
            PasswordBox.KeyDown += InputField_KeyDown;
            VisiblePasswordBox.KeyDown += InputField_KeyDown;

            // Обработчики для проверки состояния Caps Lock
            this.KeyDown += CheckCapsLock;
            this.KeyUp += CheckCapsLock;
            this.Loaded += (s, e) => CheckCapsLock(null, null);
        }
        #endregion

        #region Проверка Caps Lock
        private void CheckCapsLock(object sender, KeyEventArgs e)
        {
            if (Keyboard.IsKeyToggled(Key.CapsLock))
                CapsLockWarning.Visibility = Visibility.Visible;
            else
                CapsLockWarning.Visibility = Visibility.Collapsed;
        }
        #endregion

        #region Обработка нажатия Enter
        private void InputField_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (sender == LoginTextBox)
                {
                    if (isPasswordVisible)
                        VisiblePasswordBox.Focus();
                    else
                        PasswordBox.Focus();
                }
                else
                {
                    LoginButton_Click(null, null);
                }
            }
        }
        #endregion

        #region Вход в систему
        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            // Обрезаем только логин. Пароль берем как есть (пробелы могут быть частью пароля)
            string login = LoginTextBox.Text.Trim();
            string password = isPasswordVisible ? VisiblePasswordBox.Text : PasswordBox.Password;

            if (string.IsNullOrWhiteSpace(login))
            {
                Feedback.ShowWarning("Внимание", "Введите логин.");
                LoginTextBox.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                Feedback.ShowWarning("Внимание", "Введите пароль.");
                if (isPasswordVisible)
                    VisiblePasswordBox.Focus();
                else
                    PasswordBox.Focus();
                return;
            }

            bool isAuthenticated = AuthenticationService.Login(login, password);

            if (!isAuthenticated)
            {
                Feedback.ShowError("Ошибка входа", "Неверный логин или пароль.");
                PasswordBox.Clear();
                VisiblePasswordBox.Clear();

                if (isPasswordVisible)
                    VisiblePasswordBox.Focus();
                else
                    PasswordBox.Focus();

                return;
            }

            string role = AuthenticationService.GetUserRole();
            string username = AuthenticationService.GetUserLogin();

            MainWindow mainWindow = new MainWindow(role, username);
            mainWindow.Show();
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

                // Смена картинки на закрытый глаз
                EyeIcon.Source = new BitmapImage(new Uri("pack://application:,,,/Resources/Images/eye_closed.png"));

                PasswordBox.Focus();
            }
            else
            {
                VisiblePasswordBox.Text = PasswordBox.Password;
                PasswordBox.Visibility = Visibility.Collapsed;
                VisiblePasswordBox.Visibility = Visibility.Visible;

                // Смена картинки на открытый глаз
                EyeIcon.Source = new BitmapImage(new Uri("pack://application:,,,/Resources/Images/eye_open.png"));

                VisiblePasswordBox.Focus();
                VisiblePasswordBox.SelectionStart = VisiblePasswordBox.Text.Length;
            }

            isPasswordVisible = !isPasswordVisible;
        }
        #endregion
    }
}
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
using System.Windows.Threading;

namespace PingTrack.View.Windows
{
    public partial class LoginWindow : Window
    {
        #region Поля

        private const int MaxFailedAttempts = 5;
        private const int LockoutSeconds = 30;

        private bool isPasswordVisible = false;
        private int failedAttempts = 0;
        private DateTime? lockoutEndTime;
        private DispatcherTimer lockoutTimer;

        #endregion

        #region Конструктор

        public LoginWindow()
        {
            InitializeComponent();

            InitializeEvents();
            InitializeLockoutTimer();

            LoginTextBox.Focus();
        }

        #endregion

        #region Инициализация

        private void InitializeEvents()
        {
            LoginTextBox.KeyDown += InputField_KeyDown;
            PasswordBox.KeyDown += InputField_KeyDown;
            VisiblePasswordBox.KeyDown += InputField_KeyDown;

            KeyDown += CheckCapsLock;
            KeyUp += CheckCapsLock;
            Loaded += (s, e) => CheckCapsLock(null, null);
        }

        private void InitializeLockoutTimer()
        {
            lockoutTimer = new DispatcherTimer();
            lockoutTimer.Interval = TimeSpan.FromSeconds(1);
            lockoutTimer.Tick += LockoutTimer_Tick;
        }

        #endregion

        #region Проверка Caps Lock

        private void CheckCapsLock(object sender, KeyEventArgs e)
        {
            CapsLockWarning.Visibility = Keyboard.IsKeyToggled(Key.CapsLock)
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        #endregion

        #region Обработка клавиатуры

        private void InputField_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter)
                return;

            if (IsLoginLocked())
            {
                ShowLockoutMessage();
                return;
            }

            if (sender == LoginTextBox)
            {
                FocusPasswordField();
                return;
            }

            LoginButton_Click(null, null);
        }

        #endregion

        #region Авторизация

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            if (IsLoginLocked())
            {
                ShowLockoutMessage();
                return;
            }

            string login = LoginTextBox.Text.Trim();
            string password = GetCurrentPassword();

            if (!ValidateInput(login, password))
                return;

            LoginButton.IsEnabled = false;

            bool isAuthenticated = false;

            try
            {
                isAuthenticated = AuthenticationService.Login(login, password);

                if (!isAuthenticated)
                {
                    ProcessFailedLogin(login);
                    return;
                }

                ResetFailedAttempts();
                OpenMainWindow();
            }
            catch (Exception ex)
            {
                Feedback.ShowError("Ошибка входа", $"Не удалось выполнить вход в систему.\n\n{ex.Message}");
            }
            finally
            {
                if (!isAuthenticated && !IsLoginLocked())
                    LoginButton.IsEnabled = true;
            }
        }

        private bool ValidateInput(string login, string password)
        {
            if (string.IsNullOrWhiteSpace(login))
            {
                Feedback.ShowWarning("Внимание", "Введите логин.");
                LoginTextBox.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                Feedback.ShowWarning("Внимание", "Введите пароль.");
                FocusPasswordField();
                return false;
            }

            return true;
        }

        private void ProcessFailedLogin(string login)
        {
            failedAttempts++;

            ClearPasswordFields();

            int attemptsLeft = MaxFailedAttempts - failedAttempts;

            if (attemptsLeft <= 0)
            {
                StartLockout();
                return;
            }

            Feedback.ShowError(
                "Ошибка входа",
                $"Неверный логин или пароль.\nОсталось попыток: {attemptsLeft}.");

            FocusPasswordField();
        }

        private void StartLockout()
        {
            lockoutEndTime = DateTime.Now.AddSeconds(LockoutSeconds);
            LoginButton.IsEnabled = false;
            lockoutTimer.Start();

            Feedback.ShowWarning(
                "Вход временно заблокирован",
                $"Слишком много неверных попыток входа.\nПовторите попытку через {LockoutSeconds} секунд.");
        }

        private bool IsLoginLocked()
        {
            return lockoutEndTime.HasValue && DateTime.Now < lockoutEndTime.Value;
        }

        private void ShowLockoutMessage()
        {
            int secondsLeft = GetLockoutSecondsLeft();

            Feedback.ShowWarning(
                "Вход временно заблокирован",
                $"Повторите попытку через {secondsLeft} сек.");
        }

        private int GetLockoutSecondsLeft()
        {
            if (!lockoutEndTime.HasValue)
                return 0;

            double secondsLeft = (lockoutEndTime.Value - DateTime.Now).TotalSeconds;

            return secondsLeft > 0
                ? (int)Math.Ceiling(secondsLeft)
                : 0;
        }

        private void LockoutTimer_Tick(object sender, EventArgs e)
        {
            if (IsLoginLocked())
                return;

            lockoutTimer.Stop();
            lockoutEndTime = null;
            failedAttempts = 0;
            LoginButton.IsEnabled = true;

            Feedback.ShowInfo("Вход разблокирован", "Теперь можно снова выполнить вход.");
            FocusPasswordField();
        }

        private void ResetFailedAttempts()
        {
            failedAttempts = 0;
            lockoutEndTime = null;
            lockoutTimer.Stop();
            LoginButton.IsEnabled = true;
        }

        private void OpenMainWindow()
        {
            string role = AuthenticationService.GetUserRole();
            string username = AuthenticationService.GetUserLogin();
            int userId = AuthenticationService.GetUserId();

            MainWindow mainWindow = new MainWindow(role, username, userId);
            mainWindow.Show();

            Close();
        }

        #endregion

        #region Показ пароля

        private void TogglePasswordButton_Click(object sender, RoutedEventArgs e)
        {
            if (isPasswordVisible)
                HidePassword();
            else
                ShowPassword();

            isPasswordVisible = !isPasswordVisible;
        }

        private void ShowPassword()
        {
            VisiblePasswordBox.Text = PasswordBox.Password;
            PasswordBox.Visibility = Visibility.Collapsed;
            VisiblePasswordBox.Visibility = Visibility.Visible;

            EyeIcon.Source = new BitmapImage(new Uri("pack://application:,,,/Resources/Images/eye_open.png"));

            VisiblePasswordBox.Focus();
            VisiblePasswordBox.SelectionStart = VisiblePasswordBox.Text.Length;
        }

        private void HidePassword()
        {
            PasswordBox.Password = VisiblePasswordBox.Text;
            VisiblePasswordBox.Visibility = Visibility.Collapsed;
            PasswordBox.Visibility = Visibility.Visible;

            EyeIcon.Source = new BitmapImage(new Uri("pack://application:,,,/Resources/Images/eye_closed.png"));

            PasswordBox.Focus();
        }

        private string GetCurrentPassword()
        {
            return isPasswordVisible ? VisiblePasswordBox.Text : PasswordBox.Password;
        }

        private void ClearPasswordFields()
        {
            PasswordBox.Clear();
            VisiblePasswordBox.Clear();
        }

        private void FocusPasswordField()
        {
            if (isPasswordVisible)
                VisiblePasswordBox.Focus();
            else
                PasswordBox.Focus();
        }

        #endregion
    }
}
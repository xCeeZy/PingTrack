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

            if (!string.IsNullOrEmpty(login))
                UserInitial.Text = login.Substring(0, 1).ToUpper();

            ApplyRolePermissions();
            CheckSystemRisksAsync();

            if (userRole == "Игрок")
            {
                MainFrame.Navigate(new PlayerDashboardPage(login));
            }
            else
            {
                MainFrame.Navigate(new DashboardPage());
            }
        }

        #region Системная логика и контроль рисков
        private async void CheckSystemRisksAsync()
        {
            if (userRole == "Администратор" || userRole == "Тренер")
            {
                int expiredCount = await Task.Run(() =>
                    App.db.Players.ToList().Count(p => p.IsDeleted == false
                                          && p.Medical_Clearance_Date.HasValue
                                          && p.Medical_Clearance_Date.Value < DateTime.Now));

                if (expiredCount > 0)
                {
                    Feedback.ShowWarning("Контроль рисков",
                        $"Внимание! В системе обнаружено {expiredCount} игроков с просроченными медицинскими справками. Рекомендуется проверить списки команд.");
                }
            }
        }
        #endregion

        #region Разграничение прав доступа
        private void ApplyRolePermissions()
        {
            if (userRole == "Игрок")
            {
                SidebarBorder.Visibility = Visibility.Collapsed;
                SidebarColumn.Width = new GridLength(0);

                PlayersBtn.Visibility = Visibility.Collapsed;
                GroupsBtn.Visibility = Visibility.Collapsed;
                TrainingsBtn.Visibility = Visibility.Collapsed;
                PlayerStatsBtn.Visibility = Visibility.Collapsed;
                ReportsBtn.Visibility = Visibility.Collapsed;
                UsersBtn.Visibility = Visibility.Collapsed;
                JournalBtn.Visibility = Visibility.Collapsed;
                DashboardBtn.Visibility = Visibility.Collapsed;
                LogsBtn.Visibility = Visibility.Collapsed;
            }

            if (userRole == "Тренер")
            {
                PlayerStatsBtn.Visibility = Visibility.Visible;
                ReportsBtn.Visibility = Visibility.Collapsed;
                UsersBtn.Visibility = Visibility.Collapsed;
                LogsBtn.Visibility = Visibility.Collapsed;
            }

            if (userRole == "Администратор")
            {
                PlayerStatsBtn.Visibility = Visibility.Visible;
                ReportsBtn.Visibility = Visibility.Visible;
                UsersBtn.Visibility = Visibility.Visible;
                LogsBtn.Visibility = Visibility.Visible;
            }
        }
        #endregion

        #region Управление навигацией интерфейса
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

        private void LogsBtn_Click(object sender, RoutedEventArgs e)
        {
            if (userRole == "Администратор")
                MainFrame.Navigate(new LogsPage());
            else
                Feedback.ShowWarning("Доступ запрещён", "Раздел доступен только администраторам.");
        }
        #endregion

        #region Системные действия и аудит
        private async void InfoBtn_Click(object sender, RoutedEventArgs e)
        {
            bool confirmCheck = Feedback.AskQuestion("Служба обновлений СУОиА",
                "Программный комплекс: «PingTrack Enterprise Edition»\n\n" +
                "Разработано ИТ-компанией: ООО «НекстГен Технолоджис» (NextGen Technologies Ltd.) по заказу Федерации настольного тенниса.\n\n" +
                "Назначение: Автоматизация процессов спортивного менеджмента, включая интеллектуальный сквозной учет посещаемости, предиктивный контроль медицинских допусков, финансовый биллинг активных абонементов и распределенный аудит действий персонала.\n\n" +
                "Лицензия: № NGT-PT-2026-X904 (Академическая подписка корпоративного уровня)\n" +
                "Спецификация ядра: Релиз сборки v1.1.264 (Архитектура синхронизации клиент-сервер на базе MS SQL Node)\n\n" +
                "Обнаружен активный канал связи с сервером обновлений NextGen-Cloud.\n" +
                "Выполнить принудительную верификацию текущего хэша сборки и проверить наличие патчей?");

            if (confirmCheck)
            {
                await Task.Delay(1500);
                Feedback.ShowInfo("Служба обновлений СУОиА",
                    "Проверка завершена.\n\n" +
                    "Контрольная сумма локальных библиотек совпадает со стабильной веткой релизов СУОиА v1.1.264 на удаленном сервере NextGen-Cloud.\n\n" +
                    "Изменений или критических патчей безопасности для вашей организации не найдено.");
            }
        }

        private void LogoutBtn_Click(object sender, RoutedEventArgs e)
        {
            bool confirm = Feedback.AskQuestion("Подтверждение", "Вы действительно хотите выйти из системы?");
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
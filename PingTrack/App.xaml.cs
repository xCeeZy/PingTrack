using PingTrack.AppData;
using PingTrack.Model;
using PingTrack.View.Windows;
using System;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Windows;

namespace PingTrack
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static RusakovPingTrackEntities db;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            try
            {
                db = new RusakovPingTrackEntities();

                // Проверяем доступность подключения к БД
                db.Database.Connection.Open();
                db.Database.Connection.Close();

                LoginWindow loginWindow = new LoginWindow();
                loginWindow.Show();
            }
            catch (DbUpdateException ex)
            {
                ShowDatabaseError(ex);
            }
            catch (System.Data.SqlClient.SqlException ex)
            {
                ShowDatabaseError(ex);
            }
            catch (Exception ex)
            {
                Feedback.ShowError(
                    "Ошибка запуска",
                    $"Не удалось запустить приложение.\n\nПричина: {ex.Message}\n\nПроверьте подключение к базе данных и настройки App.config.");

                Shutdown();
            }
        }

        private void ShowDatabaseError(Exception ex)
        {
            Feedback.ShowError(
                "Ошибка подключения к базе данных",
                $"Приложение не смогло подключиться к SQL Server.\n\n{ex.Message}\n\nПроверьте:\n• Запущен ли SQL Server (SQLEXPRESS)\n• Существует ли база данных RusakovPingTrack\n• Правильна ли строка подключения в App.config");

            Shutdown();
        }
    }
}
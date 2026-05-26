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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data.Entity;

namespace PingTrack.View.Pages
{
    public partial class LogsPage : Page
    {
        #region Конструктор
        public LogsPage()
        {
            InitializeComponent();
            LoadLogsAsync();
        }
        #endregion

        #region Асинхронная загрузка данных
        private async void LoadLogsAsync()
        {
            try
            {
                List<ActionLogs> logs = await Task.Run(() =>
                {
                    return App.db.ActionLogs
                        .Include(l => l.Users)
                        .OrderByDescending(l => l.Log_Date)
                        .Take(300)
                        .ToList();
                });

                LogsDataGrid.ItemsSource = logs;
            }
            catch (Exception ex)
            {
                Feedback.ShowError("Ошибка загрузки", $"Не удалось загрузить системный журнал.\n\n{ex.Message}");
            }
        }
        #endregion

        #region Обработчики событий интерфейса
        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadLogsAsync();
            Feedback.ShowInfo("Обновление", "Журнал системных действий успешно обновлен.");
        }
        #endregion
    }
}

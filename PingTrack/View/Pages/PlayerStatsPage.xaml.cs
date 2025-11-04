using PingTrack.AppData;
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

namespace PingTrack.View.Pages
{
    public partial class PlayerStatsPage : Page
    {
        #region Конструктор
        public PlayerStatsPage()
        {
            InitializeComponent();
            LoadPlayers();
            StartDatePicker.SelectedDate = DateTime.Now.AddMonths(-3);
            EndDatePicker.SelectedDate = DateTime.Now;
        }
        #endregion

        #region Загрузка данных
        private void LoadPlayers()
        {
            PlayerComboBox.ItemsSource = App.db.Players
                .OrderBy(p => p.Full_Name)
                .ToList();
        }
        #endregion

        #region Обработчики событий
        private void LoadStatsButton_Click(object sender, RoutedEventArgs e)
        {
            if (PlayerComboBox.SelectedValue == null)
            {
                Feedback.ShowWarning("Ошибка", "Выберите игрока");
                return;
            }

            if (!StartDatePicker.SelectedDate.HasValue || !EndDatePicker.SelectedDate.HasValue)
            {
                Feedback.ShowWarning("Ошибка", "Выберите период");
                return;
            }

            int playerId = (int)PlayerComboBox.SelectedValue;
            DateTime startDate = StartDatePicker.SelectedDate.Value;
            DateTime endDate = EndDatePicker.SelectedDate.Value;

            PlayerDetailedStats stats = PlayerStatisticsService.GetPlayerStats(playerId, startDate, endDate);

            if (stats == null)
            {
                Feedback.ShowError("Ошибка", "Не удалось загрузить статистику");
                return;
            }

            DisplayStats(stats);
        }
        #endregion

        #region Отображение статистики
        private void DisplayStats(PlayerDetailedStats stats)
        {
            PlaceholderText.Visibility = Visibility.Collapsed;
            StatsScrollViewer.Visibility = Visibility.Visible;

            PlayerNameText.Text = stats.PlayerName;
            GroupNameText.Text = stats.GroupName;
            LastAttendanceText.Text = stats.LastAttendance != DateTime.MinValue
                ? stats.LastAttendance.ToString("dd.MM.yyyy")
                : "Нет данных";

            TotalTrainingsText.Text = stats.TotalTrainings.ToString();
            AttendedText.Text = stats.AttendedTrainings.ToString();
            AttendancePercentText.Text = $"{stats.AttendancePercent}%";
            AvgScoreText.Text = stats.AverageScore > 0 ? stats.AverageScore.ToString("0.0") : "—";

            BestStreakText.Text = $"{stats.BestStreak} подряд";
            CurrentStreakText.Text = $"{stats.CurrentStreak} подряд";

            PreferredTypesList.ItemsSource = stats.PreferredTrainingTypes.Any()
                ? stats.PreferredTrainingTypes
                : new System.Collections.Generic.List<string> { "Нет данных" };
        }
        #endregion
    }
}
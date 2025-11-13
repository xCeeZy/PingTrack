using PingTrack.AppData;
using PingTrack.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PingTrack.View.Pages
{
    public partial class PlayerDashboardPage : Page
    {
        #region Поля
        private readonly string userName;
        private Players currentPlayer;
        #endregion

        #region Конструктор
        public PlayerDashboardPage(string login)
        {
            InitializeComponent();
            userName = login;
            LoadPlayerData();
        }
        #endregion

        #region Загрузка данных игрока
        private void LoadPlayerData()
        {
            // Найти пользователя по логину
            Users user = App.db.Users.FirstOrDefault(u => u.Login == userName);
            if (user == null)
            {
                Feedback.ShowError("Ошибка", "Пользователь не найден.");
                return;
            }

            // Найти игрока по ID пользователя
            currentPlayer = App.db.Players
                .Include("Groups")
                .FirstOrDefault(p => p.ID_User == user.ID_User);

            if (currentPlayer == null)
            {
                Feedback.ShowWarning("Внимание", "Профиль игрока не создан. Обратитесь к администратору.");
                PlayerFullNameText.Text = user.Full_Name;
                PlayerGroupText.Text = "Группа: Не назначена";
                return;
            }

            // Отобразить информацию о игроке
            PlayerFullNameText.Text = currentPlayer.Full_Name;
            PlayerGroupText.Text = currentPlayer.Groups != null
                ? $"Группа: {currentPlayer.Groups.Group_Name}"
                : "Группа: Не назначена";

            // Загрузить статистику
            LoadStatistics();
            LoadMyTrainings();
            LoadAttendanceChart();
        }

        private void LoadStatistics()
        {
            if (currentPlayer == null)
                return;

            // Получить все записи посещаемости игрока
            List<Attendance> playerAttendance = App.db.Attendance
                .Include("Trainings")
                .Where(a => a.ID_Player == currentPlayer.ID_Player)
                .ToList();

            int totalTrainings = playerAttendance.Count;
            int presentCount = playerAttendance.Count(a => a.Is_Present);

            // Процент посещаемости
            double attendancePercent = totalTrainings > 0
                ? (double)presentCount / totalTrainings * 100.0
                : 0;
            AttendancePercentText.Text = string.Format("{0:F1}%", attendancePercent);

            // Всего тренировок
            TotalTrainingsText.Text = presentCount.ToString();

            // Средняя оценка
            List<int> scores = playerAttendance
                .Where(a => a.Score.HasValue)
                .Select(a => a.Score.Value)
                .ToList();

            if (scores.Count > 0)
            {
                double averageScore = scores.Average();
                AverageScoreText.Text = string.Format("{0:F1}", averageScore);
            }
            else
            {
                AverageScoreText.Text = "-";
            }
        }

        private void LoadMyTrainings()
        {
            if (currentPlayer == null)
                return;

            List<PlayerTrainingItem> trainings = App.db.Attendance
                .Include("Trainings")
                .Include("Trainings.Training_Types")
                .Include("Trainings.Users")
                .Where(a => a.ID_Player == currentPlayer.ID_Player)
                .ToList()
                .OrderByDescending(a => a.Trainings != null ? a.Trainings.Date : DateTime.MinValue)
                .ThenByDescending(a => a.Trainings != null ? a.Trainings.Time : TimeSpan.Zero)
                .Take(15)
                .Select(a => new PlayerTrainingItem
                {
                    DateTime = a.Trainings != null
                        ? string.Format("{0:dd.MM.yyyy} {1:hh\\:mm}", a.Trainings.Date, a.Trainings.Time)
                        : "-",
                    Type = a.Trainings != null && a.Trainings.Training_Types != null
                        ? a.Trainings.Training_Types.Type_Name
                        : "-",
                    Coach = a.Trainings != null && a.Trainings.Users != null
                        ? a.Trainings.Users.Full_Name
                        : "-",
                    Presence = a.Is_Present ? "✓ Был" : "✗ Не был",
                    PresenceColor = a.Is_Present ? "#27AE60" : "#E74C3C",
                    PresenceBg = a.Is_Present ? "#E8F5E9" : "#FFEBEE",
                    Score = a.Score.HasValue ? a.Score.Value.ToString() : "-"
                })
                .ToList();

            MyTrainingsGrid.ItemsSource = trainings;
        }

        private void LoadAttendanceChart()
        {
            if (currentPlayer == null)
                return;

            List<AttendanceChartItem> chartData = new List<AttendanceChartItem>();
            DateTime now = DateTime.Now;

            for (int i = 5; i >= 0; i--)
            {
                DateTime monthDate = now.AddMonths(-i);
                DateTime startOfMonth = new DateTime(monthDate.Year, monthDate.Month, 1);
                DateTime endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

                List<Attendance> monthAttendances = App.db.Attendance
                    .Include("Trainings")
                    .Where(a => a.ID_Player == currentPlayer.ID_Player &&
                                a.Trainings.Date >= startOfMonth &&
                                a.Trainings.Date <= endOfMonth)
                    .ToList();

                int totalCount = monthAttendances.Count;
                int presentCount = monthAttendances.Count(a => a.Is_Present);
                double percent = totalCount > 0 ? (double)presentCount / totalCount * 100.0 : 0;

                string monthName = monthDate.ToString("MMMM yyyy", new CultureInfo("ru-RU"));
                monthName = char.ToUpper(monthName[0]) + monthName.Substring(1);

                double maxWidth = 600.0;
                double barWidth = percent > 0 ? (percent / 100.0) * maxWidth : 1;

                chartData.Add(new AttendanceChartItem
                {
                    MonthName = monthName,
                    PercentText = string.Format("{0:F1}%", percent),
                    DetailText = string.Format("Посещено: {0} из {1} тренировок", presentCount, totalCount),
                    BarWidth = barWidth
                });
            }

            AttendanceChartItems.ItemsSource = chartData;
        }
        #endregion

        #region Обработчики событий
        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadPlayerData();
            Feedback.ShowInfo("Обновление", "Статистика обновлена.");
        }
        #endregion
    }

    #region Вспомогательные классы для отображения
    public class PlayerTrainingItem
    {
        public string DateTime { get; set; }
        public string Type { get; set; }
        public string Coach { get; set; }
        public string Presence { get; set; }
        public string PresenceColor { get; set; }
        public string PresenceBg { get; set; }
        public string Score { get; set; }
    }

    public class AttendanceChartItem
    {
        public string MonthName { get; set; }
        public string PercentText { get; set; }
        public string DetailText { get; set; }
        public double BarWidth { get; set; }
    }
    #endregion
}

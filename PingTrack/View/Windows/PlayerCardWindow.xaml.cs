using PingTrack.AppData;
using PingTrack.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace PingTrack.View.Windows
{
    public partial class PlayerCardWindow : Window
    {
        #region Поля
        private readonly int playerId;
        #endregion

        #region Конструктор
        public PlayerCardWindow(int selectedPlayerId)
        {
            InitializeComponent();
            playerId = selectedPlayerId;
            LoadPlayerCard();
        }
        #endregion

        #region Загрузка карточки игрока
        private void LoadPlayerCard()
        {
            Players player = App.db.Players
                .Include("Groups")
                .FirstOrDefault(p => p.ID_Player == playerId);

            if (player == null)
            {
                Feedback.ShowError("Ошибка", "Игрок не найден.");
                Close();
                return;
            }

            PlayerNameText.Text = player.Full_Name;
            GroupNameText.Text = $"Группа: {player.Groups?.Group_Name ?? "-"}";
            InitialText.Text = string.IsNullOrWhiteSpace(player.Full_Name) ? "?" : player.Full_Name.Substring(0, 1).ToUpper();
            AgeText.Text = $"{CalculateAge(player.Birth_Date)} лет";
            PhoneText.Text = string.IsNullOrWhiteSpace(player.Phone) ? "-" : player.Phone.Trim();
            MedicalText.Text = player.Medical_Clearance_Date.HasValue
                ? player.Medical_Clearance_Date.Value.ToString("dd.MM.yyyy")
                : "Не указана";

            LoadAttendanceStatistics(player);
            ActionLogService.LogView("Players", player.ID_Player, $"Открыта карточка игрока: {player.Full_Name}.");
        }

        private void LoadAttendanceStatistics(Players player)
        {
            List<Attendance> attendances = App.db.Attendance
                .Include("Trainings")
                .Include("Trainings.Training_Types")
                .Where(a => a.ID_Player == player.ID_Player
                         && a.IsDeleted == false
                         && a.Trainings.IsDeleted == false)
                .OrderByDescending(a => a.Trainings.Date)
                .ToList();

            int total = attendances.Count;
            int present = attendances.Count(a => a.Is_Present);
            int absent = total - present;
            double percent = total > 0 ? Math.Round(present * 100.0 / total, 1) : 0;

            TotalTrainingsText.Text = total.ToString();
            PresentText.Text = present.ToString();
            AbsentText.Text = absent.ToString();
            PercentText.Text = $"{percent}%";

            LastAttendanceGrid.ItemsSource = attendances
                .Take(10)
                .Select(a => new PlayerCardAttendanceItem
                {
                    Date = a.Trainings.Date.ToString("dd.MM.yyyy"),
                    TrainingType = a.Trainings.Training_Types?.Type_Name ?? "-",
                    Status = a.Is_Present ? "Присутствовал" : "Отсутствовал"
                })
                .ToList();
        }
        #endregion

        #region Вспомогательные методы
        private int CalculateAge(DateTime birthDate)
        {
            DateTime today = DateTime.Today;
            int age = today.Year - birthDate.Year;

            if (birthDate.Date > today.AddYears(-age))
                age--;

            return age;
        }
        #endregion

        #region Обработчики событий
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        #endregion
    }

    #region Вспомогательные классы
    public class PlayerCardAttendanceItem
    {
        public string Date { get; set; }
        public string TrainingType { get; set; }
        public string Status { get; set; }
    }
    #endregion
}
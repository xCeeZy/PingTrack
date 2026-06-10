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
    public partial class GroupAttendanceWindow : Window
    {
        #region Поля

        private readonly int trainingId;
        private Trainings currentTraining;
        private List<GroupAttendanceItem> attendanceItems;

        #endregion

        #region Конструктор

        public GroupAttendanceWindow(int selectedTrainingId)
        {
            InitializeComponent();

            trainingId = selectedTrainingId;
            attendanceItems = new List<GroupAttendanceItem>();

            LoadTraining();
            LoadAttendanceItems();
        }

        #endregion

        #region Загрузка данных

        private void LoadTraining()
        {
            currentTraining = App.db.Trainings
                .Include("Groups")
                .Include("Users")
                .Include("Training_Types")
                .FirstOrDefault(t => t.ID_Training == trainingId && t.IsDeleted == false);

            if (currentTraining == null)
            {
                Feedback.ShowError("Ошибка", "Выбранная тренировка не найдена.");
                DialogResult = false;
                Close();
                return;
            }

            DateTextBlock.Text = currentTraining.Date.ToString("dd.MM.yyyy");
            TimeTextBlock.Text = currentTraining.Time.ToString(@"hh\:mm");
            GroupTextBlock.Text = currentTraining.Groups != null ? currentTraining.Groups.Group_Name : "-";
            TypeTextBlock.Text = currentTraining.Training_Types != null ? currentTraining.Training_Types.Type_Name : "-";
        }

        private void LoadAttendanceItems()
        {
            if (currentTraining == null)
                return;

            List<Players> players = App.db.Players
                .Where(p => p.ID_Group == currentTraining.ID_Group && p.IsDeleted == false)
                .OrderBy(p => p.Full_Name)
                .ToList();

            List<Attendance> existingRecords = App.db.Attendance
                .Where(a => a.ID_Training == currentTraining.ID_Training)
                .ToList();

            attendanceItems = players.Select(player =>
            {
                Attendance existing = existingRecords.FirstOrDefault(a => a.ID_Player == player.ID_Player);

                return new GroupAttendanceItem
                {
                    PlayerId = player.ID_Player,
                    AttendanceId = existing != null ? (int?)existing.ID_Record : null,
                    PlayerName = player.Full_Name,
                    IsPresent = existing != null && existing.IsDeleted == false && existing.Is_Present,
                    Score = existing != null && existing.IsDeleted == false ? existing.Score : null,
                    RecordStatus = existing != null && existing.IsDeleted == false ? "Есть запись" : "Новая запись"
                };
            }).ToList();

            AttendanceDataGrid.ItemsSource = attendanceItems;
            CountTextBlock.Text = $"Игроков: {attendanceItems.Count}";
        }

        #endregion

        #region Массовые действия

        private void MarkAllPresentButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (GroupAttendanceItem item in attendanceItems)
                item.IsPresent = true;

            AttendanceDataGrid.Items.Refresh();
        }

        private void ClearAllButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (GroupAttendanceItem item in attendanceItems)
            {
                item.IsPresent = false;
                item.Score = null;
            }

            AttendanceDataGrid.Items.Refresh();
        }

        #endregion

        #region Сохранение

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentTraining == null)
                return;

            AttendanceDataGrid.CommitEdit();
            AttendanceDataGrid.CommitEdit(System.Windows.Controls.DataGridEditingUnit.Row, true);

            if (!ValidateScores())
                return;

            try
            {
                foreach (GroupAttendanceItem item in attendanceItems)
                    SaveAttendanceItem(item);

                App.db.SaveChanges();

                ActionLogService.LogUpdate(
                    "Attendance",
                    currentTraining.ID_Training,
                    $"Обновлена посещаемость по тренировке: {currentTraining.Date:dd.MM.yyyy}, группа: {GroupTextBlock.Text}.");

                Feedback.ShowSuccess("Успешно", "Посещаемость группы успешно сохранена.");
                DialogResult = true;
            }
            catch (Exception ex)
            {
                Feedback.ShowError("Ошибка сохранения", $"Не удалось сохранить посещаемость.\n\n{ex.Message}");
            }
        }

        private void SaveAttendanceItem(GroupAttendanceItem item)
        {
            Attendance record = App.db.Attendance.FirstOrDefault(a =>
                a.ID_Player == item.PlayerId &&
                a.ID_Training == currentTraining.ID_Training);

            if (record == null)
            {
                record = new Attendance
                {
                    ID_Player = item.PlayerId,
                    ID_Training = currentTraining.ID_Training
                };

                App.db.Attendance.Add(record);
            }

            record.Is_Present = item.IsPresent;
            record.Score = item.Score;
            record.IsDeleted = false;
        }

        private bool ValidateScores()
        {
            foreach (GroupAttendanceItem item in attendanceItems)
            {
                if (item.Score.HasValue && (item.Score.Value < 1 || item.Score.Value > 5))
                {
                    Feedback.ShowWarning(
                        "Ошибка",
                        $"Оценка у игрока \"{item.PlayerName}\" должна быть от 1 до 5.");

                    return false;
                }
            }

            return true;
        }

        #endregion

        #region Закрытие

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        #endregion
    }

    #region Модели отображения

    public class GroupAttendanceItem
    {
        public int PlayerId { get; set; }
        public int? AttendanceId { get; set; }
        public string PlayerName { get; set; }
        public bool IsPresent { get; set; }
        public int? Score { get; set; }
        public string RecordStatus { get; set; }
    }

    #endregion
}
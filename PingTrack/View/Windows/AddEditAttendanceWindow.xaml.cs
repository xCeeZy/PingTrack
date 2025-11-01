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
    public partial class AddEditAttendanceWindow : Window
    {
        #region Поля
        private Attendance currentAttendance;
        private readonly string userRole;
        #endregion

        #region Конструкторы
        public AddEditAttendanceWindow(string role) : this(role, null) { }

        public AddEditAttendanceWindow(string role, Attendance attendance)
        {
            InitializeComponent();
            userRole = role;
            currentAttendance = attendance;

            PlayerComboBox.ItemsSource = App.db.Players.ToList();

            TrainingComboBox.ItemsSource = App.db.Trainings
    .ToList()
    .Select(t => new
    {
        t.ID_Training,
        TrainingDisplay = t.Date.ToString("dd.MM.yyyy") + " " +
                          t.Time.ToString(@"hh\:mm") + " (" +
                          (t.Groups != null ? t.Groups.Group_Name : "-") + ")"
    })
    .ToList();

            if (attendance != null)
            {
                Title = "Редактирование отметки";
                PlayerComboBox.SelectedValue = attendance.ID_Player;
                TrainingComboBox.SelectedValue = attendance.ID_Training;
                IsPresentCheckBox.IsChecked = attendance.Is_Present;
                ScoreBox.Text = attendance.Score.HasValue ? attendance.Score.Value.ToString() : string.Empty;
            }
        }
        #endregion

        #region Проверка роли
        public bool CanOpenForRole()
        {
            if (userRole == "Студент")
            {
                Feedback.ShowWarning("Доступ запрещён", "У студентов нет прав для изменения посещаемости.");
                return false;
            }
            return true;
        }
        #endregion

        #region Сохранение данных
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (PlayerComboBox.SelectedValue == null || TrainingComboBox.SelectedValue == null)
            {
                Feedback.ShowWarning("Ошибка", "Заполните все обязательные поля.");
                return;
            }

            int playerId = (int)PlayerComboBox.SelectedValue;
            int trainingId = (int)TrainingComboBox.SelectedValue;

            Attendance existing = App.db.Attendance.FirstOrDefault(a => a.ID_Player == playerId && a.ID_Training == trainingId);
            if (existing != null && currentAttendance == null)
            {
                Feedback.ShowError("Ошибка", "Этот игрок уже отмечен на выбранной тренировке.");
                return;
            }

            bool isPresent = IsPresentCheckBox.IsChecked == true;
            int? score = null;
            int parsed;
            if (int.TryParse(ScoreBox.Text, out parsed)) score = parsed;

            if (currentAttendance == null)
            {
                currentAttendance = new Attendance
                {
                    ID_Player = playerId,
                    ID_Training = trainingId,
                    Is_Present = isPresent,
                    Score = score
                };
                App.db.Attendance.Add(currentAttendance);
            }
            else
            {
                currentAttendance.ID_Player = playerId;
                currentAttendance.ID_Training = trainingId;
                currentAttendance.Is_Present = isPresent;
                currentAttendance.Score = score;
            }

            try
            {
                App.db.SaveChanges();
                Feedback.ShowSuccess("Успешно", "Изменения успешно сохранены.");
                DialogResult = true;
            }
            catch (Exception)
            {
                Feedback.ShowError("Ошибка", "Не удалось сохранить изменения. Проверьте данные.");
            }
        }
        #endregion

        #region Отмена
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
        #endregion
    }
}
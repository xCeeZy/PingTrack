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
    public partial class AddEditTrainingWindow : Window
    {
        #region Поля
        private Trainings currentTraining;
        #endregion

        #region Конструктор
        public AddEditTrainingWindow(Trainings training = null)
        {
            InitializeComponent();
            currentTraining = training;

            GroupComboBox.ItemsSource = App.db.Groups.ToList();
            TypeComboBox.ItemsSource = App.db.Training_Types.ToList();
            CoachComboBox.ItemsSource = App.db.Users
                .Where(u => u.Roles.Role_Name == "Тренер")
                .ToList();

            if (training != null)
            {
                Title = "Редактирование занятия";
                DatePickerField.SelectedDate = training.Date;
                TimeBox.Text = training.Time.ToString(@"hh\\:mm");
                GroupComboBox.SelectedValue = training.ID_Group;
                TypeComboBox.SelectedValue = training.ID_Type;
                CoachComboBox.SelectedValue = training.ID_Coach;
                NoteBox.Text = training.Note;
            }
        }
        #endregion

        #region Сохранение данных
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (DatePickerField.SelectedDate == null ||
                string.IsNullOrWhiteSpace(TimeBox.Text) ||
                GroupComboBox.SelectedValue == null ||
                TypeComboBox.SelectedValue == null ||
                CoachComboBox.SelectedValue == null)
            {
                Feedback.ShowWarning("Ошибка", "Заполните все обязательные поля.");
                return;
            }

            TimeSpan parsedTime;
            bool isValidTime = TimeSpan.TryParse(TimeBox.Text, out parsedTime);
            if (!isValidTime)
            {
                Feedback.ShowError("Ошибка", "Некорректный формат времени. Используйте ЧЧ:ММ.");
                return;
            }

            if (currentTraining == null)
            {
                currentTraining = new Trainings
                {
                    Date = DatePickerField.SelectedDate.Value,
                    Time = parsedTime,
                    ID_Group = (int)GroupComboBox.SelectedValue,
                    ID_Type = (int)TypeComboBox.SelectedValue,
                    ID_Coach = (int)CoachComboBox.SelectedValue,
                    Note = NoteBox.Text.Trim()
                };
                App.db.Trainings.Add(currentTraining);
            }
            else
            {
                currentTraining.Date = DatePickerField.SelectedDate.Value;
                currentTraining.Time = parsedTime;
                currentTraining.ID_Group = (int)GroupComboBox.SelectedValue;
                currentTraining.ID_Type = (int)TypeComboBox.SelectedValue;
                currentTraining.ID_Coach = (int)CoachComboBox.SelectedValue;
                currentTraining.Note = NoteBox.Text.Trim();
            }

            try
            {
                App.db.SaveChanges();
                Feedback.ShowSuccess("Успешно", "Информация о занятии сохранена.");
                DialogResult = true;
            }
            catch (Exception)
            {
                Feedback.ShowError("Ошибка", "Не удалось сохранить данные. Проверьте введённую информацию.");
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
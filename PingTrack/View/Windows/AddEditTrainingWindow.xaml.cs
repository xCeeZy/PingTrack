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
        private bool isEditMode;
        #endregion

        #region Конструктор
        public AddEditTrainingWindow(Trainings training = null)
        {
            InitializeComponent();
            currentTraining = training;
            isEditMode = training != null;

            LoadData();

            if (isEditMode)
            {
                Title = "Редактирование тренировки";
                TitleTextBlock.Text = "Редактирование тренировки";
                LoadTrainingData();
            }
            else
            {
                DatePickerField.SelectedDate = DateTime.Now;
                TimeBox.Text = "10:00";
            }
        }
        #endregion

        #region Загрузка данных
        private void LoadData()
        {
            GroupComboBox.ItemsSource = App.db.Groups
                .OrderBy(g => g.Group_Name)
                .ToList();

            TypeComboBox.ItemsSource = App.db.Training_Types
                .OrderBy(t => t.Type_Name)
                .ToList();

            CoachComboBox.ItemsSource = App.db.Users
                .Include("Roles")
                .Where(u => u.Roles.Role_Name == "Тренер")
                .OrderBy(u => u.Full_Name)
                .ToList();
        }

        private void LoadTrainingData()
        {
            DatePickerField.SelectedDate = currentTraining.Date;
            TimeBox.Text = currentTraining.Time.ToString(@"hh\:mm");
            GroupComboBox.SelectedValue = currentTraining.ID_Group;
            TypeComboBox.SelectedValue = currentTraining.ID_Type;
            CoachComboBox.SelectedValue = currentTraining.ID_Coach;
            NoteBox.Text = currentTraining.Note ?? string.Empty;
        }
        #endregion

        #region Валидация
        private bool ValidateInput()
        {
            if (DatePickerField.SelectedDate == null)
            {
                Feedback.ShowWarning("Ошибка валидации", "Выберите дату тренировки.");
                DatePickerField.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(TimeBox.Text))
            {
                Feedback.ShowWarning("Ошибка валидации", "Укажите время тренировки.");
                TimeBox.Focus();
                return false;
            }

            TimeSpan parsedTime;
            if (!TimeSpan.TryParse(TimeBox.Text, out parsedTime))
            {
                Feedback.ShowWarning("Ошибка валидации", "Некорректный формат времени.\nИспользуйте формат ЧЧ:ММ (например, 10:30).");
                TimeBox.Focus();
                return false;
            }

            if (parsedTime < TimeSpan.Zero || parsedTime >= TimeSpan.FromDays(1))
            {
                Feedback.ShowWarning("Ошибка валидации", "Время должно быть в диапазоне от 00:00 до 23:59.");
                TimeBox.Focus();
                return false;
            }

            if (GroupComboBox.SelectedValue == null)
            {
                Feedback.ShowWarning("Ошибка валидации", "Выберите группу.");
                GroupComboBox.Focus();
                return false;
            }

            if (TypeComboBox.SelectedValue == null)
            {
                Feedback.ShowWarning("Ошибка валидации", "Выберите тип занятия.");
                TypeComboBox.Focus();
                return false;
            }

            if (CoachComboBox.SelectedValue == null)
            {
                Feedback.ShowWarning("Ошибка валидации", "Выберите тренера.");
                CoachComboBox.Focus();
                return false;
            }

            return true;
        }
        #endregion

        #region Сохранение данных
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInput())
                return;

            TimeSpan parsedTime = TimeSpan.Parse(TimeBox.Text);

            try
            {
                if (isEditMode)
                {
                    currentTraining.Date = DatePickerField.SelectedDate.Value;
                    currentTraining.Time = parsedTime;
                    currentTraining.ID_Group = (int)GroupComboBox.SelectedValue;
                    currentTraining.ID_Type = (int)TypeComboBox.SelectedValue;
                    currentTraining.ID_Coach = (int)CoachComboBox.SelectedValue;
                    currentTraining.Note = string.IsNullOrWhiteSpace(NoteBox.Text) ? null : NoteBox.Text.Trim();
                }
                else
                {
                    currentTraining = new Trainings
                    {
                        Date = DatePickerField.SelectedDate.Value,
                        Time = parsedTime,
                        ID_Group = (int)GroupComboBox.SelectedValue,
                        ID_Type = (int)TypeComboBox.SelectedValue,
                        ID_Coach = (int)CoachComboBox.SelectedValue,
                        Note = string.IsNullOrWhiteSpace(NoteBox.Text) ? null : NoteBox.Text.Trim()
                    };
                    App.db.Trainings.Add(currentTraining);
                }

                App.db.SaveChanges();

                string message = isEditMode
                    ? "Информация о тренировке успешно обновлена."
                    : "Новая тренировка успешно добавлена в расписание.";

                Feedback.ShowSuccess("Успешно", message);
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                Feedback.ShowError("Ошибка сохранения",
                    $"Не удалось сохранить данные о тренировке.\n\n{ex.Message}");
            }
        }
        #endregion

        #region Отмена
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
        #endregion
    }
}
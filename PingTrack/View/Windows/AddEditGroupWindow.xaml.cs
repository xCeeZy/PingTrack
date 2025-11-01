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
    public partial class AddEditGroupWindow : Window
    {
        #region Поля
        private Groups currentGroup;
        #endregion

        #region Конструктор
        public AddEditGroupWindow(Groups selectedGroup = null)
        {
            InitializeComponent();

            CoachComboBox.ItemsSource = App.db.Users
                .Where(u => u.Roles.Role_Name == "Тренер")
                .ToList();

            LevelComboBox.ItemsSource = App.db.Levels.ToList();

            currentGroup = selectedGroup ?? new Groups();

            if (selectedGroup != null)
            {
                Title = "Редактирование группы";
                GroupNameBox.Text = currentGroup.Group_Name;
                CoachComboBox.SelectedValue = currentGroup.ID_Coach;
                LevelComboBox.SelectedValue = currentGroup.ID_Level;
            }
        }
        #endregion

        #region Сохранение данных
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(GroupNameBox.Text) ||
                CoachComboBox.SelectedValue == null ||
                LevelComboBox.SelectedValue == null)
            {
                Feedback.ShowWarning("Ошибка", "Заполните все обязательные поля.");
                return;
            }

            currentGroup.Group_Name = GroupNameBox.Text.Trim();
            currentGroup.ID_Coach = (int)CoachComboBox.SelectedValue;
            currentGroup.ID_Level = (int)LevelComboBox.SelectedValue;

            if (currentGroup.ID_Group == 0)
                App.db.Groups.Add(currentGroup);

            try
            {
                App.db.SaveChanges();
                Feedback.ShowSuccess("Успешно", "Информация о группе сохранена.");
                DialogResult = true;
            }
            catch (Exception)
            {
                Feedback.ShowError("Ошибка", "Не удалось сохранить данные. Проверьте корректность введённой информации.");
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
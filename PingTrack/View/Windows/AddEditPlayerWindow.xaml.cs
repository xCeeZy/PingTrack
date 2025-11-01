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
    public partial class AddEditPlayerWindow : Window
    {
        #region Поля
        private Players currentPlayer;
        #endregion

        #region Конструктор
        public AddEditPlayerWindow(Players selectedPlayer = null)
        {
            InitializeComponent();
            GroupComboBox.ItemsSource = App.db.Groups.ToList();
            currentPlayer = selectedPlayer ?? new Players();

            if (selectedPlayer != null)
            {
                Title = "Редактирование игрока";
                FullNameBox.Text = currentPlayer.Full_Name;
                BirthDatePicker.SelectedDate = currentPlayer.Birth_Date;
                PhoneBox.Text = currentPlayer.Phone;
                GroupComboBox.SelectedValue = currentPlayer.ID_Group;
            }
        }
        #endregion

        #region Сохранение данных
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(FullNameBox.Text) ||
                BirthDatePicker.SelectedDate == null ||
                GroupComboBox.SelectedValue == null)
            {
                Feedback.ShowWarning("Ошибка", "Заполните все обязательные поля.");
                return;
            }

            currentPlayer.Full_Name = FullNameBox.Text.Trim();
            currentPlayer.Birth_Date = BirthDatePicker.SelectedDate.Value;
            currentPlayer.Phone = PhoneBox.Text.Trim();
            currentPlayer.ID_Group = (int)GroupComboBox.SelectedValue;

            if (currentPlayer.ID_Player == 0)
                App.db.Players.Add(currentPlayer);

            try
            {
                App.db.SaveChanges();
                Feedback.ShowSuccess("Успешно", "Информация об игроке сохранена.");
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
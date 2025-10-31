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
        private Players currentPlayer;

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

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(FullNameBox.Text) ||
                BirthDatePicker.SelectedDate == null ||
                GroupComboBox.SelectedValue == null)
            {
                MessageBox.Show("Заполните все обязательные поля.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            currentPlayer.Full_Name = FullNameBox.Text.Trim();
            currentPlayer.Birth_Date = BirthDatePicker.SelectedDate.Value;
            currentPlayer.Phone = PhoneBox.Text.Trim();
            currentPlayer.ID_Group = (int)GroupComboBox.SelectedValue;

            if (currentPlayer.ID_Player == 0)
                App.db.Players.Add(currentPlayer);

            App.db.SaveChanges();
            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}


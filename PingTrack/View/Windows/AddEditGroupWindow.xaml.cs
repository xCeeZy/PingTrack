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
        private Groups currentGroup;

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

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(GroupNameBox.Text) ||
                CoachComboBox.SelectedValue == null ||
                LevelComboBox.SelectedValue == null)
            {
                MessageBox.Show("Заполните все поля.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            currentGroup.Group_Name = GroupNameBox.Text.Trim();
            currentGroup.ID_Coach = (int)CoachComboBox.SelectedValue;
            currentGroup.ID_Level = (int)LevelComboBox.SelectedValue;

            if (currentGroup.ID_Group == 0)
                App.db.Groups.Add(currentGroup);

            App.db.SaveChanges();
            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}

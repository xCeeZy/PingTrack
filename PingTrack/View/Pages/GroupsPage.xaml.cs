using PingTrack.View.Windows;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PingTrack.View.Pages
{
    public partial class GroupsPage : Page
    {
        public GroupsPage()
        {
            InitializeComponent();
            LoadGroups();
            GroupsDataGrid.MouseDoubleClick += GroupsDataGrid_MouseDoubleClick;
        }

        private void LoadGroups()
        {
            var groups = App.db.Groups
                .Select(g => new
                {
                    g.ID_Group,
                    Name = g.Group_Name,
                    Coach = g.Users.Login,
                    PlayerCount = App.db.Players.Count(p => p.ID_Group == g.ID_Group),
                    Level = g.Levels.Level_Name
                })
                .ToList();

            GroupsDataGrid.ItemsSource = groups;
        }

        private void AddGroupButton_Click(object sender, RoutedEventArgs e)
        {
            var window = new AddEditGroupWindow();
            if (window.ShowDialog() == true)
                LoadGroups();
        }

        private void DeleteGroupButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedItem = GroupsDataGrid.SelectedItem;
            if (selectedItem == null)
            {
                MessageBox.Show("Выберите группу для удаления.", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int id = (int)selectedItem.GetType().GetProperty("ID_Group").GetValue(selectedItem);

            var group = App.db.Groups.Find(id);
            if (group == null) return;

            if (MessageBox.Show($"Удалить группу {group.Group_Name}?",
                "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                App.db.Groups.Remove(group);
                App.db.SaveChanges();
                LoadGroups();
            }
        }

        private void GroupsDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var selectedItem = GroupsDataGrid.SelectedItem;
            if (selectedItem == null) return;

            int id = (int)selectedItem.GetType().GetProperty("ID_Group").GetValue(selectedItem);
            var group = App.db.Groups.Find(id);

            var window = new AddEditGroupWindow(group);
            if (window.ShowDialog() == true)
                LoadGroups();
        }
    }
}

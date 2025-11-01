using PingTrack.AppData;
using PingTrack.Model;
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
    public partial class UsersPage : Page
    {
        private List<Users> allUsers;
        private bool isInitialized = false;

        public UsersPage()
        {
            InitializeComponent();
            LoadRoles();
            LoadUsers();

            RoleFilter.SelectionChanged += RoleFilter_SelectionChanged;
            SearchBox.TextChanged += SearchBox_TextChanged;

            isInitialized = true;
        }

        private void LoadRoles()
        {
            List<Roles> roles = App.db.Roles.ToList();
            Roles allOption = new Roles { ID_Role = 0, Role_Name = "Все роли" };
            roles.Insert(0, allOption);
            RoleFilter.ItemsSource = roles;
            RoleFilter.DisplayMemberPath = "Role_Name";
            RoleFilter.SelectedIndex = 0;
        }

        private void LoadUsers()
        {
            allUsers = App.db.Users.Include("Roles").ToList();
            UsersDataGrid.ItemsSource = allUsers;
        }

        private void ApplyFilters()
        {
            if (!isInitialized) return;
            if (allUsers == null || RoleFilter == null) return;

            string searchText = SearchBox.Text?.Trim().ToLower() ?? string.Empty;
            Roles selectedRole = RoleFilter.SelectedItem as Roles;

            IEnumerable<Users> filtered = allUsers;

            if (selectedRole != null && selectedRole.ID_Role != 0)
                filtered = filtered.Where(u => u.ID_Role == selectedRole.ID_Role);

            if (!string.IsNullOrWhiteSpace(searchText) && searchText != "поиск по логину или фио")
                filtered = filtered.Where(u =>
                    (!string.IsNullOrEmpty(u.Login) && u.Login.ToLower().Contains(searchText)) ||
                    (!string.IsNullOrEmpty(u.Full_Name) && u.Full_Name.ToLower().Contains(searchText)));

            UsersDataGrid.ItemsSource = filtered.ToList();
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void SearchBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (SearchBox.Text == "Поиск по логину или ФИО")
            {
                SearchBox.Text = "";
                SearchBox.Foreground = Brushes.Black;
            }
        }

        private void SearchBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SearchBox.Text))
            {
                SearchBox.Text = "Поиск по логину или ФИО";
                SearchBox.Foreground = Brushes.Gray;
            }
        }

        private void RoleFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void AddUserButton_Click(object sender, RoutedEventArgs e)
        {
            AddEditUserWindow window = new AddEditUserWindow();
            if (window.ShowDialog() == true)
                LoadUsers();
        }

        private void EditUserButton_Click(object sender, RoutedEventArgs e)
        {
            Users selectedUser = UsersDataGrid.SelectedItem as Users;
            if (selectedUser == null)
            {
                Feedback.ShowWarning("Ошибка", "Выберите пользователя для редактирования.");
                return;
            }

            AddEditUserWindow window = new AddEditUserWindow(selectedUser);
            if (window.ShowDialog() == true)
                LoadUsers();
        }

        private void DeleteUserButton_Click(object sender, RoutedEventArgs e)
        {
            Users selectedUser = UsersDataGrid.SelectedItem as Users;
            if (selectedUser == null)
            {
                Feedback.ShowWarning("Ошибка", "Выберите пользователя для удаления.");
                return;
            }

            bool confirm = Feedback.AskQuestion("Подтверждение", $"Удалить пользователя {selectedUser.Login}?");
            if (!confirm) return;

            App.db.Users.Remove(selectedUser);
            App.db.SaveChanges();
            LoadUsers();
        }

        private void UsersDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Users selectedUser = UsersDataGrid.SelectedItem as Users;
            if (selectedUser == null) return;

            AddEditUserWindow window = new AddEditUserWindow(selectedUser);
            if (window.ShowDialog() == true)
                LoadUsers();
        }
    }
}
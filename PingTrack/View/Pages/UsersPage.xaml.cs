using PingTrack.AppData;
using PingTrack.Model;
using PingTrack.View.Windows;
using System;
using System.Collections.Generic;
using System.Globalization;
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
        #region Поля
        private List<Users> allUsers;
        private bool isInitialized = false;
        #endregion

        #region Конструктор
        public UsersPage()
        {
            InitializeComponent();
            LoadRoles();
            LoadUsers();
            isInitialized = true;
        }
        #endregion

        #region Загрузка данных
        private void LoadRoles()
        {
            List<Roles> roles = App.db.Roles.ToList();
            Roles allOption = new Roles { ID_Role = 0, Role_Name = "Все роли" };
            roles.Insert(0, allOption);
            RoleFilter.ItemsSource = roles;
            RoleFilter.DisplayMemberPath = "Role_Name";
            RoleFilter.SelectedIndex = 0;
            RoleFilter.SelectionChanged += RoleFilter_SelectionChanged;
        }

        private void LoadUsers()
        {
            allUsers = App.db.Users.Include("Roles").OrderBy(u => u.Login).ToList();
            ApplyFilters();
            UpdateCountDisplay();
        }

        private void UpdateCountDisplay()
        {
            int displayedCount = UsersDataGrid.Items.Count;
            int totalCount = allUsers.Count;

            if (displayedCount == totalCount)
                CountTextBlock.Text = $"Всего пользователей: {totalCount}";
            else
                CountTextBlock.Text = $"Показано: {displayedCount} из {totalCount}";
        }
        #endregion

        #region Фильтрация
        private void ApplyFilters()
        {
            if (!isInitialized || allUsers == null || RoleFilter == null)
                return;

            string searchText = SearchBox.Text?.Trim().ToLower() ?? string.Empty;
            Roles selectedRole = RoleFilter.SelectedItem as Roles;

            IEnumerable<Users> filtered = allUsers;

            if (selectedRole != null && selectedRole.ID_Role != 0)
                filtered = filtered.Where(u => u.ID_Role == selectedRole.ID_Role);

            if (!string.IsNullOrWhiteSpace(searchText) && searchText != "поиск по логину или фио")
            {
                filtered = filtered.Where(u =>
                    (!string.IsNullOrEmpty(u.Login) && u.Login.ToLower().Contains(searchText)) ||
                    (!string.IsNullOrEmpty(u.Full_Name) && u.Full_Name.ToLower().Contains(searchText)));
            }

            UsersDataGrid.ItemsSource = filtered.ToList();
            UpdateCountDisplay();
        }
        #endregion

        #region Обработчики событий поиска
        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void SearchBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (SearchBox.Text == "Поиск по логину или ФИО")
            {
                SearchBox.Text = "";
                SearchBox.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1F2937"));
            }
        }

        private void SearchBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SearchBox.Text))
            {
                SearchBox.Text = "Поиск по логину или ФИО";
                SearchBox.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6B7280"));
            }
        }

        private void RoleFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }
        #endregion

        #region Обработчики DataGrid
        private void UsersDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Users selectedUser = UsersDataGrid.SelectedItem as Users;
            bool hasSelection = selectedUser != null;

            EditUserButton.IsEnabled = hasSelection;
            DeleteUserButton.IsEnabled = hasSelection;

            if (hasSelection)
                SelectionInfoTextBlock.Text = $"Выбран: {selectedUser.Full_Name} ({selectedUser.Login})";
            else
                SelectionInfoTextBlock.Text = "Выберите пользователя для редактирования или удаления";
        }

        private void UsersDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Users selectedUser = UsersDataGrid.SelectedItem as Users;
            if (selectedUser == null)
                return;

            AddEditUserWindow window = new AddEditUserWindow(selectedUser);
            if (window.ShowDialog() == true)
                LoadUsers();
        }
        #endregion

        #region Обработчики кнопок
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
                Feedback.ShowWarning("Предупреждение", "Выберите пользователя для редактирования.");
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
                Feedback.ShowWarning("Предупреждение", "Выберите пользователя для удаления.");
                return;
            }

            if (selectedUser.ID_User == AuthenticationService.CurrentUser.ID_User)
            {
                Feedback.ShowError("Ошибка", "Вы не можете удалить свою учётную запись.");
                return;
            }

            bool confirm = Feedback.AskQuestion("Подтверждение удаления",
                $"Вы уверены, что хотите удалить пользователя \"{selectedUser.Full_Name}\" ({selectedUser.Login})?\n\nЭто действие нельзя отменить.");

            if (!confirm)
                return;

            try
            {
                App.db.Users.Remove(selectedUser);
                App.db.SaveChanges();
                Feedback.ShowSuccess("Успешно", "Пользователь успешно удалён.");
                LoadUsers();
            }
            catch (Exception ex)
            {
                Feedback.ShowError("Ошибка удаления", $"Не удалось удалить пользователя.\n\n{ex.Message}");
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadUsers();
            Feedback.ShowInfo("Обновление", "Список пользователей обновлён.");
        }
        #endregion
    }

    #region Конвертеры
    public class StatusColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isActive)
            {
                return isActive
                    ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#27AE60"))
                    : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E74C3C"));
            }
            return new SolidColorBrush(Colors.Gray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class StatusTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isActive)
                return isActive ? "Активен" : "Неактивен";

            return "Неизвестно";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    #endregion
}
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
    public partial class AddEditUserWindow : Window
    {
        #region Поля
        private Users currentUser;
        #endregion

        #region Конструкторы
        public AddEditUserWindow()
        {
            InitializeComponent();
            LoadRoles();
            currentUser = new Users();
        }

        public AddEditUserWindow(Users user)
        {
            InitializeComponent();
            LoadRoles();
            currentUser = user;
            Title = "Редактирование пользователя";
            LoginBox.Text = currentUser.Login;
            PasswordBox.Password = currentUser.Password;
            RoleComboBox.SelectedValue = currentUser.ID_Role;
            IsActiveCheckBox.IsChecked = currentUser.IsActive;
        }
        #endregion

        #region Загрузка ролей
        private void LoadRoles()
        {
            List<Roles> roles = App.db.Roles.ToList();
            RoleComboBox.ItemsSource = roles;
        }
        #endregion

        #region Кнопки
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            string login = LoginBox.Text.Trim();
            string password = PasswordBox.Password.Trim();
            object roleValue = RoleComboBox.SelectedValue;
            bool? isActive = IsActiveCheckBox.IsChecked;

            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password) || roleValue == null || !isActive.HasValue)
            {
                Feedback.ShowWarning("Ошибка", "Заполните все поля.");
                return;
            }

            Users sameLogin = App.db.Users.FirstOrDefault(u => u.Login == login);
            bool isNew = currentUser.ID_User == 0;

            if (isNew)
            {
                if (sameLogin != null)
                {
                    Feedback.ShowWarning("Ошибка", "Такой логин уже существует.");
                    return;
                }

                currentUser.Login = login;
                currentUser.Password = password;
                currentUser.ID_Role = (int)roleValue;
                currentUser.IsActive = isActive.Value;
                App.db.Users.Add(currentUser);
            }
            else
            {
                if (sameLogin != null && sameLogin.ID_User != currentUser.ID_User)
                {
                    Feedback.ShowWarning("Ошибка", "Такой логин уже существует.");
                    return;
                }

                currentUser.Login = login;
                currentUser.Password = password;
                currentUser.ID_Role = (int)roleValue;
                currentUser.IsActive = isActive.Value;
            }

            App.db.SaveChanges();
            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
        #endregion
    }
}

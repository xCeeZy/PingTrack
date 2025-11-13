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
        private Users currentUser;

        public AddEditUserWindow()
        {
            InitializeComponent();
            LoadRoles();
            currentUser = new Users();
            RoleComboBox.SelectionChanged += RoleComboBox_SelectionChanged;
        }

        public AddEditUserWindow(Users user)
        {
            InitializeComponent();
            LoadRoles();
            currentUser = user;
            Title = "Редактирование пользователя";

            LoginBox.Text = currentUser.Login;
            PasswordBox.Password = currentUser.Password;
            FullNameBox.Text = currentUser.Full_Name;
            RoleComboBox.SelectedValue = currentUser.ID_Role;
            IsActiveCheckBox.IsChecked = currentUser.IsActive;

            RoleComboBox.SelectionChanged += RoleComboBox_SelectionChanged;

            // Загрузить телефон если пользователь - игрок
            Players player = App.db.Players.FirstOrDefault(p => p.ID_User == currentUser.ID_User);
            if (player != null)
            {
                PhoneBox.Text = player.Phone ?? "";
            }

            UpdatePhoneFieldVisibility();
        }

        private void LoadRoles()
        {
            List<Roles> roles = App.db.Roles.ToList();
            RoleComboBox.ItemsSource = roles;
        }

        private void RoleComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            UpdatePhoneFieldVisibility();
        }

        private void UpdatePhoneFieldVisibility()
        {
            if (RoleComboBox.SelectedValue == null)
            {
                PhonePanel.Visibility = Visibility.Collapsed;
                return;
            }

            int selectedRoleId = (int)RoleComboBox.SelectedValue;
            Roles playerRole = App.db.Roles.FirstOrDefault(r => r.Role_Name == "Игрок");

            if (playerRole != null && selectedRoleId == playerRole.ID_Role)
            {
                PhonePanel.Visibility = Visibility.Visible;
            }
            else
            {
                PhonePanel.Visibility = Visibility.Collapsed;
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            string login = LoginBox.Text.Trim();
            string password = PasswordBox.Password.Trim();
            string fullName = FullNameBox.Text.Trim();
            object roleValue = RoleComboBox.SelectedValue;
            bool? isActive = IsActiveCheckBox.IsChecked;

            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password) ||
                string.IsNullOrWhiteSpace(fullName) || roleValue == null || !isActive.HasValue)
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
                currentUser.Full_Name = fullName;
                currentUser.ID_Role = (int)roleValue;
                currentUser.IsActive = isActive.Value;
                currentUser.Created_At = DateTime.Now;

                App.db.Users.Add(currentUser);
                App.db.SaveChanges();

                // Если создается пользователь с ролью "Игрок", создать запись в таблице Players
                Roles playerRole = App.db.Roles.FirstOrDefault(r => r.Role_Name == "Игрок");
                if (playerRole != null && currentUser.ID_Role == playerRole.ID_Role)
                {
                    // Получить первую доступную группу (ID_Group обязателен для Players)
                    Groups firstGroup = App.db.Groups.OrderBy(g => g.ID_Group).FirstOrDefault();
                    if (firstGroup != null)
                    {
                        string phone = PhoneBox.Text.Trim();

                        Players newPlayer = new Players
                        {
                            Full_Name = fullName,
                            ID_User = currentUser.ID_User,
                            ID_Group = firstGroup.ID_Group,
                            Birth_Date = DateTime.Now, // Значение по умолчанию, можно изменить позже
                            Phone = phone
                        };

                        App.db.Players.Add(newPlayer);
                        App.db.SaveChanges();
                    }
                    else
                    {
                        Feedback.ShowWarning("Предупреждение", "Игрок создан, но не добавлен в таблицу Players: не найдено ни одной группы.");
                    }
                }
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
                currentUser.Full_Name = fullName;
                currentUser.ID_Role = (int)roleValue;
                currentUser.IsActive = isActive.Value;

                // Обновить телефон в таблице Players если пользователь - игрок
                Roles playerRole = App.db.Roles.FirstOrDefault(r => r.Role_Name == "Игрок");
                if (playerRole != null && currentUser.ID_Role == playerRole.ID_Role)
                {
                    Players player = App.db.Players.FirstOrDefault(p => p.ID_User == currentUser.ID_User);
                    if (player != null)
                    {
                        string phone = PhoneBox.Text.Trim();
                        player.Phone = phone;
                        player.Full_Name = fullName; // Обновить также ФИО в Players
                    }
                }

                App.db.SaveChanges();
            }

            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
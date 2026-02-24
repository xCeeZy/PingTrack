using PingTrack.AppData;
using PingTrack.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using System.Windows;
using System.Windows.Controls;
using System.Data.Entity;
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

            if (currentUser.ID_User != 0)
            {
                Players player = App.db.Players.FirstOrDefault(p => p.ID_User == currentUser.ID_User);
                if (player != null)
                {
                    PhoneBox.Text = player.Phone ?? "";
                }
            }

            UpdatePhoneFieldVisibility();
        }

        private void LoadRoles()
        {
            List<Roles> roles = App.db.Roles.ToList();
            RoleComboBox.ItemsSource = roles;
        }

        private void RoleComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
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

            if (isNew && sameLogin != null)
            {
                Feedback.ShowWarning("Ошибка", "Такой логин уже существует.");
                return;
            }
            if (!isNew && sameLogin != null && sameLogin.ID_User != currentUser.ID_User)
            {
                Feedback.ShowWarning("Ошибка", "Такой логин уже существует.");
                return;
            }

            Roles playerRole = App.db.Roles.FirstOrDefault(r => r.Role_Name == "Игрок");
            bool isPlayerRole = playerRole != null && (int)roleValue == playerRole.ID_Role;

            if (isPlayerRole && !App.db.Groups.Any())
            {
                Feedback.ShowWarning("Ошибка", "Невозможно создать игрока: нет ни одной группы.");
                return;
            }

            using (TransactionScope transaction = new TransactionScope())
            {
                try
                {
                    currentUser.Login = login;
                    currentUser.Password = password;
                    currentUser.Full_Name = fullName;
                    currentUser.ID_Role = (int)roleValue;
                    currentUser.IsActive = isActive.Value;

                    if (isNew)
                    {
                        currentUser.Created_At = DateTime.Now;
                        App.db.Users.Add(currentUser);
                    }

                    App.db.SaveChanges(); // Сохраняем User, чтобы получить ID_User

                    if (isPlayerRole)
                    {
                        Players player = App.db.Players.FirstOrDefault(p => p.ID_User == currentUser.ID_User);
                        Groups defaultGroup = App.db.Groups.OrderBy(g => g.ID_Group).FirstOrDefault();

                        if (player == null)
                        {
                            // Если записи игрока не было (например, поменяли роль на Игрок), создаем ее
                            player = new Players
                            {
                                ID_User = currentUser.ID_User,
                                Full_Name = fullName,
                                Phone = PhoneBox.Text.Trim(),
                                ID_Group = defaultGroup.ID_Group,
                                Birth_Date = DateTime.Now
                            };
                            App.db.Players.Add(player);
                        }
                        else
                        {
                            // Если запись есть, просто обновляем
                            player.Full_Name = fullName;
                            player.Phone = PhoneBox.Text.Trim();
                        }

                        App.db.SaveChanges(); // Сохраняем Player
                    }

                    transaction.Complete();
                    Feedback.ShowSuccess("Успешно", "Данные сохранены.");
                    DialogResult = true;
                }
                catch (Exception ex)
                {
                    // Выбрасываем сломанный объект из памяти, чтобы он не сохранился при следующем нажатии
                    if (isNew)
                    {
                        App.db.Users.Remove(currentUser);
                    }

                    Feedback.ShowError("Ошибка", $"Не удалось сохранить: {ex.Message}");
                }
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
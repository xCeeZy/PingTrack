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
    public partial class GroupsPage : Page
    {
        #region Поля
        private List<GroupGridItem> allGroups;
        private bool isInitialized = false;
        #endregion

        #region Конструктор
        public GroupsPage()
        {
            InitializeComponent();
            InitializeFilters();
            LoadGroups();
            isInitialized = true;
        }
        #endregion

        #region Инициализация фильтров
        private void InitializeFilters()
        {
            List<Users> coaches = App.db.Users
                .Include("Roles")
                .Where(u => u.Roles.Role_Name == "Тренер")
                .OrderBy(u => u.Full_Name)
                .ToList();

            Users allCoachesOption = new Users { ID_User = 0, Full_Name = "Все тренеры" };
            coaches.Insert(0, allCoachesOption);
            CoachFilter.ItemsSource = coaches;
            CoachFilter.DisplayMemberPath = "Full_Name";
            CoachFilter.SelectedIndex = 0;
            CoachFilter.SelectionChanged += Filter_SelectionChanged;

            List<Levels> levels = App.db.Levels.OrderBy(l => l.Level_Name).ToList();
            Levels allLevelsOption = new Levels { ID_Level = 0, Level_Name = "Все уровни" };
            levels.Insert(0, allLevelsOption);
            LevelFilter.ItemsSource = levels;
            LevelFilter.DisplayMemberPath = "Level_Name";
            LevelFilter.SelectedIndex = 0;
            LevelFilter.SelectionChanged += Filter_SelectionChanged;
        }
        #endregion

        #region Загрузка данных
        private void LoadGroups()
        {
            allGroups = App.db.Groups
                .Include("Users")
                .Include("Levels")
                .Include("Players")
                .ToList()
                .OrderBy(g => g.Group_Name)
                .Select(g => new GroupGridItem
                {
                    ID_Group = g.ID_Group,
                    Name = g.Group_Name,
                    Coach = g.Users.Full_Name,
                    Level = g.Levels.Level_Name,
                    PlayerCount = g.Players.Count
                })
                .ToList();

            ApplyFilters();
        }

        private void UpdateCountDisplay()
        {
            int displayedCount = GroupsDataGrid.Items.Count;
            int totalCount = allGroups.Count;

            if (displayedCount == totalCount)
                CountTextBlock.Text = $"Всего групп: {totalCount}";
            else
                CountTextBlock.Text = $"Показано: {displayedCount} из {totalCount}";
        }
        #endregion

        #region Фильтрация
        private void ApplyFilters()
        {
            if (!isInitialized || allGroups == null)
                return;

            string searchText = SearchBox.Text?.Trim().ToLower() ?? string.Empty;
            Users selectedCoach = CoachFilter.SelectedItem as Users;
            Levels selectedLevel = LevelFilter.SelectedItem as Levels;

            IEnumerable<GroupGridItem> filtered = allGroups;

            if (selectedCoach != null && selectedCoach.ID_User != 0)
                filtered = filtered.Where(g => g.Coach == selectedCoach.Full_Name);

            if (selectedLevel != null && selectedLevel.ID_Level != 0)
                filtered = filtered.Where(g => g.Level == selectedLevel.Level_Name);

            if (!string.IsNullOrWhiteSpace(searchText) && searchText != "поиск по названию группы")
                filtered = filtered.Where(g => g.Name.ToLower().Contains(searchText));

            GroupsDataGrid.ItemsSource = filtered.ToList();
            UpdateCountDisplay();
        }
        #endregion

        #region Обработчики событий поиска и фильтров
        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void SearchBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (SearchBox.Text == "Поиск по названию группы")
            {
                SearchBox.Text = "";
                SearchBox.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1F2937"));
            }
        }

        private void SearchBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SearchBox.Text))
            {
                SearchBox.Text = "Поиск по названию группы";
                SearchBox.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6B7280"));
            }
        }

        private void Filter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }
        #endregion

        #region Обработчики DataGrid
        private void GroupsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            GroupGridItem selectedGroup = GroupsDataGrid.SelectedItem as GroupGridItem;
            bool hasSelection = selectedGroup != null;

            EditGroupButton.IsEnabled = hasSelection;
            DeleteGroupButton.IsEnabled = hasSelection;

            if (hasSelection)
                SelectionInfoTextBlock.Text = $"Выбрана группа: {selectedGroup.Name} (тренер: {selectedGroup.Coach})";
            else
                SelectionInfoTextBlock.Text = "Выберите группу для редактирования или удаления";
        }

        private void GroupsDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            GroupGridItem selectedItem = GroupsDataGrid.SelectedItem as GroupGridItem;
            if (selectedItem == null)
                return;

            Groups group = App.db.Groups.FirstOrDefault(g => g.ID_Group == selectedItem.ID_Group);
            if (group == null)
                return;

            AddEditGroupWindow window = new AddEditGroupWindow(group);
            if (window.ShowDialog() == true)
                LoadGroups();
        }
        #endregion

        #region Обработчики кнопок
        private void AddGroupButton_Click(object sender, RoutedEventArgs e)
        {
            AddEditGroupWindow window = new AddEditGroupWindow();
            if (window.ShowDialog() == true)
                LoadGroups();
        }

        private void EditGroupButton_Click(object sender, RoutedEventArgs e)
        {
            GroupGridItem selectedItem = GroupsDataGrid.SelectedItem as GroupGridItem;
            if (selectedItem == null)
            {
                Feedback.ShowWarning("Предупреждение", "Выберите группу для редактирования.");
                return;
            }

            Groups group = App.db.Groups.FirstOrDefault(g => g.ID_Group == selectedItem.ID_Group);
            if (group == null)
                return;

            AddEditGroupWindow window = new AddEditGroupWindow(group);
            if (window.ShowDialog() == true)
                LoadGroups();
        }

        private void DeleteGroupButton_Click(object sender, RoutedEventArgs e)
        {
            GroupGridItem selectedItem = GroupsDataGrid.SelectedItem as GroupGridItem;
            if (selectedItem == null)
            {
                Feedback.ShowWarning("Предупреждение", "Выберите группу для удаления.");
                return;
            }

            Groups group = App.db.Groups.FirstOrDefault(g => g.ID_Group == selectedItem.ID_Group);
            if (group == null)
                return;

            if (group.Players.Count > 0)
            {
                Feedback.ShowError("Ошибка удаления",
                    $"Невозможно удалить группу \"{group.Group_Name}\".\n\nВ группе есть игроки ({group.Players.Count} чел.). Сначала переместите всех игроков в другие группы.");
                return;
            }

            bool confirm = Feedback.AskQuestion("Подтверждение удаления",
                $"Вы уверены, что хотите удалить группу \"{group.Group_Name}\"?\n\nЭто действие нельзя отменить.");

            if (!confirm)
                return;

            try
            {
                App.db.Groups.Remove(group);
                App.db.SaveChanges();
                Feedback.ShowSuccess("Успешно", "Группа успешно удалена.");
                LoadGroups();
            }
            catch (Exception ex)
            {
                Feedback.ShowError("Ошибка удаления", $"Не удалось удалить группу.\n\n{ex.Message}");
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadGroups();
            Feedback.ShowInfo("Обновление", "Список групп обновлён.");
        }
        #endregion
    }

    #region Вспомогательный класс для отображения
    public class GroupGridItem
    {
        public int ID_Group { get; set; }
        public string Name { get; set; }
        public string Coach { get; set; }
        public string Level { get; set; }
        public int PlayerCount { get; set; }
    }
    #endregion
}
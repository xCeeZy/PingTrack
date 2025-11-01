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
    public partial class JournalPage : Page
    {
        #region Поля
        private readonly string userRole;
        private PaginationService<object> pagination;
        #endregion

        #region Конструктор
        public JournalPage(string role)
        {
            InitializeComponent();
            userRole = role;
            pagination = new PaginationService<object>(10);
            LoadJournal();
            JournalDataGrid.MouseDoubleClick += JournalDataGrid_MouseDoubleClick;
            AddRecordButton.Click += AddRecordButton_Click;
            DeleteRecordButton.Click += DeleteRecordButton_Click;
            NextPageButton.Click += NextPageButton_Click;
            PrevPageButton.Click += PrevPageButton_Click;

            if (userRole == "Студент")
            {
                AddRecordButton.IsEnabled = false;
                DeleteRecordButton.IsEnabled = false;
                AddRecordButton.Visibility = Visibility.Collapsed;
                DeleteRecordButton.Visibility = Visibility.Collapsed;
            }
        }
        #endregion

        #region Загрузка данных
        private void LoadJournal()
        {
            List<JournalGridItem> data = App.db.Attendance
                .Include("Trainings")
                .Include("Players")
                .Include("Trainings.Training_Types")
                .Include("Players.Groups")
                .ToList()
                .Select(a => new JournalGridItem
                {
                    ID_Record = a.ID_Record,
                    Date = a.Trainings.Date.ToString("dd.MM.yyyy"),
                    Player = a.Players.Full_Name,
                    Group = a.Players.Groups.Group_Name,
                    Training = a.Trainings.Training_Types.Type_Name,
                    IsPresent = a.Is_Present
                })
                .ToList();

            pagination.SetItems(data.Cast<object>().ToList());
            UpdatePage();
        }
        #endregion

        #region Навигация страниц
        private void UpdatePage()
        {
            List<object> current = pagination.GetCurrentPage();
            JournalDataGrid.ItemsSource = current;
            PageInfoText.Text = "Страница " + pagination.CurrentPage + " из " + pagination.TotalPages;
            PrevPageButton.IsEnabled = pagination.HasPreviousPage;
            NextPageButton.IsEnabled = pagination.HasNextPage;
        }

        private void NextPageButton_Click(object sender, RoutedEventArgs e)
        {
            pagination.NextPage();
            UpdatePage();
        }

        private void PrevPageButton_Click(object sender, RoutedEventArgs e)
        {
            pagination.PreviousPage();
            UpdatePage();
        }
        #endregion

        #region Добавление, редактирование, удаление
        private void AddRecordButton_Click(object sender, RoutedEventArgs e)
        {
            AddEditAttendanceWindow window = new AddEditAttendanceWindow(userRole);
            if (!window.CanOpenForRole()) return;

            if (window.ShowDialog() == true)
                LoadJournal();
        }

        private void JournalDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (JournalDataGrid.SelectedItem == null) return;

            JournalGridItem selected = JournalDataGrid.SelectedItem as JournalGridItem;
            if (selected == null) return;

            Attendance record = App.db.Attendance.FirstOrDefault(x => x.ID_Record == selected.ID_Record);
            if (record == null) return;

            AddEditAttendanceWindow window = new AddEditAttendanceWindow(userRole, record);
            if (!window.CanOpenForRole()) return;

            if (window.ShowDialog() == true)
                LoadJournal();
        }

        private void DeleteRecordButton_Click(object sender, RoutedEventArgs e)
        {
            if (JournalDataGrid.SelectedItem == null)
            {
                Feedback.ShowWarning("Ошибка", "Выберите запись для удаления.");
                return;
            }

            JournalGridItem selected = JournalDataGrid.SelectedItem as JournalGridItem;
            if (selected == null) return;

            Attendance record = App.db.Attendance.FirstOrDefault(x => x.ID_Record == selected.ID_Record);
            if (record == null) return;

            bool confirm = Feedback.AskQuestion("Подтверждение", "Удалить выбранную запись?");
            if (!confirm) return;

            App.db.Attendance.Remove(record);
            App.db.SaveChanges();
            Feedback.ShowInfo("Успех", "Запись успешно удалена.");
            LoadJournal();
        }
        #endregion
    }
}
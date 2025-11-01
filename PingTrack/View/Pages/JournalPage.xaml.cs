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
        private readonly string userRole;

        public JournalPage(string role)
        {
            InitializeComponent();
            userRole = role;
            LoadJournal();
            JournalDataGrid.MouseDoubleClick += JournalDataGrid_MouseDoubleClick;
            AddRecordButton.Click += AddRecordButton_Click;
            DeleteRecordButton.Click += DeleteRecordButton_Click;

            if (userRole == "Студент")
            {
                AddRecordButton.IsEnabled = false;
                DeleteRecordButton.IsEnabled = false;
                AddRecordButton.Visibility = Visibility.Collapsed;
                DeleteRecordButton.Visibility = Visibility.Collapsed;
            }
        }

        private void LoadJournal()
        {
            List<object> data = App.db.Attendance
                .Include("Trainings")
                .Include("Players")
                .Include("Trainings.Training_Types")
                .Include("Players.Groups")
                .ToList()
                .Select(a => new
                {
                    a.ID_Record,
                    Date = a.Trainings.Date.ToString("dd.MM.yyyy"),
                    Player = a.Players.Full_Name,
                    Group = a.Players.Groups.Group_Name,
                    Training = a.Trainings.Training_Types.Type_Name,
                    Presence = a.Is_Present ? "Присутствовал" : "Отсутствовал",
                    a.Score
                })
                .OrderByDescending(x => x.Date)
                .Cast<object>()
                .ToList();

            JournalDataGrid.ItemsSource = data;
        }

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

            dynamic selected = JournalDataGrid.SelectedItem;
            int id = selected.ID_Record;
            Attendance record = App.db.Attendance.FirstOrDefault(x => x.ID_Record == id);
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
                MessageBox.Show("Выберите запись для удаления.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            dynamic selected = JournalDataGrid.SelectedItem;
            int id = selected.ID_Record;
            Attendance record = App.db.Attendance.FirstOrDefault(x => x.ID_Record == id);
            if (record == null) return;

            if (MessageBox.Show("Удалить выбранную запись?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                App.db.Attendance.Remove(record);
                App.db.SaveChanges();
                LoadJournal();
            }
        }
    }
}

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
using PingTrack.View.Windows;

namespace PingTrack.View.Pages
{
    public sealed class TrainingGridItem
    {
        public int ID_Training { get; set; }
        public string Date { get; set; }
        public string Time { get; set; }
        public string Group { get; set; }
        public string Coach { get; set; }
        public string Type { get; set; }
    }

    public partial class TrainingsPage : Page
    {
        public TrainingsPage()
        {
            InitializeComponent();
            LoadTrainings();
            TrainingsDataGrid.MouseDoubleClick += TrainingsDataGrid_MouseDoubleClick;
            AddTrainingButton.Click += AddTrainingButton_Click;
            DeleteTrainingButton.Click += DeleteTrainingButton_Click;
        }

        private void LoadTrainings()
        {
            List<TrainingGridItem> data = App.db.Trainings
                .ToList()
                .Select(t => new TrainingGridItem
                {
                    ID_Training = t.ID_Training,
                    Date = t.Date.ToString("dd.MM.yyyy"),
                    Time = t.Time.ToString(@"hh\:mm"),
                    Group = t.Groups != null ? t.Groups.Group_Name : "-",
                    Coach = t.Users != null ? t.Users.Login : "-",
                    Type = t.Training_Types != null ? t.Training_Types.Type_Name : "-"
                })
                .ToList();

            TrainingsDataGrid.ItemsSource = data;
        }

        private void AddTrainingButton_Click(object sender, RoutedEventArgs e)
        {
            AddEditTrainingWindow window = new AddEditTrainingWindow();
            bool? result = window.ShowDialog();
            if (result == true) LoadTrainings();
        }

        private void TrainingsDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (TrainingsDataGrid.SelectedItem == null) return;

            TrainingGridItem selected = (TrainingGridItem)TrainingsDataGrid.SelectedItem;
            int id = selected.ID_Training;
            PingTrack.Model.Trainings training = App.db.Trainings.FirstOrDefault(x => x.ID_Training == id);
            if (training == null) return;

            AddEditTrainingWindow window = new AddEditTrainingWindow(training);
            bool? result = window.ShowDialog();
            if (result == true) LoadTrainings();
        }

        private void DeleteTrainingButton_Click(object sender, RoutedEventArgs e)
        {
            if (TrainingsDataGrid.SelectedItem == null)
            {
                MessageBox.Show("Выберите занятие для удаления.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            TrainingGridItem selected = (TrainingGridItem)TrainingsDataGrid.SelectedItem;
            int id = selected.ID_Training;
            PingTrack.Model.Trainings training = App.db.Trainings.FirstOrDefault(x => x.ID_Training == id);
            if (training == null) return;

            MessageBoxResult confirm = MessageBox.Show("Удалить выбранное занятие?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (confirm == MessageBoxResult.Yes)
            {
                App.db.Trainings.Remove(training);
                App.db.SaveChanges();
                LoadTrainings();
            }
        }
    }
}
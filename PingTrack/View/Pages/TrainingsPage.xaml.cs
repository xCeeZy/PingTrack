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
            TrainingsDataGrid.ItemsSource = App.db.Trainings
                .Select(t => new
                {
                    t.ID_Training,
                    Date = t.Date.ToString("dd.MM.yyyy"),
                    Time = t.Time.ToString(@"hh\:mm"),
                    Group = t.Groups.Group_Name,
                    Coach = t.Users.Login,
                    Type = t.Training_Types.Type_Name
                })
                .ToList();
        }

        private void AddTrainingButton_Click(object sender, RoutedEventArgs e)
        {
            var window = new AddEditTrainingWindow();
            if (window.ShowDialog() == true)
                LoadTrainings();
        }

        private void TrainingsDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (TrainingsDataGrid.SelectedItem is null) return;

            dynamic selected = TrainingsDataGrid.SelectedItem;
            int id = selected.ID_Training;
            var training = App.db.Trainings.FirstOrDefault(x => x.ID_Training == id);

            var window = new AddEditTrainingWindow(training);
            if (window.ShowDialog() == true)
                LoadTrainings();
        }

        private void DeleteTrainingButton_Click(object sender, RoutedEventArgs e)
        {
            if (TrainingsDataGrid.SelectedItem is null)
            {
                MessageBox.Show("Выберите занятие для удаления.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            dynamic selected = TrainingsDataGrid.SelectedItem;
            int id = selected.ID_Training;
            var training = App.db.Trainings.FirstOrDefault(x => x.ID_Training == id);

            if (training != null)
            {
                if (MessageBox.Show("Удалить выбранное занятие?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    App.db.Trainings.Remove(training);
                    App.db.SaveChanges();
                    LoadTrainings();
                }
            }
        }
    }
}

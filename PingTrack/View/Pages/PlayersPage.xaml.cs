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
    public partial class PlayersPage : Page
    {
        public PlayersPage()
        {
            InitializeComponent();
            LoadPlayers();
            PlayersDataGrid.MouseDoubleClick += PlayersDataGrid_MouseDoubleClick;
        }

        private void LoadPlayers()
        {
            PlayersDataGrid.ItemsSource = App.db.Players.ToList();
        }

        private void AddPlayerButton_Click(object sender, RoutedEventArgs e)
        {
            var window = new AddEditPlayerWindow();
            if (window.ShowDialog() == true)
                LoadPlayers();
        }

        private void DeletePlayerButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedPlayer = PlayersDataGrid.SelectedItem as Players;
            if (selectedPlayer == null)
            {
                MessageBox.Show("Выберите игрока для удаления.", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (MessageBox.Show($"Удалить игрока {selectedPlayer.Full_Name}?",
                "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                App.db.Players.Remove(selectedPlayer);
                App.db.SaveChanges();
                LoadPlayers();
            }
        }

        private void PlayersDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var selectedPlayer = PlayersDataGrid.SelectedItem as Players;
            if (selectedPlayer == null) return;

            var window = new AddEditPlayerWindow(selectedPlayer);
            if (window.ShowDialog() == true)
                LoadPlayers();
        }
    }
}

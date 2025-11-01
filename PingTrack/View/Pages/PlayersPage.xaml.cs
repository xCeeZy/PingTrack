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
    public partial class PlayersPage : Page
    {
        #region Поля
        private PaginationService<Players> pagination;
        #endregion

        #region Конструктор
        public PlayersPage()
        {
            InitializeComponent();
            pagination = new PaginationService<Players>(10);
            LoadPlayers();
            PlayersDataGrid.MouseDoubleClick += PlayersDataGrid_MouseDoubleClick;
            AddPlayerButton.Click += AddPlayerButton_Click;
            DeletePlayerButton.Click += DeletePlayerButton_Click;
            NextPageButton.Click += NextPageButton_Click;
            PrevPageButton.Click += PrevPageButton_Click;
        }
        #endregion

        #region Загрузка данных
        private void LoadPlayers()
        {
            List<Players> data = App.db.Players
                .Include("Groups")
                .OrderBy(p => p.Full_Name)
                .ToList();

            pagination.SetItems(data);
            UpdatePage();
        }
        #endregion

        #region Пагинация
        private void UpdatePage()
        {
            List<Players> current = pagination.GetCurrentPage();
            PlayersDataGrid.ItemsSource = current;
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
        private void AddPlayerButton_Click(object sender, RoutedEventArgs e)
        {
            AddEditPlayerWindow window = new AddEditPlayerWindow();
            if (window.ShowDialog() == true)
                LoadPlayers();
        }

        private void DeletePlayerButton_Click(object sender, RoutedEventArgs e)
        {
            Players selectedPlayer = PlayersDataGrid.SelectedItem as Players;
            if (selectedPlayer == null)
            {
                Feedback.ShowWarning("Ошибка", "Выберите игрока для удаления.");
                return;
            }

            bool confirm = Feedback.AskQuestion("Подтверждение", "Удалить игрока " + selectedPlayer.Full_Name + "?");
            if (!confirm) return;

            App.db.Players.Remove(selectedPlayer);
            App.db.SaveChanges();
            Feedback.ShowInfo("Успех", "Игрок успешно удалён.");
            LoadPlayers();
        }

        private void PlayersDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Players selectedPlayer = PlayersDataGrid.SelectedItem as Players;
            if (selectedPlayer == null) return;

            AddEditPlayerWindow window = new AddEditPlayerWindow(selectedPlayer);
            if (window.ShowDialog() == true)
                LoadPlayers();
        }
        #endregion
    }
}
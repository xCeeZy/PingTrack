using PingTrack.View.Pages;
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

namespace PingTrack.View.Windows
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            MainFrame.Navigate(new DashboardPage());
        }

        private void DashboardBtn_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new DashboardPage());
        }

        private void PlayersBtn_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Страница 'Игроки' пока не создана.");
        }

        private void GroupsBtn_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Страница 'Группы' пока не создана.");
        }

        private void TrainingsBtn_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Страница 'Занятия' пока не создана.");
        }

        private void JournalBtn_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Страница 'Журнал' пока не создана.");
        }

        private void ReportsBtn_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Страница 'Отчёты' пока не создана.");
        }
    }
}

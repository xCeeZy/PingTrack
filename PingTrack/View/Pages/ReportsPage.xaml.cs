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
    public partial class ReportsPage : Page
    {
        public ReportsPage()
        {
            InitializeComponent();
            LoadGroups();
            GenerateReportButton.Click += GenerateReportButton_Click;
        }

        private void LoadGroups()
        {
            GroupComboBox.ItemsSource = App.db.Groups.ToList();
            GroupComboBox.DisplayMemberPath = "Group_Name";
            GroupComboBox.SelectedValuePath = "ID_Group";
        }

        private void GenerateReportButton_Click(object sender, RoutedEventArgs e)
        {
            DateTime? startDate = StartDatePicker.SelectedDate;
            DateTime? endDate = EndDatePicker.SelectedDate;
            int? selectedGroupId = GroupComboBox.SelectedValue as int?;

            var query = App.db.Attendance.AsQueryable();

            if (selectedGroupId.HasValue)
                query = query.Where(a => a.Players.ID_Group == selectedGroupId.Value);

            if (startDate.HasValue)
                query = query.Where(a => a.Trainings.Date >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(a => a.Trainings.Date <= endDate.Value);

            var report = query
                .GroupBy(a => new
                {
                    Group = a.Players.Groups.Group_Name,
                    Player = a.Players.Full_Name
                })
                .Select(g => new
                {
                    Player = g.Key.Player,
                    Group = g.Key.Group,
                    TotalTrainings = g.Count(),
                    PresentCount = g.Count(x => x.Is_Present),
                    AttendancePercent = g.Count() == 0 ? 0 :
                        Math.Round(g.Count(x => x.Is_Present) * 100.0 / g.Count(), 1)
                })
                .OrderByDescending(x => x.AttendancePercent)
                .ThenBy(x => x.Player)
                .ToList();

            ReportsDataGrid.ItemsSource = report;

            if (report.Count == 0)
                MessageBox.Show("Нет данных за выбранные фильтры.", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
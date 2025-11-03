using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;

namespace PingTrack.AppData
{
    public static class DataGridHelper
    {
        #region Настройка колонок для отчётов
        public static void ConfigureColumnsForPlayerAttendance(DataGrid dataGrid)
        {
            dataGrid.Columns.Clear();
            dataGrid.Columns.Add(new DataGridTextColumn
            {
                Header = "ФИО игрока",
                Binding = new Binding("Player"),
                Width = new DataGridLength(1, DataGridLengthUnitType.Star),
                MinWidth = 200
            });
            dataGrid.Columns.Add(new DataGridTextColumn
            {
                Header = "Группа",
                Binding = new Binding("Group"),
                Width = 180
            });
            dataGrid.Columns.Add(new DataGridTextColumn
            {
                Header = "Всего занятий",
                Binding = new Binding("TotalTrainings"),
                Width = 130
            });
            dataGrid.Columns.Add(new DataGridTextColumn
            {
                Header = "Присутствовал",
                Binding = new Binding("PresentCount"),
                Width = 140
            });
            dataGrid.Columns.Add(new DataGridTextColumn
            {
                Header = "Отсутствовал",
                Binding = new Binding("AbsentCount"),
                Width = 140
            });
            dataGrid.Columns.Add(new DataGridTextColumn
            {
                Header = "Посещаемость",
                Binding = new Binding("AttendancePercent"),
                Width = 140
            });
        }

        public static void ConfigureColumnsForGroupAttendance(DataGrid dataGrid)
        {
            dataGrid.Columns.Clear();
            dataGrid.Columns.Add(new DataGridTextColumn
            {
                Header = "Группа",
                Binding = new Binding("Group"),
                Width = new DataGridLength(1, DataGridLengthUnitType.Star),
                MinWidth = 200
            });
            dataGrid.Columns.Add(new DataGridTextColumn
            {
                Header = "Всего занятий",
                Binding = new Binding("TotalTrainings"),
                Width = 180
            });
            dataGrid.Columns.Add(new DataGridTextColumn
            {
                Header = "Присутствий",
                Binding = new Binding("PresentCount"),
                Width = 180
            });
            dataGrid.Columns.Add(new DataGridTextColumn
            {
                Header = "Посещаемость",
                Binding = new Binding("AttendancePercent"),
                Width = 180
            });
        }

        public static void ConfigureColumnsForActivityRating(DataGrid dataGrid)
        {
            dataGrid.Columns.Clear();
            dataGrid.Columns.Add(new DataGridTextColumn
            {
                Header = "Место",
                Binding = new Binding("Place"),
                Width = 100
            });
            dataGrid.Columns.Add(new DataGridTextColumn
            {
                Header = "ФИО игрока",
                Binding = new Binding("Player"),
                Width = new DataGridLength(1, DataGridLengthUnitType.Star),
                MinWidth = 200
            });
            dataGrid.Columns.Add(new DataGridTextColumn
            {
                Header = "Группа",
                Binding = new Binding("Group"),
                Width = 200
            });
            dataGrid.Columns.Add(new DataGridTextColumn
            {
                Header = "Посещено тренировок",
                Binding = new Binding("TrainingsAttended"),
                Width = 180
            });
        }

        public static void ConfigureColumnsForGeneralStatistics(DataGrid dataGrid)
        {
            dataGrid.Columns.Clear();
            dataGrid.Columns.Add(new DataGridTextColumn
            {
                Header = "Тип тренировки",
                Binding = new Binding("TrainingType"),
                Width = new DataGridLength(1, DataGridLengthUnitType.Star),
                MinWidth = 200
            });
            dataGrid.Columns.Add(new DataGridTextColumn
            {
                Header = "Всего проведено",
                Binding = new Binding("TotalTrainings"),
                Width = 180
            });
            dataGrid.Columns.Add(new DataGridTextColumn
            {
                Header = "Всего посещений",
                Binding = new Binding("TotalAttendance"),
                Width = 180
            });
            dataGrid.Columns.Add(new DataGridTextColumn
            {
                Header = "Средняя посещаемость",
                Binding = new Binding("AverageAttendance"),
                Width = 200
            });
        }
        #endregion
    }
}
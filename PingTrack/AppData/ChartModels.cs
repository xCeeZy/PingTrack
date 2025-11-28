using System;

namespace PingTrack.AppData
{
    /// <summary>
    /// Модель данных для отображения графика посещаемости
    /// </summary>
    public class AttendanceChartItem
    {
        public string MonthName { get; set; }
        public string PercentText { get; set; }
        public string DetailText { get; set; }
        public double BarWidth { get; set; }
    }
}

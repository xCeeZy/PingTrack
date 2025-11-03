using PingTrack.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PingTrack.AppData
{
    public static class ReportService
    {
        #region Типы отчётов
        public static List<ReportType> GetReportTypes()
        {
            return new List<ReportType>
            {
                new ReportType { ID = 1, Name = "Посещаемость по игрокам" },
                new ReportType { ID = 2, Name = "Посещаемость по группам" },
                new ReportType { ID = 3, Name = "Рейтинг активности игроков" },
                new ReportType { ID = 4, Name = "Общая статистика тренировок" }
            };
        }
        #endregion

        #region Отчёт: Посещаемость по игрокам
        public static List<PlayerAttendanceReport> GeneratePlayerAttendanceReport(DateTime startDate, DateTime endDate, int? groupId)
        {
            IQueryable<Attendance> query = App.db.Attendance
                .Include("Players")
                .Include("Players.Groups")
                .Include("Trainings");

            if (groupId.HasValue && groupId.Value != 0)
                query = query.Where(a => a.Players.ID_Group == groupId.Value);

            query = query.Where(a => a.Trainings.Date >= startDate && a.Trainings.Date <= endDate);

            List<PlayerAttendanceReport> report = query
                .ToList()
                .GroupBy(a => new { a.ID_Player, PlayerName = a.Players.Full_Name, GroupName = a.Players.Groups.Group_Name })
                .Select(g => new PlayerAttendanceReport
                {
                    Player = g.Key.PlayerName,
                    Group = g.Key.GroupName,
                    TotalTrainings = g.Count(),
                    PresentCount = g.Count(x => x.Is_Present),
                    AbsentCount = g.Count(x => !x.Is_Present),
                    AttendancePercent = $"{(g.Count() == 0 ? 0 : Math.Round(g.Count(x => x.Is_Present) * 100.0 / g.Count(), 1))}%"
                })
                .OrderByDescending(x => double.Parse(x.AttendancePercent.TrimEnd('%')))
                .ThenBy(x => x.Player)
                .ToList();

            return report;
        }
        #endregion

        #region Отчёт: Посещаемость по группам
        public static List<GroupAttendanceReport> GenerateGroupAttendanceReport(DateTime startDate, DateTime endDate, int? groupId)
        {
            IQueryable<Attendance> query = App.db.Attendance
                .Include("Players")
                .Include("Players.Groups")
                .Include("Trainings");

            if (groupId.HasValue && groupId.Value != 0)
                query = query.Where(a => a.Players.ID_Group == groupId.Value);

            query = query.Where(a => a.Trainings.Date >= startDate && a.Trainings.Date <= endDate);

            List<GroupAttendanceReport> report = query
                .ToList()
                .GroupBy(a => a.Players.Groups.Group_Name)
                .Select(g => new GroupAttendanceReport
                {
                    Group = g.Key,
                    TotalTrainings = g.Count(),
                    PresentCount = g.Count(x => x.Is_Present),
                    AttendancePercent = $"{(g.Count() == 0 ? 0 : Math.Round(g.Count(x => x.Is_Present) * 100.0 / g.Count(), 1))}%"
                })
                .OrderByDescending(x => double.Parse(x.AttendancePercent.TrimEnd('%')))
                .ToList();

            return report;
        }
        #endregion

        #region Отчёт: Рейтинг активности
        public static List<ActivityRatingReport> GenerateActivityRatingReport(DateTime startDate, DateTime endDate, int? groupId)
        {
            IQueryable<Attendance> query = App.db.Attendance
                .Include("Players")
                .Include("Players.Groups")
                .Include("Trainings");

            if (groupId.HasValue && groupId.Value != 0)
                query = query.Where(a => a.Players.ID_Group == groupId.Value);

            query = query.Where(a => a.Trainings.Date >= startDate && a.Trainings.Date <= endDate);

            List<ActivityRatingReport> report = query
                .ToList()
                .Where(a => a.Is_Present)
                .GroupBy(a => new { a.ID_Player, PlayerName = a.Players.Full_Name, GroupName = a.Players.Groups.Group_Name })
                .Select(g => new ActivityRatingReport
                {
                    Place = 0,
                    Player = g.Key.PlayerName,
                    Group = g.Key.GroupName,
                    TrainingsAttended = g.Count()
                })
                .OrderByDescending(x => x.TrainingsAttended)
                .ThenBy(x => x.Player)
                .ToList();

            for (int i = 0; i < report.Count; i++)
                report[i].Place = i + 1;

            return report;
        }
        #endregion

        #region Отчёт: Общая статистика
        public static List<GeneralStatisticsReport> GenerateGeneralStatisticsReport(DateTime startDate, DateTime endDate)
        {
            List<Trainings> trainings = App.db.Trainings
                .Include("Groups")
                .Include("Training_Types")
                .Include("Attendance")
                .Where(t => t.Date >= startDate && t.Date <= endDate)
                .ToList();

            List<GeneralStatisticsReport> report = trainings
                .GroupBy(t => t.Training_Types.Type_Name)
                .Select(g => new GeneralStatisticsReport
                {
                    TrainingType = g.Key,
                    TotalTrainings = g.Count(),
                    TotalAttendance = g.Sum(t => t.Attendance.Count),
                    AverageAttendance = g.Average(t => t.Attendance.Count(a => a.Is_Present))
                })
                .OrderByDescending(x => x.TotalTrainings)
                .ToList();

            return report;
        }
        #endregion
    }

    #region Модели отчётов
    public class ReportType
    {
        public int ID { get; set; }
        public string Name { get; set; }
    }

    public class PlayerAttendanceReport
    {
        public string Player { get; set; }
        public string Group { get; set; }
        public int TotalTrainings { get; set; }
        public int PresentCount { get; set; }
        public int AbsentCount { get; set; }
        public string AttendancePercent { get; set; }
    }

    public class GroupAttendanceReport
    {
        public string Group { get; set; }
        public int TotalTrainings { get; set; }
        public int PresentCount { get; set; }
        public string AttendancePercent { get; set; }
    }

    public class ActivityRatingReport
    {
        public int Place { get; set; }
        public string Player { get; set; }
        public string Group { get; set; }
        public int TrainingsAttended { get; set; }
    }

    public class GeneralStatisticsReport
    {
        public string TrainingType { get; set; }
        public int TotalTrainings { get; set; }
        public int TotalAttendance { get; set; }
        public double AverageAttendance { get; set; }
    }
    #endregion
}

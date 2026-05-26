using PingTrack.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PingTrack.AppData
{
    public static class RiskAnalysisService
    {
        private const int DefaultDaysBack = 30;

        /// <summary>
        /// Получить единый список игроков в зоне риска для Dashboard, отчётов и статистики.
        /// </summary>
        public static List<PlayerRiskInfo> GetAtRiskPlayers(int daysBack = DefaultDaysBack, int? topCount = null)
        {
            try
            {
                DateTime endDate = DateTime.Now;
                DateTime startDate = endDate.AddDays(-daysBack);

                List<PlayerRiskInfo> riskList = new List<PlayerRiskInfo>();

                List<Players> players = App.db.Players
                    .Include("Groups")
                    .Where(p => p.IsDeleted == false)
                    .ToList();

                foreach (Players player in players)
                {
                    List<Trainings> groupTrainings = App.db.Trainings
                        .Where(t => t.ID_Group == player.ID_Group
                                 && t.Date >= startDate
                                 && t.Date <= endDate
                                 && t.IsDeleted == false)
                        .OrderByDescending(t => t.Date)
                        .ToList();

                    if (!groupTrainings.Any())
                        continue;

                    List<int> trainingIds = groupTrainings.Select(t => t.ID_Training).ToList();

                    List<Attendance> attendances = App.db.Attendance
                        .Include("Trainings")
                        .Where(a => a.ID_Player == player.ID_Player
                                 && trainingIds.Contains(a.ID_Training)
                                 && a.IsDeleted == false)
                        .OrderByDescending(a => a.Trainings.Date)
                        .ToList();

                    int totalTrainings = groupTrainings.Count;
                    int attendedCount = attendances.Count(a => a.Is_Present);
                    int missedCount = totalTrainings - attendedCount;
                    int missedInRow = CalculateMissedInRow(groupTrainings, attendances);

                    double attendancePercent = totalTrainings > 0
                        ? Math.Round(attendedCount * 100.0 / totalTrainings, 1)
                        : 0;

                    Attendance lastAttendance = attendances
                        .Where(a => a.Is_Present)
                        .OrderByDescending(a => a.Trainings.Date)
                        .FirstOrDefault();

                    DateTime? lastAttendanceDate = lastAttendance?.Trainings.Date;
                    int daysSinceLastVisit = lastAttendanceDate.HasValue
                        ? (endDate - lastAttendanceDate.Value).Days
                        : daysBack;

                    RiskDecision riskDecision = DetermineRisk(attendancePercent, missedCount, missedInRow, daysSinceLastVisit, lastAttendanceDate);

                    if (riskDecision.Level == RiskLevel.Low)
                        continue;

                    riskList.Add(new PlayerRiskInfo
                    {
                        PlayerId = player.ID_Player,
                        PlayerName = player.Full_Name,
                        GroupName = player.Groups?.Group_Name ?? "Без группы",
                        RiskLevel = riskDecision.DisplayName,
                        DaysSinceLastVisit = daysSinceLastVisit,
                        RecommendedAction = riskDecision.RecommendedAction,
                        MissedTrainings = missedCount,
                        MissedInRow = missedInRow,
                        LastAttendance = lastAttendanceDate,
                        AttendancePercent = attendancePercent
                    });
                }

                IEnumerable<PlayerRiskInfo> orderedRiskList = riskList
                    .OrderBy(r => GetRiskSortOrder(r.RiskLevel))
                    .ThenByDescending(r => r.DaysSinceLastVisit)
                    .ThenBy(r => r.AttendancePercent)
                    .ThenBy(r => r.PlayerName);

                if (topCount.HasValue)
                    orderedRiskList = orderedRiskList.Take(topCount.Value);

                return orderedRiskList.ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка в GetAtRiskPlayers: {ex.Message}");
                return new List<PlayerRiskInfo>();
            }
        }

        /// <summary>
        /// Совместимость со старым методом, который использовал модель PlayerAtRisk.
        /// </summary>
        public static List<PlayerAtRisk> GetPlayersAtRisk(int daysBack = DefaultDaysBack, int topCount = 5)
        {
            return GetAtRiskPlayers(daysBack, topCount)
                .Select(r => new PlayerAtRisk
                {
                    PlayerId = r.PlayerId,
                    PlayerName = r.PlayerName,
                    GroupName = r.GroupName,
                    MissedTrainings = r.MissedTrainings,
                    LastAttendance = r.LastAttendance,
                    AttendancePercent = r.AttendancePercent,
                    RiskLevel = NormalizeRiskLevel(r.RiskLevel)
                })
                .ToList();
        }

        private static int CalculateMissedInRow(List<Trainings> groupTrainings, List<Attendance> attendances)
        {
            int missedInRow = 0;

            foreach (Trainings training in groupTrainings.OrderByDescending(t => t.Date))
            {
                Attendance attendance = attendances.FirstOrDefault(a => a.ID_Training == training.ID_Training);

                if (attendance == null || !attendance.Is_Present)
                    missedInRow++;
                else
                    break;
            }

            return missedInRow;
        }

        private static RiskDecision DetermineRisk(double attendancePercent, int missedCount, int missedInRow, int daysSinceLastVisit, DateTime? lastAttendance)
        {
            if (!lastAttendance.HasValue)
            {
                return new RiskDecision
                {
                    Level = RiskLevel.High,
                    DisplayName = "Высокий",
                    RecommendedAction = "Срочно связаться"
                };
            }

            if (attendancePercent < 40 || missedCount >= 5 || missedInRow >= 5 || daysSinceLastVisit > 14)
            {
                return new RiskDecision
                {
                    Level = RiskLevel.High,
                    DisplayName = "Высокий",
                    RecommendedAction = "Срочная встреча"
                };
            }

            if (attendancePercent < 60 || missedCount >= 3 || missedInRow >= 3 || daysSinceLastVisit > 7)
            {
                return new RiskDecision
                {
                    Level = RiskLevel.Medium,
                    DisplayName = "Средний",
                    RecommendedAction = "Позвонить"
                };
            }

            return new RiskDecision
            {
                Level = RiskLevel.Low,
                DisplayName = "Низкий",
                RecommendedAction = "Мониторинг"
            };
        }

        private static int GetRiskSortOrder(string riskLevel)
        {
            string normalizedLevel = NormalizeRiskLevel(riskLevel);

            if (normalizedLevel == "Высокий")
                return 0;

            if (normalizedLevel == "Средний")
                return 1;

            return 2;
        }

        private static string NormalizeRiskLevel(string riskLevel)
        {
            if (string.IsNullOrWhiteSpace(riskLevel))
                return string.Empty;

            if (riskLevel.Contains("Высокий") || riskLevel.Contains("Критический"))
                return "Высокий";

            if (riskLevel.Contains("Средний"))
                return "Средний";

            if (riskLevel.Contains("Низкий"))
                return "Низкий";

            return riskLevel;
        }

        private enum RiskLevel
        {
            Low,
            Medium,
            High
        }

        private class RiskDecision
        {
            public RiskLevel Level { get; set; }
            public string DisplayName { get; set; }
            public string RecommendedAction { get; set; }
        }
    }

    public class PlayerRiskInfo
    {
        public int PlayerId { get; set; }
        public string PlayerName { get; set; }
        public string GroupName { get; set; }
        public string RiskLevel { get; set; }
        public int DaysSinceLastVisit { get; set; }
        public string RecommendedAction { get; set; }
        public int MissedTrainings { get; set; }
        public int MissedInRow { get; set; }
        public DateTime? LastAttendance { get; set; }
        public double AttendancePercent { get; set; }
    }
}
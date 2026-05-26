using PingTrack.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PingTrack.AppData
{
    public static class PlayerStatisticsService
    {
        #region Детальная статистика игрока
        public static PlayerDetailedStats GetPlayerStats(int playerId, DateTime startDate, DateTime endDate)
        {
            Players player = App.db.Players
                .Include("Groups")
                .FirstOrDefault(p => p.ID_Player == playerId);

            if (player == null)
                return null;

            List<Attendance> attendances = App.db.Attendance
                .Include("Trainings")
                .Include("Trainings.Training_Types")
                .Where(a => a.ID_Player == playerId
                       && a.Trainings.Date >= startDate
                       && a.Trainings.Date <= endDate)
                .OrderBy(a => a.Trainings.Date)
                .ToList();

            int totalTrainings = attendances.Count;
            int attendedTrainings = attendances.Count(a => a.Is_Present);
            double attendancePercent = totalTrainings > 0
                ? Math.Round(attendedTrainings * 100.0 / totalTrainings, 1)
                : 0;

            double averageScore = 0;
            if (attendances.Any(a => a.Score.HasValue))
            {
                averageScore = Math.Round(
                    attendances.Where(a => a.Score.HasValue).Average(a => a.Score.Value), 1);
            }

            int bestStreak = CalculateBestStreak(attendances);
            int currentStreak = CalculateCurrentStreak(attendances);

            DateTime lastAttendance = DateTime.MinValue;
            if (attendances.Any(a => a.Is_Present))
            {
                lastAttendance = attendances
                    .Where(a => a.Is_Present)
                    .Max(a => a.Trainings.Date);
            }

            List<string> preferredTypes = attendances
                .Where(a => a.Is_Present)
                .GroupBy(a => a.Trainings.Training_Types.Type_Name)
                .OrderByDescending(g => g.Count())
                .Take(3)
                .Select(g => $"{g.Key} ({g.Count()})")
                .ToList();

            return new PlayerDetailedStats
            {
                PlayerName = player.Full_Name,
                GroupName = player.Groups.Group_Name,
                TotalTrainings = totalTrainings,
                AttendedTrainings = attendedTrainings,
                AttendancePercent = attendancePercent,
                AverageScore = averageScore,
                BestStreak = bestStreak,
                CurrentStreak = currentStreak,
                LastAttendance = lastAttendance,
                PreferredTrainingTypes = preferredTypes
            };
        }

        private static int CalculateBestStreak(List<Attendance> attendances)
        {
            int bestStreak = 0;
            int currentStreak = 0;

            foreach (Attendance att in attendances.OrderBy(a => a.Trainings.Date))
            {
                if (att.Is_Present)
                {
                    currentStreak++;
                    if (currentStreak > bestStreak)
                        bestStreak = currentStreak;
                }
                else
                {
                    currentStreak = 0;
                }
            }

            return bestStreak;
        }

        private static int CalculateCurrentStreak(List<Attendance> attendances)
        {
            int currentStreak = 0;

            foreach (Attendance att in attendances.OrderByDescending(a => a.Trainings.Date))
            {
                if (att.Is_Present)
                    currentStreak++;
                else
                    break;
            }

            return currentStreak;
        }
        #endregion

        #region Список игроков в зоне риска
        public static List<PlayerRiskInfo> GetAtRiskPlayers()
        {
            return RiskAnalysisService.GetAtRiskPlayers();
        }
        #endregion
    }

    #region Классы данных
    public class PlayerDetailedStats
    {
        public string PlayerName { get; set; }
        public string GroupName { get; set; }
        public int TotalTrainings { get; set; }
        public int AttendedTrainings { get; set; }
        public double AttendancePercent { get; set; }
        public double AverageScore { get; set; }
        public int BestStreak { get; set; }
        public int CurrentStreak { get; set; }
        public DateTime LastAttendance { get; set; }
        public List<string> PreferredTrainingTypes { get; set; }
    }
    #endregion
}
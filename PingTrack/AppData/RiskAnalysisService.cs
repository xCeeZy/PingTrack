using PingTrack.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PingTrack.AppData
{
    public static class RiskAnalysisService
    {
        /// <summary>
        /// Получить список игроков в зоне риска
        /// </summary>
        /// <param name="daysBack">За сколько дней анализировать (по умолчанию 30)</param>
        /// <param name="topCount">Сколько игроков вернуть (по умолчанию 5)</param>
        public static List<PlayerAtRisk> GetPlayersAtRisk(int daysBack = 30, int topCount = 5)
        {
            try
            {
                var startDate = DateTime.Now.AddDays(-daysBack);
                var playersAtRisk = new List<PlayerAtRisk>();

                // Получаем всех игроков
                var allPlayers = App.db.Players
                    .Include("Groups")
                    .ToList();

                foreach (var player in allPlayers)
                {
                    // Получаем все тренировки группы игрока за период
                    var groupTrainings = App.db.Trainings
                        .Where(t => t.ID_Group == player.ID_Group &&
                                   t.Date >= startDate &&
                                   t.Date <= DateTime.Now)
                        .ToList();

                    if (!groupTrainings.Any())
                        continue;

                    // Получаем посещения игрока
                    var trainingIds = groupTrainings.Select(t => t.ID_Training).ToList();
                    var attendances = App.db.Attendance
                        .Include("Trainings")
                        .Where(a => a.ID_Player == player.ID_Player &&
                                   trainingIds.Contains(a.ID_Training))
                        .ToList();

                    // Считаем пропуски
                    var attendedCount = attendances.Count(a => a.Is_Present);
                    var totalTrainings = groupTrainings.Count;
                    var missedCount = totalTrainings - attendedCount;
                    var attendancePercent = totalTrainings > 0
                        ? Math.Round((double)attendedCount / totalTrainings * 100, 1)
                        : 0;

                    // Последнее посещение
                    var lastAttendance = attendances
                        .Where(a => a.Is_Present)
                        .OrderByDescending(a => a.Trainings.Date)
                        .FirstOrDefault();

                    // Определяем уровень риска
                    string riskLevel = DetermineRiskLevel(attendancePercent, missedCount, lastAttendance?.Trainings.Date);

                    // Добавляем в список, если есть риск
                    if (riskLevel != null)
                    {
                        playersAtRisk.Add(new PlayerAtRisk
                        {
                            PlayerId = player.ID_Player,
                            PlayerName = player.Full_Name,
                            GroupName = player.Groups?.Group_Name ?? "Без группы",
                            MissedTrainings = missedCount,
                            LastAttendance = lastAttendance?.Trainings.Date,
                            AttendancePercent = attendancePercent,
                            RiskLevel = riskLevel
                        });
                    }
                }

                // Сортируем по уровню риска и проценту посещаемости
                return playersAtRisk
                    .OrderBy(p => p.RiskLevel == "Высокий" ? 0 : 1)
                    .ThenBy(p => p.AttendancePercent)
                    .Take(topCount)
                    .ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка в GetPlayersAtRisk: {ex.Message}");
                return new List<PlayerAtRisk>();
            }
        }

        /// <summary>
        /// Определить уровень риска игрока
        /// </summary>
        private static string DetermineRiskLevel(double attendancePercent, int missedCount, DateTime? lastAttendance)
        {
            // Высокий риск
            if (attendancePercent < 40 || missedCount >= 5)
                return "Высокий";

            // Средний риск
            if (attendancePercent < 60 || missedCount >= 3)
                return "Средний";

            // Проверяем давность последнего посещения
            if (lastAttendance.HasValue)
            {
                var daysSinceLastVisit = (DateTime.Now - lastAttendance.Value).Days;

                if (daysSinceLastVisit > 14)
                    return "Высокий";
                else if (daysSinceLastVisit > 7)
                    return "Средний";
            }
            else
            {
                // Никогда не посещал
                return "Высокий";
            }

            // Все хорошо, не в зоне риска
            return null;
        }
    }
}

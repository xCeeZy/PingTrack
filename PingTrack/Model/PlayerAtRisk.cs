using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PingTrack.Model
{
    public class PlayerAtRisk
    {
        public int PlayerId { get; set; }
        public string PlayerName { get; set; }
        public string GroupName { get; set; }
        public int MissedTrainings { get; set; }
        public DateTime? LastAttendance { get; set; }
        public double AttendancePercent { get; set; }
        public string RiskLevel { get; set; } // "Высокий", "Средний"

        public string LastAttendanceText
        {
            get
            {
                if (!LastAttendance.HasValue)
                    return "Никогда";

                var days = (DateTime.Now - LastAttendance.Value).Days;

                if (days == 0)
                    return "Сегодня";
                else if (days == 1)
                    return "Вчера";
                else if (days < 7)
                    return $"{days} дн. назад";
                else if (days < 30)
                    return $"{days / 7} нед. назад";
                else
                    return $"{days / 30} мес. назад";
            }
        }

        public string RiskLevelEmoji
        {
            get
            {
                return RiskLevel == "Высокий" ? "🔴" : "🟡";
            }
        }
    }
}

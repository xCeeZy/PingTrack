using Microsoft.Win32;
using PingTrack.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PingTrack.AppData
{
    public static class ExportService
    {
        #region Экспорт расписания в текстовый файл
        public static void ExportTrainingsToText(List<Trainings> trainings)
        {
            if (trainings == null || trainings.Count == 0)
            {
                Feedback.ShowWarning("Экспорт невозможен", "Нет данных для экспорта.");
                return;
            }

            SaveFileDialog saveDialog = new SaveFileDialog
            {
                Filter = "Текстовые файлы (*.txt)|*.txt",
                FileName = string.Format("Расписание_тренировок_{0:dd-MM-yyyy}.txt", DateTime.Now),
                Title = "Сохранить расписание"
            };

            bool? result = saveDialog.ShowDialog();
            if (result != true)
                return;

            try
            {
                StringBuilder content = new StringBuilder();

                content.AppendLine("═══════════════════════════════════════════════════════════════════════════════");
                content.AppendLine("                        РАСПИСАНИЕ ТРЕНИРОВОК СЕКЦИИ                          ");
                content.AppendLine("                          Настольный теннис PingTrack                          ");
                content.AppendLine("═══════════════════════════════════════════════════════════════════════════════");
                content.AppendLine();
                content.AppendLine(string.Format("Дата формирования: {0:dd.MM.yyyy HH:mm}", DateTime.Now));
                content.AppendLine(string.Format("Всего тренировок: {0}", trainings.Count));
                content.AppendLine();
                content.AppendLine("───────────────────────────────────────────────────────────────────────────────");
                content.AppendLine();

                List<Trainings> sortedTrainings = trainings.OrderBy(t => t.Date).ThenBy(t => t.Time).ToList();

                string currentDate = string.Empty;
                int dayCounter = 0;

                foreach (Trainings training in sortedTrainings)
                {
                    string trainingDate = training.Date.ToString("dd.MM.yyyy");

                    if (currentDate != trainingDate)
                    {
                        if (dayCounter > 0)
                        {
                            content.AppendLine();
                            content.AppendLine("───────────────────────────────────────────────────────────────────────────────");
                            content.AppendLine();
                        }

                        dayCounter++;
                        currentDate = trainingDate;

                        string dayOfWeek = training.Date.ToString("dddd", new System.Globalization.CultureInfo("ru-RU"));
                        dayOfWeek = char.ToUpper(dayOfWeek[0]) + dayOfWeek.Substring(1);

                        content.AppendLine(string.Format("📅 {0}, {1}", dayOfWeek, trainingDate));
                        content.AppendLine();
                    }

                    string time = training.Time.ToString(@"hh\:mm");
                    string group = training.Groups?.Group_Name ?? "—";
                    string type = training.Training_Types?.Type_Name ?? "—";
                    string coach = training.Users?.Full_Name ?? "—";
                    string note = string.IsNullOrEmpty(training.Note) ? "" : string.Format(" | {0}", training.Note);

                    content.AppendLine(string.Format("   ⏰ Время: {0}", time));
                    content.AppendLine(string.Format("   👥 Группа: {0}", group));
                    content.AppendLine(string.Format("   🏓 Тип: {0}", type));
                    content.AppendLine(string.Format("   👨‍🏫 Тренер: {0}{1}", coach, note));
                    content.AppendLine();
                }

                content.AppendLine("═══════════════════════════════════════════════════════════════════════════════");
                content.AppendLine("                           Конец расписания                                    ");
                content.AppendLine("═══════════════════════════════════════════════════════════════════════════════");

                File.WriteAllText(saveDialog.FileName, content.ToString(), Encoding.UTF8);

                string fileName = Path.GetFileName(saveDialog.FileName);
                string message = string.Format("Расписание успешно экспортировано!\n\nФайл: {0}\n\nОткрыть файл?", fileName);

                bool openFile = Feedback.AskQuestion("Экспорт завершён", message);

                if (openFile)
                {
                    System.Diagnostics.Process.Start(saveDialog.FileName);
                }
            }
            catch (Exception ex)
            {
                Feedback.ShowError("Ошибка экспорта", string.Format("Не удалось сохранить файл.\n\n{0}", ex.Message));
            }
        }
        #endregion

        #region Экспорт отчётов в CSV
        public static void ExportReportToCsv(IEnumerable<object> reportItems, string reportName)
        {
            List<object> items = reportItems?.ToList() ?? new List<object>();

            if (items.Count == 0)
            {
                Feedback.ShowWarning("Экспорт невозможен", "Сначала сформируйте отчёт. Данных для экспорта нет.");
                return;
            }

            SaveFileDialog saveDialog = new SaveFileDialog
            {
                Filter = "CSV-файлы (*.csv)|*.csv",
                FileName = $"{SanitizeFileName(reportName)}_{DateTime.Now:dd-MM-yyyy_HH-mm}.csv",
                Title = "Сохранить отчёт"
            };

            bool? result = saveDialog.ShowDialog();
            if (result != true)
                return;

            try
            {
                Type itemType = items.First().GetType();
                List<PropertyInfo> properties = itemType.GetProperties()
                    .Where(p => p.CanRead && IsSimpleType(p.PropertyType))
                    .ToList();

                StringBuilder csv = new StringBuilder();
                csv.AppendLine(string.Join(";", properties.Select(p => EscapeCsvValue(p.Name))));

                foreach (object item in items)
                {
                    IEnumerable<string> values = properties.Select(p =>
                    {
                        object value = p.GetValue(item, null);
                        return EscapeCsvValue(FormatValue(value));
                    });

                    csv.AppendLine(string.Join(";", values));
                }

                File.WriteAllText(saveDialog.FileName, csv.ToString(), Encoding.UTF8);

                bool openFile = Feedback.AskQuestion(
                    "Экспорт завершён",
                    $"Отчёт успешно сохранён.\n\nФайл: {Path.GetFileName(saveDialog.FileName)}\n\nОткрыть файл?");

                if (openFile)
                    System.Diagnostics.Process.Start(saveDialog.FileName);
            }
            catch (Exception ex)
            {
                Feedback.ShowError("Ошибка экспорта", $"Не удалось экспортировать отчёт.\n\n{ex.Message}");
            }
        }

        private static bool IsSimpleType(Type type)
        {
            Type realType = Nullable.GetUnderlyingType(type) ?? type;

            return realType.IsPrimitive
                || realType == typeof(string)
                || realType == typeof(decimal)
                || realType == typeof(DateTime)
                || realType == typeof(TimeSpan);
        }

        private static string FormatValue(object value)
        {
            if (value == null)
                return string.Empty;

            if (value is DateTime dateTime)
                return dateTime.ToString("dd.MM.yyyy HH:mm");

            if (value is TimeSpan timeSpan)
                return timeSpan.ToString(@"hh\:mm");

            return value.ToString();
        }

        private static string EscapeCsvValue(string value)
        {
            value = value ?? string.Empty;
            value = value.Replace("\"", "\"\"");

            if (value.Contains(";") || value.Contains("\n") || value.Contains("\r") || value.Contains("\""))
                return $"\"{value}\"";

            return value;
        }

        private static string SanitizeFileName(string fileName)
        {
            string safeName = string.IsNullOrWhiteSpace(fileName) ? "Отчёт" : fileName;

            foreach (char invalidChar in Path.GetInvalidFileNameChars())
                safeName = safeName.Replace(invalidChar, '_');

            return safeName.Replace(' ', '_');
        }
        #endregion
    }
}
using Microsoft.Win32;
using PingTrack.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
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
    }
}
using PingTrack.Model;
using System;
using System.Diagnostics;

namespace PingTrack.AppData
{
    public static class ActionLogService
    {
        #region Запись действий пользователя
        public static void AddLog(string actionType, string tableName, int? recordId, string description)
        {
            try
            {
                if (App.db == null)
                    return;

                ActionLogs log = new ActionLogs
                {
                    ID_User = AuthenticationService.GetUserId() == 0 ? (int?)null : AuthenticationService.GetUserId(),
                    Action_Type = actionType,
                    Table_Name = tableName,
                    Record_ID = recordId,
                    Description = description,
                    Log_Date = DateTime.Now
                };

                App.db.ActionLogs.Add(log);
                App.db.SaveChanges();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка записи системного журнала: {ex.Message}");
            }
        }
        #endregion

        #region Быстрые методы для типовых действий
        public static void LogCreate(string tableName, int? recordId, string description)
        {
            AddLog("Добавление", tableName, recordId, description);
        }

        public static void LogUpdate(string tableName, int? recordId, string description)
        {
            AddLog("Изменение", tableName, recordId, description);
        }

        public static void LogDelete(string tableName, int? recordId, string description)
        {
            AddLog("Удаление", tableName, recordId, description);
        }

        public static void LogView(string tableName, int? recordId, string description)
        {
            AddLog("Просмотр", tableName, recordId, description);
        }

        public static void LogExport(string tableName, string description)
        {
            AddLog("Экспорт", tableName, null, description);
        }

        public static void LogSystem(string description)
        {
            AddLog("Система", "System", null, description);
        }
        #endregion
    }
}
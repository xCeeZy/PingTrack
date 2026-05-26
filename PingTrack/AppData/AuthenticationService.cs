using PingTrack.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PingTrack.AppData
{
    public sealed class AuthenticationService
    {
        private static Users _currentUser;

        public static Users CurrentUser => _currentUser;

        #region Авторизация и аутентификация
        public static bool Login(string login, string password)
        {
            Users user = App.db.Users.FirstOrDefault(u => u.Login == login && u.Password == password && u.IsDeleted == false);
            if (user == null)
                return false;

            _currentUser = user;
            ActionLogService.LogSystem($"Пользователь {user.Login} вошёл в систему.");
            return true;
        }

        public static void Logout()
        {
            string login = _currentUser?.Login ?? "Неизвестный пользователь";
            ActionLogService.LogSystem($"Пользователь {login} вышел из системы.");
            _currentUser = null;
        }

        public static bool IsAuthenticated()
        {
            return _currentUser != null;
        }
        #endregion

        #region Получение данных пользователя
        public static string GetUserRole()
        {
            if (_currentUser == null || _currentUser.Roles == null)
                return string.Empty;

            return _currentUser.Roles.Role_Name;
        }

        public static string GetUserLogin()
        {
            return _currentUser?.Login ?? string.Empty;
        }

        public static int GetUserId()
        {
            return _currentUser?.ID_User ?? 0;
        }
        #endregion
    }
}
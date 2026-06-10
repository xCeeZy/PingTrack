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
        #region Поля

        private static Users _currentUser;

        public static Users CurrentUser => _currentUser;

        #endregion

        #region Авторизация

        public static bool Login(string login, string password)
        {
            string normalizedLogin = login.Trim();

            Users user = App.db.Users
                .Include("Roles")
                .FirstOrDefault(u =>
                    u.Login == normalizedLogin &&
                    u.Password == password &&
                    u.IsDeleted == false);

            if (user == null)
                return false;

            _currentUser = user;

            ActionLogService.LogSystem($"Пользователь {user.Login} вошёл в систему.");

            return true;
        }

        public static void Logout()
        {
            string login = _currentUser != null
                ? _currentUser.Login
                : "Неизвестный пользователь";

            ActionLogService.LogSystem($"Пользователь {login} вышел из системы.");

            _currentUser = null;
        }

        public static bool IsAuthenticated()
        {
            return _currentUser != null;
        }

        #endregion

        #region Данные пользователя

        public static string GetUserRole()
        {
            if (_currentUser == null || _currentUser.Roles == null)
                return string.Empty;

            return _currentUser.Roles.Role_Name;
        }

        public static string GetUserLogin()
        {
            return _currentUser != null ? _currentUser.Login : string.Empty;
        }

        public static int GetUserId()
        {
            return _currentUser != null ? _currentUser.ID_User : 0;
        }

        #endregion
    }
}
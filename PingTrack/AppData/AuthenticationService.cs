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

        public static bool Login(string login, string password)
        {
            Users user = App.db.Users.FirstOrDefault(u => u.Login == login && u.Password == password);
            if (user == null)
                return false;

            _currentUser = user;
            return true;
        }

        public static void Logout()
        {
            _currentUser = null;
        }

        public static bool IsAuthenticated()
        {
            return _currentUser != null;
        }

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
    }
}
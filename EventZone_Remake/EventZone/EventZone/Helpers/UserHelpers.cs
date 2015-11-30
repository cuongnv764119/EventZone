using System.Web;
using EventZone.Models;
using System;

namespace EventZone.Helpers
{
    public class UserHelpers
    {
        private const string User = "user";
        private const string Admin = "admin";

        public static User GetCurrentUser(HttpSessionStateBase session)
        {
            var userSession = session[User];

            if (userSession == null || userSession.ToString().Length == 0)
            {
                return null;
            }
            var user = (User) userSession;

            return user;
        }

        public static void SetCurrentUser(HttpSessionStateBase session, User user)
        {
            session[User] = user;
        }
        public static User GetCurrentAdmin(HttpSessionStateBase session) {
            var adminSession = session[Admin];
            if (adminSession == null || adminSession.ToString().Length == 0)
            {
                return null;
            }
            var user = (User) adminSession;
            return user;
        }
        public static void SetCurrentAdmin(HttpSessionStateBase session, User admin) {
            session[Admin] = admin;
        }

    }
}
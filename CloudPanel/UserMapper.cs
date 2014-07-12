using log4net;
using Nancy;
using Nancy.Authentication.Forms;
using Nancy.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CloudPanel
{
    public class UserMapper : IUserMapper
    {
        private static readonly ILog log = log4net.LogManager.GetLogger(typeof(UserMapper));

        public static List<AuthenticatedUser> loggedInUsers = new List<AuthenticatedUser>();

        public IUserIdentity GetUserFromIdentifier(Guid identifier, NancyContext context)
        {
            return loggedInUsers.FirstOrDefault(u => u.UserGuid == identifier);
        }

        public static Guid? ValidateUser(string username, string password)
        {
            // Query Active Directory
            // bool valid = PrincipalContext.ValidateUser(username, password);
            // if (valid) Find UserPrincipalExt object
            // return UserObject class filled with GUID and other data such as membership, displayname, etc

            // Return the user's GUID from Active Directory
            log.DebugFormat("Attempting to log in user {0}...", username);

            Guid newGuid = Guid.NewGuid();
            var userRecord = loggedInUsers.FirstOrDefault(u => u.UserGuid == newGuid);
            if (userRecord == null)
            {
                log.DebugFormat("User {0} is not already logged in. Logging in the new user", username);

                AuthenticatedUser newUser = new AuthenticatedUser();
                newUser.UserGuid = Guid.NewGuid();
                newUser.UserName = username;
                newUser.Claims = new[] { "SuperAdmin" };
                loggedInUsers.Add(newUser);

                userRecord = newUser;

                log.DebugFormat("User {0} successfully logged in. User guid value is {1}", username, newGuid.ToString());
            }

            return userRecord.UserGuid;
        }
    }
}
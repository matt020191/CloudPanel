using CloudPanel.ActiveDirectory;
using CloudPanel.Base.Config;
using CloudPanel.Database.EntityFramework;
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
        private static readonly ILog logger = log4net.LogManager.GetLogger(typeof(UserMapper));

        public static List<AuthenticatedUser> loggedInUsers = new List<AuthenticatedUser>();

        public IUserIdentity GetUserFromIdentifier(Guid identifier, NancyContext context)
        {
            return loggedInUsers.FirstOrDefault(u => u.UserGuid == identifier);
        }

        public static Guid? ValidateUser(string username, string password)
        {
            ADUsers user = null;
            CloudPanelContext db = null;
            try
            {
                user = new ADUsers(Settings.Username, Settings.DecryptedPassword, Settings.PrimaryDC);

                // Authenticate the user
                var authenticatedUser = user.Authenticate(username, password);

                // Create our authenticated user object to store since authentication was successful
                var authUser = new AuthenticatedUser();
                authUser.UserGuid = authenticatedUser.UserGuid;
                authUser.UserName = authenticatedUser.UserPrincipalName;
                authUser.DisplayName = authenticatedUser.DisplayName;

                // Create our claims (security rights)
                var claims = new List<string>();
                foreach (var c in authenticatedUser.MemberOf)
                {
                    // See if the current group matches a group in our list of SuperAdmins
                    if (!string.IsNullOrEmpty(c))
                    {
                        var found = Settings.SuperAdmins.Where(x => x.Equals(c, StringComparison.InvariantCultureIgnoreCase));
                        if (found != null)
                        {
                            logger.DebugFormat("Compared {0} to {1} and found a match", c, found.FirstOrDefault());
                            claims.Add("SuperAdmin");
                            break;
                        }
                    }
                }

                // See if the user is a SuperAdmin. No need to query database if SuperAdmin
                if (!claims.Contains("SuperAdmin"))
                {
                    logger.DebugFormat("User is not a super admin so we need to query the database for reseller and company admin values");
                    db = new CloudPanelContext(Settings.ConnectionString);
                    ParseAdmin(ref db, ref authUser, ref claims);
                }

                // Add to our list of logged in users after adding claims
                authUser.Claims = claims;
                loggedInUsers.Add(authUser);

                // Return the user guid
                return authUser.UserGuid;
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error logging in user {0}: {1}", username, ex.ToString());
                return null;
            }
            finally
            {
                if (db != null)
                    db.Dispose();

                if (user != null)
                    user.Dispose();
            }
        }

        public static bool ContainsClaims(Guid guid)
        {
            var user = loggedInUsers.Where(x => x.UserGuid.Equals(guid)).FirstOrDefault();
            if (user == null)
                return false;
            else
            {
                if (user.Claims == null)
                    return false;
                else
                    return true;
            }
        }

        private static void ParseAdmin(ref CloudPanelContext db, ref AuthenticatedUser authUser, ref List<string> claims)
        {
            string upn = authUser.UserName;
            var user = (from d in db.Users
                       where d.UserPrincipalName == upn
                       select d).FirstOrDefault();

            if (user != null)
            {
                if (user.IsResellerAdmin == true || user.IsCompanyAdmin == true)
                {
                    var companyInfo = (from c in db.Companies
                                       join r in db.Companies on c.ResellerCode equals r.CompanyCode into reseller
                                       from data in reseller.DefaultIfEmpty()
                                       where c.CompanyCode == user.CompanyCode
                                       select new {
                                         CompanyCode = c.CompanyCode,
                                         CompanyName = c.CompanyName,
                                         ResellerCode = c.ResellerCode,
                                         ResellerName = data.CompanyName
                                       }).FirstOrDefault();

                    // Add the values to the user object for 
                    // selected company codes and reseller codes
                    authUser.CompanyCode = user.CompanyCode;
                    authUser.SelectedCompanyCode = user.CompanyCode;

                    if (companyInfo != null)
                    {
                        authUser.SelectedCompanyName = companyInfo.CompanyName;
                        authUser.SelectedResellerCode = companyInfo.ResellerCode;
                        authUser.SelectedResellerName = companyInfo.ResellerName;
                    }

                    // Add the claims
                    if (user.IsResellerAdmin == true)
                    {
                        claims.Add("ResellerAdmin@" + authUser.SelectedResellerCode);
                    }

                    if (user.IsCompanyAdmin == true)
                    {
                        claims.Add("CompanyAdmin@" + authUser.CompanyCode);
                    }
                }
            }            
        }
    }
}
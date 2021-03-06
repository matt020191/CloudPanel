﻿using CloudPanel.ActiveDirectory;
using CloudPanel.Base.Config;
using CloudPanel.Database.EntityFramework;
using log4net;
using Nancy;
using Nancy.Authentication.Forms;
using Nancy.Security;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CloudPanel
{
    public class UserMapper : IUserMapper
    {
        private static readonly ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static List<AuthenticatedUser> loggedInUsers = new List<AuthenticatedUser>();

        public IUserIdentity GetUserFromIdentifier(Guid identifier, NancyContext context)
        {
            return loggedInUsers.FirstOrDefault(u => u.UserGuid == identifier);
        }

        /// <summary>
        /// Gets the user from the API key
        /// </summary>
        /// <param name="apiKey"></param>
        /// <returns></returns>
        public static IUserIdentity GetUserFromApiKey(string apiKey)
        {
            CloudPanelContext db = null;
            try
            {
                db = new CloudPanelContext(Settings.ConnectionString);

                var user = db.Users
                             .Include("ApiKey")
                             .Include("Role")
                             .Where(x => x.ApiKey.Key == apiKey)
                             .Single();

                if (user == null)
                    return null;
                else
                {
                    var authUser = new AuthenticatedUser();
                    authUser.UserGuid = user.UserGuid;
                    authUser.UserName = user.UserPrincipalName;
                    authUser.CompanyCode = user.CompanyCode;
                    authUser.SelectedCompanyCode = user.CompanyCode;
                    authUser.DisplayName = user.DisplayName;
                    authUser.SecurityPermissions = user.Role;

                    var claims = new List<string>();
                    foreach (var p in user.Role.GetType().GetProperties())
                    {
                        if (p.PropertyType == typeof(bool))
                        {
                            if ((bool)p.GetValue(user.Role, null))
                            {
                                claims.Add(p.Name);
                            }
                        }
                    }
                    authUser.Claims = claims;

                    return authUser;
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error logging user in with api key {0}: {1}", apiKey, ex.ToString());
                return null;
            }
            finally
            {
                if (db != null)
                    db.Dispose();
            }
        }

        public static IUserIdentity ValidateUser(string username, string password)
        {
            ADUsers user = null;
            CloudPanelContext db = null;
            try
            {
                user = new ADUsers(Settings.Username, Settings.DecryptedPassword, Settings.PrimaryDC);

                // Authenticate the user
                var authenticatedUser = user.AuthenticateQuickly(username, password);
                if (authenticatedUser == null)
                    throw new Exception("Login failed. Please try again or contact support.");

                // See if the user already exists
                var authUser = loggedInUsers.Where(x => x.UserName.Equals(username, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();
                if (authUser == null)
                {
                    logger.DebugFormat("The user {0} is not cached. Adding to collection", username);
                    var newUser = new AuthenticatedUser()
                        {
                            UserGuid = authenticatedUser.UserGuid,
                            UserName = authenticatedUser.UserPrincipalName,
                            DisplayName = authenticatedUser.DisplayName
                        };
                    loggedInUsers.Add(newUser);
                    authUser = newUser;
                }
                else
                    logger.DebugFormat("The user {0} is cached. Updating security claims", username);

                // Add all the claims for the user
                var claims = new List<string>();
                foreach (var memberof in authenticatedUser.MemberOf) {
                    logger.DebugFormat("Checking group {0} against {1}", memberof, Settings.SuperAdminsAsString);
                    var isSuper = Settings.SuperAdmins.Any(x => x.Equals(memberof,StringComparison.InvariantCultureIgnoreCase));
                    if (isSuper)
                    {
                        claims.Add("SuperAdmin");
                        logger.DebugFormat("Adding claim SuperAdmin to user {0}", authUser.UserName);
                        break;
                    }
                }

                // If the user isn't a super admin then check company permissions
                if (!claims.Contains("SuperAdmin"))
                {
                    db = new CloudPanelContext(Settings.ConnectionString);
                    db.Database.Connection.Open();

                    var sqlUser = (from d in db.Users
                                   join c in db.Companies on d.CompanyCode equals c.CompanyCode into c1
                                   from company in c1.DefaultIfEmpty()
                                   join r in db.Companies on company.ResellerCode equals r.CompanyCode into r1
                                   from reseller in r1.DefaultIfEmpty()
                                   join p in db.UserRoles on d.RoleID equals p.RoleID into p1
                                   from permission in p1.DefaultIfEmpty()
                                   where d.UserPrincipalName == authUser.UserName
                                   select new
                                   {
                                        CompanyCode = d.CompanyCode,
                                        CompanyName = company.CompanyName,
                                        ResellerCode = company.ResellerCode,
                                        ResellerName = reseller.CompanyName,
                                        IsResellerAdmin = d.IsResellerAdmin,
                                        IsCompanyAdmin = d.IsCompanyAdmin,
                                        UserRole = permission
                                   }).First();

                    // Add values
                    authUser.ResellerCode = sqlUser.ResellerCode;
                    authUser.SelectedResellerCode = sqlUser.ResellerCode;
                    authUser.SelectedResellerName = sqlUser.ResellerName;
                    authUser.CompanyCode = sqlUser.CompanyCode;
                    authUser.SelectedCompanyCode = sqlUser.CompanyCode;
                    authUser.SelectedCompanyName = sqlUser.CompanyName;

                    if (sqlUser.IsResellerAdmin == true)
                    {
                        claims.Add("ResellerAdmin");
                        logger.InfoFormat("Adding claim ResellerAdmin to {0}", authUser.UserName);
                    }
                    else
                        logger.DebugFormat("User {0} is NOT a ResellerAdmin", authUser.UserName);

                    if (sqlUser.IsCompanyAdmin == true)
                    { // Check if the user is a company admin
                        if (sqlUser.UserRole != null)
                        { // Make sure the user role isn't null and add as company admin
                            claims.Add("CompanyAdmin");
                            logger.InfoFormat("Adding claim CompanyAdmin to user {0}", authUser.UserName);

                            foreach (var p in sqlUser.UserRole.GetType().GetProperties())
                            { // Loop through each property and set the value if it is true on the user role data
                                if (p.PropertyType == typeof(bool))
                                {
                                    bool isTrue = (bool)p.GetValue(sqlUser.UserRole, null);
                                    if (isTrue)
                                    {
                                        claims.Add(p.Name);
                                        logger.InfoFormat("Adding claim {0} to user {1}", p.Name, authUser.UserName);
                                    }
                                }
                            }
                        }
                    }
                    else
                        logger.DebugFormat("User {0} is NOT a CompanyAdmin", authUser.UserName);
                }

                logger.DebugFormat("Setting the claims for user {0} to {1}", username, String.Join(", ", claims));
                authUser.Claims = claims;

                return authUser;
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error logging in user {0}: {1}", username, ex.ToString());
                throw;
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
    }
}
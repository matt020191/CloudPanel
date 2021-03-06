﻿using CloudPanel.ActiveDirectory.Extensions;
using CloudPanel.Base.Models.Database;
using log4net;
using System;
using System.Linq;
using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;

namespace CloudPanel.ActiveDirectory
{
    public class ADUsers : IDisposable
    {
        private readonly ILog logger = LogManager.GetLogger(typeof(ADUsers));

        private string _username;
        private string _password;
        private string _domainController;
        private string _ldapStr;
        private bool _disposed;

        internal DirectoryEntry de = null;
        internal PrincipalContext pc = null;

        public ADUsers(string username, string decryptedPassword, string domainController)
        {
            this._username = username;
            this._password = decryptedPassword;
            this._domainController = domainController;

            this._ldapStr = string.Format("LDAP://{0}/", domainController);
        }

        #region Initialization

        private DirectoryEntry GetDirectoryEntry(string dnPath = null)
        {
            logger.DebugFormat("Retrieving the DirectoryEntry... Path: {0}", dnPath);

            if (de == null)
                de = new DirectoryEntry(_ldapStr, this._username, this._password);

            if (!string.IsNullOrEmpty(dnPath))
                de.Path = dnPath;
            else
                de.Path = string.Empty;


            return de;
        }

        private PrincipalContext GetPrincipalContext()
        {
            logger.Debug("Retrieving the PrincipalContext...");

            if (pc == null)
                pc = new PrincipalContext(ContextType.Domain, this._domainController, this._username, this._password);

            return pc;
        }

        #endregion
      
        /// <summary>
        /// Simple authenticate without retrieving groups
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public bool AuthenticateSimple(string username, string password)
        {
            pc = GetPrincipalContext();

            logger.DebugFormat("Attempting to authenticate user {0}", username);
            bool valid = pc.ValidateCredentials(username, password);
            if (!valid)
                return false;
            else
            {
                logger.DebugFormat("Successfully authenticated user {0}", username);
                return true;
            }      
        }

        /// <summary>
        /// Authenticates a user against Active Directory
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public Users Authenticate(string username, string password)
        {
            try
            {
                pc = GetPrincipalContext();

                logger.DebugFormat("Attempting to authenticate user {0}", username);

                bool valid = pc.ValidateCredentials(username, password);
                if (!valid)
                    throw new UnauthorizedAccessException();
                else
                {
                    logger.DebugFormat("Successfully authenticated user {0}... Now retrieving security groups...", username);

                    return GetUser(username);
                }                
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error authenticating user {0}. Exception: {1}", username, ex.ToString());
                throw;
            }
        }

        /// <summary>
        /// Authenticates a user against Active Directory with minimal information to quickly authenticate
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public Users AuthenticateQuickly(string username, string password)
        {
            try
            {
                pc = GetPrincipalContext();

                logger.DebugFormat("Attempting to authenticate user {0}", username);
                var user = UserPrincipal.FindByIdentity(pc, IdentityType.UserPrincipalName, username);
                if (user == null)
                    return null;

                logger.DebugFormat("User was found in AD. Validating credentials for {0}", username);
                bool validCredentials = pc.ValidateCredentials(username, password, ContextOptions.SimpleBind);
                if (!validCredentials)
                    return null;

                var returnUser = new Users();
                returnUser.UserPrincipalName = username;
                returnUser.DisplayName = user.DisplayName;
                returnUser.UserGuid = user.Guid.Value;

                logger.DebugFormat("Getting AuthorizationGroups for {0}", username);
                var groups = user.GetGroups();
                var groupsList = new List<string>();
                foreach (var g in groups)
                {
                    logger.DebugFormat("Found group {0} with distinguishedname {1}", g.Name, g.DistinguishedName);
                    groupsList.Add(g.Name);
                }
                returnUser.MemberOf = groupsList.ToArray();

                return returnUser;
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error authenticating user {0}. Exception: {1}", username, ex.ToString());
                throw;
            }
        }

        /// <summary>
        /// Get user information and properties including groups
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public Users GetUser(string username)
        {
            UserPrincipal usr = null;

            Users foundUser = new Users();
            try
            {
                pc = GetPrincipalContext();

                logger.DebugFormat("Attempting to retrieve user {0}", username);
                usr = UserPrincipal.FindByIdentity(pc, username);

                if (usr == null)
                    throw new Exception("Unable to find user " + username);

                logger.DebugFormat("Found user.. now retrieving property values...");
                DirectoryEntry tmp = (DirectoryEntry)usr.GetUnderlyingObject();
                foundUser.UserGuid = tmp.Guid;
                foundUser.DistinguishedName = GetPropertyValue(ref tmp, "DistinguishedName");
                foundUser.BadPwdCount = GetPropertyValue(ref tmp, "badPwdCount", "int");
                foundUser.UserAccountControl = GetPropertyValue(ref tmp, "userAccountControl", "int");
                foundUser.SamAccountType = GetPropertyValue(ref tmp, "sAMAccountType", "int");
                foundUser.sAMAccountName = GetPropertyValue(ref tmp, "sAMAccountName");
                foundUser.Street = GetPropertyValue(ref tmp, "streetAddress");
                foundUser.City = GetPropertyValue(ref tmp, "l");
                foundUser.State = GetPropertyValue(ref tmp, "st");
                foundUser.PostalCode = GetPropertyValue(ref tmp, "postalCode");
                foundUser.Country = GetPropertyValue(ref tmp, "co");
                foundUser.CountryCode = GetPropertyValue(ref tmp, "c");
                foundUser.Company = GetPropertyValue(ref tmp, "company");
                foundUser.Department = GetPropertyValue(ref tmp, "department");
                foundUser.Description = GetPropertyValue(ref tmp, "description");
                foundUser.Firstname = GetPropertyValue(ref tmp, "givenName");
                foundUser.Lastname = GetPropertyValue(ref tmp, "sn");
                foundUser.DisplayName = GetPropertyValue(ref tmp, "displayName");
                foundUser.Name = GetPropertyValue(ref tmp, "name");
                foundUser.UserPrincipalName = GetPropertyValue(ref tmp, "userPrincipalName");
                foundUser.Fax = GetPropertyValue(ref tmp, "facsimileTelephoneNumber");
                foundUser.TelephoneNumber = GetPropertyValue(ref tmp, "telephoneNumber");
                foundUser.HomePhone = GetPropertyValue(ref tmp, "homePhone");
                foundUser.IPPhone = GetPropertyValue(ref tmp, "ipPhone");
                foundUser.JobTitle = GetPropertyValue(ref tmp, "title");
                foundUser.MobilePhone = GetPropertyValue(ref tmp, "mobile");
                foundUser.Office = GetPropertyValue(ref tmp, "physicalDeliveryOfficeName");
                foundUser.Pager = GetPropertyValue(ref tmp, "pager");
                foundUser.POBox = GetPropertyValue(ref tmp, "postOfficeBox");
                foundUser.ScriptPath = GetPropertyValue(ref tmp, "scriptPath");
                foundUser.ProfilePath = GetPropertyValue(ref tmp, "profilePath");
                foundUser.Webpage = GetPropertyValue(ref tmp, "wWWHomePage");
                foundUser.Email = GetPropertyValue(ref tmp, "mail");
                foundUser.msExchMailboxGuid = GetPropertyValue(ref tmp, "msExchMailboxGuid", "guid");

                logger.DebugFormat("Making sure the user is enabled");
                int flags = (int)tmp.Properties["userAccountControl"].Value;
                foundUser.IsEnabled = !Convert.ToBoolean(flags & 0x0002);
                logger.DebugFormat("User Enabled: {0}", foundUser.IsEnabled);

                // Get groups
                logger.DebugFormat("Retrieving groups");
                List<string> groups = new List<string>();
                foreach (var g in usr.GetGroups())
                {
                    logger.DebugFormat("Found group {0} with distinguishedname {1}", g.Name, g.DistinguishedName);
                    groups.Add(g.Name);
                }
                foundUser.MemberOf = groups.ToArray();
                logger.DebugFormat("Member of the following groups {0}", string.Join(",", foundUser.MemberOf));

                return foundUser;
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error retrieving user {0}. Exception: {1}", username, ex.ToString());
                throw;
            }
            finally
            {
                if (usr != null)
                    usr.Dispose();
            }
        }

        /// <summary>
        /// Get user information and properties excluding groups to speed up the call
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public Users GetUserWithoutGroups(string identity)
        {
            UserPrincipal usr = null;

            Users foundUser = new Users();
            try
            {
                pc = GetPrincipalContext();

                logger.DebugFormat("Attempting to retrieve user {0}", identity);
                usr = UserPrincipal.FindByIdentity(pc, identity);

                DirectoryEntry tmp = (DirectoryEntry)usr.GetUnderlyingObject();
                foundUser.UserGuid = tmp.Guid;
                foundUser.DistinguishedName = GetPropertyValue(ref tmp, "DistinguishedName");
                foundUser.BadPwdCount = GetPropertyValue(ref tmp, "badPwdCount", "int");
                foundUser.UserAccountControl = GetPropertyValue(ref tmp, "userAccountControl", "int");
                foundUser.SamAccountType = GetPropertyValue(ref tmp, "sAMAccountType", "int");
                foundUser.sAMAccountName = GetPropertyValue(ref tmp, "sAMAccountName");
                foundUser.Street = GetPropertyValue(ref tmp, "streetAddress");
                foundUser.City = GetPropertyValue(ref tmp, "l");
                foundUser.State = GetPropertyValue(ref tmp, "st");
                foundUser.PostalCode = GetPropertyValue(ref tmp, "postalCode");
                foundUser.Country = GetPropertyValue(ref tmp, "co");
                foundUser.CountryCode = GetPropertyValue(ref tmp, "c");
                foundUser.Company = GetPropertyValue(ref tmp, "company");
                foundUser.Department = GetPropertyValue(ref tmp, "department");
                foundUser.Description = GetPropertyValue(ref tmp, "description");
                foundUser.Firstname = GetPropertyValue(ref tmp, "givenName");
                foundUser.Lastname = GetPropertyValue(ref tmp, "sn");
                foundUser.DisplayName = GetPropertyValue(ref tmp, "displayName");
                foundUser.Name = GetPropertyValue(ref tmp, "name");
                foundUser.UserPrincipalName = GetPropertyValue(ref tmp, "userPrincipalName");
                foundUser.Fax = GetPropertyValue(ref tmp, "facsimileTelephoneNumber");
                foundUser.TelephoneNumber = GetPropertyValue(ref tmp, "telephoneNumber");
                foundUser.HomePhone = GetPropertyValue(ref tmp, "homePhone");
                foundUser.IPPhone = GetPropertyValue(ref tmp, "ipPhone");
                foundUser.JobTitle = GetPropertyValue(ref tmp, "title");
                foundUser.MobilePhone = GetPropertyValue(ref tmp, "mobile");
                foundUser.Office = GetPropertyValue(ref tmp, "physicalDeliveryOfficeName");
                foundUser.Pager = GetPropertyValue(ref tmp, "pager");
                foundUser.POBox = GetPropertyValue(ref tmp, "postOfficeBox");
                foundUser.ScriptPath = GetPropertyValue(ref tmp, "scriptPath");
                foundUser.ProfilePath = GetPropertyValue(ref tmp, "profilePath");
                foundUser.Webpage = GetPropertyValue(ref tmp, "wWWHomePage");
                foundUser.Email = GetPropertyValue(ref tmp, "mail");
                foundUser.msExchMailboxGuid = GetPropertyValue(ref tmp, "msExchMailboxGuid", "guid");

                int flags = (int)tmp.Properties["userAccountControl"].Value;
                foundUser.IsEnabled = !Convert.ToBoolean(flags & 0x0002);

                return foundUser;
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error retrieving user {0}. Exception: {1}", identity, ex.ToString());
                throw;
            }
            finally
            {
                if (usr != null)
                    usr.Dispose();
            }
        }


        /// <summary>
        /// Get user information and properties including the photo
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public Users GetUserWithPhoto(string username)
        {
            UserPrincipal usr = null;

            Users foundUser = new Users();
            try
            {
                pc = GetPrincipalContext();

                logger.DebugFormat("Attempting to retrieve user {0}", username);
                usr = UserPrincipal.FindByIdentity(pc, IdentityType.UserPrincipalName, username);

                DirectoryEntry tmp = (DirectoryEntry)usr.GetUnderlyingObject();
                foundUser.UserGuid = tmp.Guid;
                foundUser.DistinguishedName = GetPropertyValue(ref tmp, "DistinguishedName");
                foundUser.BadPwdCount = GetPropertyValue(ref tmp, "badPwdCount", "int");
                foundUser.UserAccountControl = GetPropertyValue(ref tmp, "userAccountControl", "int");
                foundUser.SamAccountType = GetPropertyValue(ref tmp, "sAMAccountType", "int");
                foundUser.sAMAccountName = GetPropertyValue(ref tmp, "sAMAccountName");
                foundUser.Street = GetPropertyValue(ref tmp, "streetAddress");
                foundUser.City = GetPropertyValue(ref tmp, "l");
                foundUser.State = GetPropertyValue(ref tmp, "st");
                foundUser.PostalCode = GetPropertyValue(ref tmp, "postalCode");
                foundUser.Country = GetPropertyValue(ref tmp, "co");
                foundUser.CountryCode = GetPropertyValue(ref tmp, "c");
                foundUser.Company = GetPropertyValue(ref tmp, "company");
                foundUser.Department = GetPropertyValue(ref tmp, "department");
                foundUser.Description = GetPropertyValue(ref tmp, "description");
                foundUser.Firstname = GetPropertyValue(ref tmp, "givenName");
                foundUser.Lastname = GetPropertyValue(ref tmp, "sn");
                foundUser.DisplayName = GetPropertyValue(ref tmp, "displayName");
                foundUser.Name = GetPropertyValue(ref tmp, "name");
                foundUser.UserPrincipalName = GetPropertyValue(ref tmp, "userPrincipalName");
                foundUser.Fax = GetPropertyValue(ref tmp, "facsimileTelephoneNumber");
                foundUser.TelephoneNumber = GetPropertyValue(ref tmp, "telephoneNumber");
                foundUser.HomePhone = GetPropertyValue(ref tmp, "homePhone");
                foundUser.IPPhone = GetPropertyValue(ref tmp, "ipPhone");
                foundUser.JobTitle = GetPropertyValue(ref tmp, "title");
                foundUser.MobilePhone = GetPropertyValue(ref tmp, "mobile");
                foundUser.Office = GetPropertyValue(ref tmp, "physicalDeliveryOfficeName");
                foundUser.Pager = GetPropertyValue(ref tmp, "pager");
                foundUser.POBox = GetPropertyValue(ref tmp, "postOfficeBox");
                foundUser.ScriptPath = GetPropertyValue(ref tmp, "scriptPath");
                foundUser.ProfilePath = GetPropertyValue(ref tmp, "profilePath");
                foundUser.Webpage = GetPropertyValue(ref tmp, "wWWHomePage");
                foundUser.Email = GetPropertyValue(ref tmp, "mail");
                foundUser.msExchMailboxGuid = GetPropertyValue(ref tmp, "msExchMailboxGuid", "guid");

                // Get groups
                List<string> groups = new List<string>();
                foreach (var g in usr.GetAuthorizationGroups())
                {
                    groups.Add(g.Name);
                }
                foundUser.MemberOf = groups.ToArray();

                // Get photo
                if (tmp.Properties["thumbnailPhoto"] != null && tmp.Properties["thumbnailPhoto"].Count > 0)
                {
                    foundUser.ImageFromAD = tmp.Properties["thumbnailPhoto"][0] as byte[];
                }

                return foundUser;
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error retrieving user {0}. Exception: {1}", username, ex.ToString());
                throw;
            }
            finally
            {
                if (usr != null)
                    usr.Dispose();
            }
        }

        /// <summary>
        /// Gets a user's samAccountName
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public string GetUserSamAccountName(Guid userGuid)
        {
            UserPrincipal usr = null;
            try
            {
                pc = GetPrincipalContext();

                logger.DebugFormat("Attempting to retrieve user {0}", userGuid);
                usr = UserPrincipal.FindByIdentity(pc, IdentityType.Guid, userGuid.ToString());

                if (usr == null)
                    throw new Exception("Unable to find user " + userGuid);

                return usr.SamAccountName;
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error retrieving user samAccountName {0}. Exception: {1}", userGuid, ex.ToString());
                throw;
            }
            finally
            {
                if (usr != null)
                    usr.Dispose();
            }
        }

        /// <summary>
        /// Creates a new Active Directory user
        /// </summary>
        /// <param name="usersOU"></param>
        /// <param name="clearTextPassword"></param>
        /// <param name="userObject"></param>
        /// <returns></returns>
        public Users Create(string usersOU, string clearTextPassword, Users userObject, int samAccountNameFormat = 0, int cnFormat = 0)
        {
            PrincipalContext ctx = null;
            UserPrincipalExt usr = null;

            try
            {
                logger.Debug("Attempting to create new user");

                if (string.IsNullOrEmpty(usersOU))
                    throw new MissingFieldException("User", "usersOU");

                if (string.IsNullOrEmpty(clearTextPassword))
                    throw new MissingFieldException("User", "clearTextPassword");

                //if (string.IsNullOrEmpty(userObject.sAMAccountName))
                    //throw new MissingFieldException("User", "SamAccountName");

                if (string.IsNullOrEmpty(userObject.UserPrincipalName))
                    throw new MissingFieldException("User", "UserPrincipalName");

                if (string.IsNullOrEmpty(userObject.DisplayName))
                    throw new MissingFieldException("User", "DisplayName");

                //if (string.IsNullOrEmpty(userObject.Name))
                    //throw new MissingFieldException("User", "Name");

                // Check if the user exists
                pc = GetPrincipalContext(); // Used for querying purposes
                usr = UserPrincipalExt.FindByIdentity(pc, IdentityType.UserPrincipalName, userObject.UserPrincipalName);
                if (usr != null)
                    throw new PrincipalExistsException(userObject.UserPrincipalName + " already exists.");

                // Now we can create the user!
                logger.DebugFormat("User doesn't exist. Continuing...");
                userObject.Name = GetCNFormat(userObject, cnFormat);
                userObject.sAMAccountName = GetAvailableSamAccountName(userObject.UserPrincipalName, samAccountNameFormat);
                ctx = new PrincipalContext(ContextType.Domain, this._domainController, usersOU, this._username, this._password); // Used for creating new user
                
                usr = new UserPrincipalExt(ctx, userObject.sAMAccountName, clearTextPassword, true);
                usr.UserPrincipalName = userObject.UserPrincipalName;
                usr.DisplayName = userObject.DisplayName;
                usr.Name = userObject.Name;

                if (!string.IsNullOrEmpty(userObject.Firstname))
                    usr.GivenName = userObject.Firstname;

                if (!string.IsNullOrEmpty(userObject.Middlename))
                    usr.MiddleName = userObject.Middlename;

                if (!string.IsNullOrEmpty(userObject.Lastname))
                    usr.LastName = userObject.Lastname;

                if (!string.IsNullOrEmpty(userObject.Company))
                    usr.Company = userObject.Company;

                if (!string.IsNullOrEmpty(userObject.Department))
                    usr.Department = userObject.Department;

                if (!string.IsNullOrEmpty(userObject.JobTitle))
                    usr.JobTitle = userObject.JobTitle;

                if (!string.IsNullOrEmpty(userObject.TelephoneNumber))
                    usr.TelephoneNumber = userObject.TelephoneNumber;

                if (!string.IsNullOrEmpty(userObject.Fax))
                    usr.Fax = userObject.Fax;

                if (!string.IsNullOrEmpty(userObject.HomePhone))
                    usr.HomePhone = userObject.HomePhone;

                if (!string.IsNullOrEmpty(userObject.MobilePhone))
                    usr.MobilePhone = userObject.MobilePhone;

                if (!string.IsNullOrEmpty(userObject.Street))
                    usr.StreetAddress = userObject.Street;

                if (!string.IsNullOrEmpty(userObject.City))
                    usr.City = userObject.City;

                if (!string.IsNullOrEmpty(userObject.State))
                    usr.State = userObject.State;

                if (!string.IsNullOrEmpty(userObject.PostalCode))
                    usr.ZipCode = userObject.PostalCode;

                if (!string.IsNullOrEmpty(userObject.Country))
                    usr.Country = userObject.Country;

                logger.DebugFormat("Saving new user {0} in the directory store", userObject.UserPrincipalName);
                usr.Save();

                // After we save we need to return some data
                logger.DebugFormat("Getting new data for user {0} after saving in Active Directory", userObject.UserPrincipalName);
                userObject.UserGuid = (Guid)usr.Guid;
                userObject.DistinguishedName = usr.DistinguishedName;

                // Check if we should set change password next login flag
                if (userObject.ChangePasswordNextLogin)
                    usr.ExpirePasswordNow();

                logger.DebugFormat("User object {0} was created in Active Directory", userObject.DistinguishedName);
                return userObject;
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error retrieving user {0}. Exception: {1}", userObject.UserPrincipalName, ex.ToString());
                throw;
            }
            finally
            {
                if (usr != null)
                    usr.Dispose();
            }
        }

        /// <summary>
        /// Updates an Active Directory user
        /// </summary>
        /// <param name="userObject"></param>
        public void UpdateUser(Users userObject)
        {
            UserPrincipal usr = null;

            try
            {
                if (string.IsNullOrEmpty(userObject.UserPrincipalName))
                    throw new MissingFieldException("User", "UserPrincipalName");

                if (string.IsNullOrEmpty(userObject.DisplayName))
                    throw new MissingFieldException("User", "DisplayName");

                logger.DebugFormat("Updating user {0} values...", userObject.UserPrincipalName);

                pc = GetPrincipalContext();
                usr = UserPrincipal.FindByIdentity(pc, IdentityType.UserPrincipalName, userObject.UserPrincipalName);
                if (usr == null)
                    throw new NoMatchingPrincipalException(userObject.UserPrincipalName);

                DirectoryEntry deEntry = usr.GetUnderlyingObject() as DirectoryEntry;
                deEntry.Properties["DisplayName"].Value = userObject.DisplayName;

                logger.DebugFormat("Setting AD property values for {0}", userObject.UserPrincipalName);
                SetPropertyValue(ref deEntry, "givenName", userObject.Firstname);
                SetPropertyValue(ref deEntry, "sn", userObject.Lastname);
                SetPropertyValue(ref deEntry, "streetAddress", userObject.Street);
                SetPropertyValue(ref deEntry, "l", userObject.City);
                SetPropertyValue(ref deEntry, "st", userObject.State);
                SetPropertyValue(ref deEntry, "postalCode", userObject.PostalCode);
                SetPropertyValue(ref deEntry, "postOfficeBox", userObject.POBox);
                SetPropertyValue(ref deEntry, "co", userObject.Country);
                SetPropertyValue(ref deEntry, "c", userObject.CountryCode);
                SetPropertyValue(ref deEntry, "department", userObject.Department);
                SetPropertyValue(ref deEntry, "company", userObject.Company);
                SetPropertyValue(ref deEntry, "description", userObject.Description);
                SetPropertyValue(ref deEntry, "title", userObject.JobTitle);
                SetPropertyValue(ref deEntry, "facsimileTelephoneNumber", userObject.Fax);
                SetPropertyValue(ref deEntry, "telephoneNumber", userObject.TelephoneNumber);
                SetPropertyValue(ref deEntry, "homePhone", userObject.HomePhone);
                SetPropertyValue(ref deEntry, "mobile", userObject.MobilePhone);
                SetPropertyValue(ref deEntry, "pager", userObject.Pager);
                SetPropertyValue(ref deEntry, "ipPhone", userObject.IPPhone);
                SetPropertyValue(ref deEntry, "physicalDeliveryOfficeName", userObject.Office);
                SetPropertyValue(ref deEntry, "info", userObject.Notes);
                SetPropertyValue(ref deEntry, "wWWHomePage", userObject.Webpage);
                deEntry.CommitChanges();

                // Check if change password on next login was set
                if (userObject.ChangePasswordNextLogin)
                {
                    logger.DebugFormat("Change password on next login was set for {0}", userObject.UserPrincipalName);
                    usr.ExpirePasswordNow();
                }

                logger.InfoFormat("Successfully updated user {0}", userObject.UserPrincipalName);
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error updating {0}. Exception: {1}", userObject.UserPrincipalName, ex.ToString());
                throw;
            }
            finally
            {
                if (usr != null)
                    usr.Dispose();
            }
        }

        /// <summary>
        /// Updates a specific attribute in Active Directory
        /// </summary>
        /// <param name="userPrincipalName"></param>
        /// <param name="property"></param>
        /// <param name="newValue"></param>
        public void UpdateUserAttribute(string userPrincipalName, string property, string newValue)
        {
            UserPrincipal usr = null;

            try
            {
                if (string.IsNullOrEmpty(userPrincipalName))
                    throw new MissingFieldException("Users", "UserPrincipalName");

                if (string.IsNullOrEmpty(property))
                    throw new MissingFieldException("Users", "Property");

                logger.DebugFormat("Updating user {0} property {1} to {2}...", userPrincipalName, property, newValue);

                pc = GetPrincipalContext();
                usr = UserPrincipal.FindByIdentity(pc, IdentityType.UserPrincipalName, userPrincipalName);
                if (usr == null)
                    throw new NoMatchingPrincipalException(userPrincipalName);

                DirectoryEntry deEntry = usr.GetUnderlyingObject() as DirectoryEntry;
                SetPropertyValue(ref deEntry, property, newValue);

                deEntry.CommitChanges();
                logger.InfoFormat("Successfully updated user {0} property {1} to {2}.", userPrincipalName, property, newValue);
            }
            catch (Exception ex)
            {
                logger.InfoFormat("Error updating user {0} property {1}: {2}", userPrincipalName, property, ex.ToString());
                throw;
            }
            finally
            {
                if (usr != null)
                    usr.Dispose();
            }
        }

        /// <summary>
        /// Changes the login name
        /// </summary>
        /// <param name="userGuid">User's GUID</param>
        /// <param name="newUpn">New UserPrincipalNAme</param>
        /// <param name="newSamAccountName">New SamAcocuntName. If this is null then it will automatically generate it</param>
        /// <param name="name">"Name". If this is null then it will automatically use the new UserPrincipalName</param>
        public Users ChangeLogin(Guid userGuid, string newUpn, string newSamAccountName = null, string name = null, int samAccountNameFormat = 0)
        {
            UserPrincipal usr = null;

            try
            {
                if (userGuid == null)
                    throw new MissingFieldException("", "userGuid");

                if (string.IsNullOrEmpty(newUpn))
                    throw new MissingFieldException("", "newUpn");

                if (string.IsNullOrEmpty(newSamAccountName))
                    newSamAccountName = GetAvailableSamAccountName(newUpn, samAccountNameFormat);

                logger.DebugFormat("Changing user {0}'s login to {1}", userGuid, newUpn);

                pc = GetPrincipalContext();
                usr = UserPrincipal.FindByIdentity(pc, IdentityType.Guid, userGuid.ToString());
                if (usr == null)
                    throw new NoMatchingPrincipalException(userGuid.ToString());

                logger.DebugFormat("Updating first two values");
                usr.SamAccountName = newSamAccountName;
                usr.UserPrincipalName = newUpn;

                logger.DebugFormat("Saving userprincipal object");
                usr.Save();

                logger.DebugFormat("Updating 'Name' attribute");
                var de = usr.GetUnderlyingObject() as DirectoryEntry;
                if (!string.IsNullOrEmpty(name))
                    de.Rename("CN=" + name);
                else
                    de.Rename("CN=" + newUpn);
                de.CommitChanges();

                logger.InfoFormat("Successfully updated user login name for {0} to {1} with SamAccountName {2} and name {3}.", userGuid, newUpn, newSamAccountName, name);

                return new Users()
                {
                    UserPrincipalName = usr.UserPrincipalName,
                    sAMAccountName = usr.SamAccountName,
                    DistinguishedName = de.Properties["DistinguishedName"].Value.ToString()
                };
            }
            catch (Exception ex)
            {
                logger.InfoFormat("Error updating user login name for {0} to {1}: {2}", userGuid, newUpn, ex.ToString());
                throw;
            }
            finally
            {
                if (usr != null)
                    usr.Dispose();
            }
        }


        /// <summary>
        /// Deletes a user from Active Directory
        /// </summary>
        /// <param name="username"></param>
        public void Delete(Guid userGuid)
        {
            UserPrincipal usr = null;

            try
            {
                pc = GetPrincipalContext();

                logger.DebugFormat("Attempting to retrieve user {0}", userGuid);
                usr = UserPrincipal.FindByIdentity(pc, IdentityType.Guid, userGuid.ToString());

                if (usr != null)
                {
                    var directoryEntry = usr.GetUnderlyingObject() as DirectoryEntry;
                    directoryEntry.DeleteTree();
                    logger.InfoFormat("Successfully deleted user {0}", userGuid);
                }
                else
                    logger.InfoFormat("Attempted to delete user {0} but could not find the user in Active Directory. Assuming user was manually deleted... continuing...", userGuid);
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Failed to delete user {0}. Exception: {1}", userGuid, ex.ToString());
                throw;
            }
            finally
            {
                if (usr != null)
                    usr.Dispose();
            }
        }

        /// <summary>
        /// Reset the user's password
        /// </summary>
        /// <param name="userGuid"></param>
        /// <param name="newClearTextPassword"></param>
        public void ResetPassword(Guid userGuid, string newClearTextPassword)
        {
            UserPrincipal usr = null;
            try
            {
                logger.DebugFormat("Attempting to reset password for user {0}", userGuid);

                if (userGuid == null)
                    throw new MissingFieldException("Users", "UserGuid");

                if (string.IsNullOrEmpty(newClearTextPassword))
                    throw new MissingFieldException("Users", "Password");

                pc = GetPrincipalContext();
                usr = UserPrincipal.FindByIdentity(pc, IdentityType.Guid, userGuid.ToString());
                if (usr == null)
                    throw new NoMatchingPrincipalException(userGuid.ToString());

                usr.SetPassword(newClearTextPassword);

                logger.InfoFormat("Successfully reset password for {0}", userGuid);
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error resetting password for {0}. Exception {1}.", userGuid, ex.ToString());
                throw;
            }
            finally
            {
                if (usr != null)
                    usr.Dispose();
            }
        }

        /// <summary>
        /// Enables an Active Directory user
        /// </summary>
        /// <param name="userGuid"></param>
        public void EnableUser(Guid userGuid)
        {
            UserPrincipal usr = null;
            try
            {
                logger.DebugFormat("Attempting to enable user {0}", userGuid);

                if (userGuid == null)
                    throw new MissingFieldException("Users", "UserGuid");

                pc = GetPrincipalContext();
                usr = UserPrincipal.FindByIdentity(pc, IdentityType.Guid, userGuid.ToString());
                if (usr == null)
                    throw new NoMatchingPrincipalException(userGuid.ToString());

                usr.Enabled = true;
                usr.Save();

                logger.InfoFormat("Successfully enable user {0}", userGuid);
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error enabling user {0}. Exception {1}.", userGuid, ex.ToString());
                throw;
            }
            finally
            {
                if (usr != null)
                    usr.Dispose();
            }
        }

        /// <summary>
        /// Disables an Active Directory user
        /// </summary>
        /// <param name="userGuid"></param>
        public void DisableUser(Guid userGuid)
        {
            UserPrincipal usr = null;
            try
            {
                logger.DebugFormat("Attempting to disable user {0}", userGuid);

                if (userGuid == null)
                    throw new MissingFieldException("Users", "UserGuid");

                pc = GetPrincipalContext();
                usr = UserPrincipal.FindByIdentity(pc, IdentityType.Guid, userGuid.ToString());
                if (usr == null)
                    throw new NoMatchingPrincipalException(userGuid.ToString());

                usr.Enabled = false;
                usr.Save();

                logger.InfoFormat("Successfully disabled user {0}", userGuid);
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error disabling user {0}. Exception {1}.", userGuid, ex.ToString());
                throw;
            }
            finally
            {
                if (usr != null)
                    usr.Dispose();
            }
        }

        /// <summary>
        /// Adds a user to a security group
        /// </summary>
        /// <param name="groupName"></param>
        /// <param name="userPrincipalName"></param>
        public void AddToGroup(string groupName, string userPrincipalName)
        {
            GroupPrincipal gp = null;
            UserPrincipal usr = null;

            try
            {
                if (string.IsNullOrEmpty(groupName))
                    throw new MissingFieldException("Users", "groupName");

                if (string.IsNullOrEmpty(userPrincipalName))
                    throw new MissingFieldException("Users", "userPrincipalName");

                logger.DebugFormat("Attempting to add {0} to group {1}...", userPrincipalName, groupName);

                pc = GetPrincipalContext();
                gp = GroupPrincipal.FindByIdentity(pc, IdentityType.Name, groupName);
                if (gp == null)
                    throw new NoMatchingPrincipalException(groupName);

                usr = UserPrincipal.FindByIdentity(pc, IdentityType.UserPrincipalName, userPrincipalName);
                if (usr == null)
                    throw new NoMatchingPrincipalException(userPrincipalName);

                if (!gp.Members.Contains(usr))
                {
                    gp.Members.Add(usr);
                    gp.Save();

                    logger.InfoFormat("Successfully added {0} to group {1}.", userPrincipalName, groupName);
                }
                else
                    logger.DebugFormat("Did not add {0} to group {1} because the user was already a member.", userPrincipalName, groupName);
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error adding {0} to group {1}. Exception: {2}", userPrincipalName, groupName, ex.ToString());
                throw;
            }
            finally
            {
                if (usr != null)
                    usr.Dispose();

                if (gp != null)
                    gp.Dispose();
            }
        }

        /// <summary>
        /// Adds a user to an array of security group
        /// </summary>
        /// <param name="groupsName"></param>
        /// <param name="userPrincipalName"></param>
        public void AddToGroup(string[] groupsName, string userPrincipalName)
        {
            GroupPrincipal gp = null;
            UserPrincipal usr = null;

            try
            {
                if (groupsName == null || groupsName.Length < 1)
                    throw new MissingFieldException("Users", "groupsName");

                if (string.IsNullOrEmpty(userPrincipalName))
                    throw new MissingFieldException("Users", "userPrincipalName");
                
                pc = GetPrincipalContext();
                usr = UserPrincipal.FindByIdentity(pc, IdentityType.UserPrincipalName, userPrincipalName);
                if (usr == null)
                    throw new NoMatchingPrincipalException(userPrincipalName);
                else
                {
                    foreach (string group in groupsName)
                    {
                        logger.DebugFormat("Attempting to add {0} to group {1}", userPrincipalName, group);

                        gp = GroupPrincipal.FindByIdentity(pc, IdentityType.Name, group);
                        if (gp == null)
                            logger.WarnFormat("Unable to add {0} to group {1} because the group was not found.", userPrincipalName, group);
                        else
                        {
                            if (gp.Members.Contains(usr) == false)
                            {
                                gp.Members.Add(usr);
                                gp.Save();
                                logger.DebugFormat("Successfully added {0} to group {1}", userPrincipalName, group);
                            }
                            else
                                logger.DebugFormat("Unable to add {0} to group {1} because they were already a member", userPrincipalName, group);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error adding {0} to the following groups {1}. Exception: {2}", userPrincipalName, string.Join(", ", groupsName), ex.ToString());
                throw;
            }
            finally
            {
                if (usr != null)
                    usr.Dispose();

                if (gp != null)
                    gp.Dispose();
            }
        }

        /// <summary>
        /// Removes a user from a group
        /// </summary>
        /// <param name="groupName"></param>
        /// <param name="userPrincipalName"></param>
        public void RemoveFromGroup(string groupName, string userPrincipalName)
        {
            GroupPrincipal gp = null;
            UserPrincipal usr = null;

            try
            {
                if (string.IsNullOrEmpty(groupName))
                    throw new MissingFieldException("Users", "groupName");

                if (string.IsNullOrEmpty(userPrincipalName))
                    throw new MissingFieldException("Users", "userPrincipalName");

                logger.DebugFormat("Attempting to remove {0} from group {1}...", userPrincipalName, groupName);

                pc = GetPrincipalContext();
                gp = GroupPrincipal.FindByIdentity(pc, IdentityType.Name, groupName);
                if (gp == null)
                    throw new NoMatchingPrincipalException(groupName);

                usr = UserPrincipal.FindByIdentity(pc, IdentityType.UserPrincipalName, userPrincipalName);
                if (usr == null)
                    throw new NoMatchingPrincipalException(userPrincipalName);

                if (gp.Members.Contains(usr))
                {
                    gp.Members.Remove(usr);
                    gp.Save();

                    logger.InfoFormat("Successfully removed {0} from group {1}.", userPrincipalName, groupName);
                }
                else
                    logger.DebugFormat("Did not remove {0} from group {1} because the user was not a member.", userPrincipalName, groupName);
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error removing {0} from group {1}. Exception: {2}", userPrincipalName, groupName, ex.ToString());
                throw;
            }
            finally
            {
                if (usr != null)
                    usr.Dispose();

                if (gp != null)
                    gp.Dispose();
            }
        }

        /// <summary>
        /// Removes a user from an array of security groups
        /// </summary>
        /// <param name="groupsName"></param>
        /// <param name="userPrincipalName"></param>
        public void RemoveToGroup(string[] groupsName, string userPrincipalName)
        {
            GroupPrincipal gp = null;
            UserPrincipal usr = null;

            try
            {
                if (groupsName == null || groupsName.Length < 1)
                    throw new MissingFieldException("Users", "groupsName");

                if (string.IsNullOrEmpty(userPrincipalName))
                    throw new MissingFieldException("Users", "userPrincipalName");

                pc = GetPrincipalContext();
                usr = UserPrincipal.FindByIdentity(pc, IdentityType.UserPrincipalName, userPrincipalName);
                if (usr == null)
                    throw new NoMatchingPrincipalException(userPrincipalName);
                else
                {
                    foreach (string group in groupsName)
                    {
                        logger.DebugFormat("Attempting to remove {0} from group {1}", userPrincipalName, group);

                        gp = GroupPrincipal.FindByIdentity(pc, IdentityType.Name, group);
                        if (gp == null)
                            logger.WarnFormat("Unable to remove {0} from group {1} because the group was not found.", userPrincipalName, group);
                        else
                        {
                            if (gp.Members.Contains(usr) == true)
                            {
                                gp.Members.Remove(usr);
                                gp.Save();
                                logger.DebugFormat("Successfully removed {0} from group {1}", userPrincipalName, group);
                            }
                            else
                                logger.DebugFormat("Unable to remove {0} from group {1} because they were not a member", userPrincipalName, group);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error removing {0} from the following groups {1}. Exception: {2}", userPrincipalName, string.Join(", ", groupsName), ex.ToString());
                throw;
            }
            finally
            {
                if (usr != null)
                    usr.Dispose();

                if (gp != null)
                    gp.Dispose();
            }
        }

        /// <summary>
        /// Gets the photo from Active Directory
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public byte[] GetPhoto(string username)
        {
            UserPrincipal usr = null;

            try
            {
                pc = GetPrincipalContext();

                logger.DebugFormat("Attempting to retrieve user photo for {0}", username);
                usr = UserPrincipal.FindByIdentity(pc, IdentityType.UserPrincipalName, username);

                DirectoryEntry tmp = (DirectoryEntry)usr.GetUnderlyingObject();

                if (tmp.Properties["thumbnailPhoto"] != null && tmp.Properties["thumbnailPhoto"].Count > 0)
                    return tmp.Properties["thumbnailPhoto"][0] as byte[];
                else
                    return null;
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error retrieving user photo for {0}. Exception: {1}", username, ex.ToString());
                throw;
            }
            finally
            {
                if (usr != null)
                    usr.Dispose();
            }
        }

        #region Private Methods

        private void SetPropertyValue(ref DirectoryEntry dEntry, string property, object newValue)
        {
            logger.DebugFormat("Setting propery {0} to new value {1}", property, newValue);

            if (newValue == null)
                dEntry.Properties[property].Clear();
            else if (newValue is string && string.IsNullOrEmpty(newValue.ToString()))
                dEntry.Properties[property].Clear();
            else
            {
                dEntry.Properties[property].Value = newValue;
            }
        }

        private dynamic GetPropertyValue(ref DirectoryEntry dEntry, string property, string expectedType = "string")
        {
            logger.DebugFormat("Parsing property {0} with expected type {1}", property, expectedType);

            if (dEntry.Properties[property] != null && dEntry.Properties[property].Value != null)
            {
                if (dEntry.Properties[property].Value is byte[] && expectedType == "guid")
                    return new Guid((byte[])dEntry.Properties[property].Value);
                else
                    return dEntry.Properties[property].Value;
            }
            else
            {
                switch (expectedType.ToLower())
                {
                    case "long":
                    case "int":
                        return 0;
                    case "guid":
                        return Guid.Parse("00000000-0000-0000-0000-000000000000");
                    default:
                        return string.Empty;
                }
            }
        }

        private string GetAvailableSamAccountName(string userPrincipalName, int samAccountNameFormat)
        {
            DirectorySearcher ds = null;

            try
            {
                logger.DebugFormat("Attempting to find an available sAMAccountName for {0} with format {1}.", userPrincipalName, samAccountNameFormat);

                // Invalid characters
                string strippedUpn = new string(userPrincipalName.Where(x => char.IsLetterOrDigit(x) || x == '@').ToArray());
                
                // Get the first part of the user principal name
                string[] split = strippedUpn.Split('@');
                string before = split[0];
                string after = split[1];

                string samAccountName = string.Empty;
                switch (samAccountNameFormat)
                {
                    case 1:
                        string start = before.Length > 9 ? before.Substring(0, 8) : before;
                        string last = after.Length > 9 ? after.Substring(0, 8) : after;
                        samAccountName = string.Format("{0}_{1}", start, last);
                        break;
                    default:
                        samAccountName = before;
                        break;
                }

                de = GetDirectoryEntry();
                ds = new DirectorySearcher(de);
                ds.SearchScope = SearchScope.Subtree;
                ds.Filter = string.Format("(sAMAccountName={0})", samAccountName);

                int count = 0;
                string originalSamAccountName = samAccountName;
                while (ds.FindOne() != null)
                {
                    count = count + 1;
                    samAccountName = string.Format("{0}{1}", originalSamAccountName, count.ToString());
                    ds.Filter = string.Format("(sAMAccountName={0})", samAccountName);
                }

                // We found our available sAMAccountName
                logger.DebugFormat("Available sAMAccountName was found: {0}", samAccountName);
                return samAccountName;
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error searching for available SamAccountName for {0}. Exception: {1} ", userPrincipalName, ex.ToString());
                throw;
            }
            finally
            {
                if (ds != null)
                    ds.Dispose();
            }
        }

        private string GetCNFormat(Users user, int cnFormat)
        {
            switch (cnFormat)
            {
                case 1:
                    return user.UserPrincipalName;
                default:
                    return user.DisplayName;
            }
        }
        #endregion

        #region Dispose

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    if (pc != null)
                        pc.Dispose();

                    if (de != null)
                        de.Dispose();
                }

                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}

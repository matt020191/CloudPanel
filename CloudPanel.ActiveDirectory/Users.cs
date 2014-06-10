using CloudPanel.ActiveDirectory.Extensions;
using CloudPanel.Base.AD;
using log4net;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.IO;
using System.Linq;
using System.Text;

namespace CloudPanel.ActiveDirectory
{
    public class Users : IDisposable
    {
        private readonly ILog log = LogManager.GetLogger(typeof(Users));

        private string _username;
        private string _password;
        private string _domainController;
        private string _ldapStr;
        private bool _disposed;

        internal DirectoryEntry de = null;
        internal PrincipalContext pc = null;

        public Users(string username, string decryptedPassword, string domainController)
        {
            this._username = username;
            this._password = decryptedPassword;
            this._domainController = domainController;

            this._ldapStr = string.Format("LDAP://{0}/", domainController);
        }

        #region Initialization

        private DirectoryEntry GetDirectoryEntry(string dnPath = null)
        {
            log.DebugFormat("Retrieving the DirectoryEntry... Path: {0}", dnPath);

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
            log.Debug("Retrieving the PrincipalContext...");

            if (pc == null)
                pc = new PrincipalContext(ContextType.Domain, this._domainController, this._username, this._password);

            return pc;
        }

        #endregion

        public User Authenticate(string username, string password)
        {
            try
            {
                pc = GetPrincipalContext();

                log.DebugFormat("Attempting to authenticate user {0}", username);

                bool valid = pc.ValidateCredentials(username, password);
                if (!valid)
                    throw new UnauthorizedAccessException();
                else
                {
                    log.DebugFormat("Successfully authenticated user {0}... Now retrieving security groups...", username);

                    return GetUser(username);
                }                
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error authenticating user {0}. Exception: {1}", username, ex.ToString());
                throw;
            }
        }

        public User GetUser(string username)
        {
            UserPrincipal usr = null;

            User foundUser = new User();
            try
            {
                pc = GetPrincipalContext();

                log.DebugFormat("Attempting to retrieve user {0}", username);
                usr = UserPrincipal.FindByIdentity(pc, IdentityType.UserPrincipalName, username);

                DirectoryEntry tmp = (DirectoryEntry)usr.GetUnderlyingObject();
                foundUser.AccountExpires = GetPropertyValue(ref tmp, "accountExpires", "long");
                foundUser.BadPasswordTime = GetPropertyValue(ref tmp, "badPasswordTime", "long");
                foundUser.BadPwdCount = GetPropertyValue(ref tmp, "badPwdCount", "int");
                foundUser.UserAccountControl = GetPropertyValue(ref tmp, "userAccountControl", "int");
                foundUser.PwdLastSet = GetPropertyValue(ref tmp, "pwdLastSet", "long");
                foundUser.SamAccountType = GetPropertyValue(ref tmp, "sAMAccountType", "int");
                foundUser.ObjectGUID = GetPropertyValue(ref tmp, "objectGuid");
                foundUser.Street = GetPropertyValue(ref tmp, "streetAddress");
                foundUser.City = GetPropertyValue(ref tmp, "l");
                foundUser.State = GetPropertyValue(ref tmp, "st");
                foundUser.PostalCode = GetPropertyValue(ref tmp, "postalCode");
                foundUser.Country = GetPropertyValue(ref tmp, "co");
                foundUser.CountryCode = GetPropertyValue(ref tmp, "c");
                foundUser.Company = GetPropertyValue(ref tmp, "company");
                foundUser.Department = GetPropertyValue(ref tmp, "department");
                foundUser.Description = GetPropertyValue(ref tmp, "description");
                foundUser.FirstName = GetPropertyValue(ref tmp, "givenName");
                foundUser.LastName = GetPropertyValue(ref tmp, "sn");
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

                // Get groups
                List<string> groups = new List<string>();
                foreach (var g in usr.GetAuthorizationGroups())
                {
                    groups.Add(g.Name);
                }
                foundUser.MemberOf = groups.ToArray();

                return foundUser;
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error retrieving user {0}. Exception: {1}", username, ex.ToString());
                throw;
            }
            finally
            {
                if (usr != null)
                    usr.Dispose();
            }
        }

        public User GetUserWithPhoto(string username)
        {
            UserPrincipal usr = null;

            User foundUser = new User();
            try
            {
                pc = GetPrincipalContext();

                log.DebugFormat("Attempting to retrieve user {0}", username);
                usr = UserPrincipal.FindByIdentity(pc, IdentityType.UserPrincipalName, username);

                DirectoryEntry tmp = (DirectoryEntry)usr.GetUnderlyingObject();
                foundUser.AccountExpires = GetPropertyValue(ref tmp, "accountExpires", "long");
                foundUser.BadPasswordTime = GetPropertyValue(ref tmp, "badPasswordTime", "long");
                foundUser.BadPwdCount = GetPropertyValue(ref tmp, "badPwdCount", "int");
                foundUser.UserAccountControl = GetPropertyValue(ref tmp, "userAccountControl", "int");
                foundUser.PwdLastSet = GetPropertyValue(ref tmp, "pwdLastSet", "long");
                foundUser.SamAccountType = GetPropertyValue(ref tmp, "sAMAccountType", "int");
                foundUser.ObjectGUID = GetPropertyValue(ref tmp, "objectGuid");
                foundUser.Street = GetPropertyValue(ref tmp, "streetAddress");
                foundUser.City = GetPropertyValue(ref tmp, "l");
                foundUser.State = GetPropertyValue(ref tmp, "st");
                foundUser.PostalCode = GetPropertyValue(ref tmp, "postalCode");
                foundUser.Country = GetPropertyValue(ref tmp, "co");
                foundUser.CountryCode = GetPropertyValue(ref tmp, "c");
                foundUser.Company = GetPropertyValue(ref tmp, "company");
                foundUser.Department = GetPropertyValue(ref tmp, "department");
                foundUser.Description = GetPropertyValue(ref tmp, "description");
                foundUser.FirstName = GetPropertyValue(ref tmp, "givenName");
                foundUser.LastName = GetPropertyValue(ref tmp, "sn");
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
                log.ErrorFormat("Error retrieving user {0}. Exception: {1}", username, ex.ToString());
                throw;
            }
            finally
            {
                if (usr != null)
                    usr.Dispose();
            }
        }

        public User Create(string usersOU, string clearTextPassword, User userObject)
        {
            PrincipalContext ctx = null;
            UserPrincipalExt usr = null;

            try
            {
                log.Debug("Attempting to create new user");

                if (string.IsNullOrEmpty(usersOU))
                    throw new MissingFieldException("User", "usersOU");

                if (string.IsNullOrEmpty(clearTextPassword))
                    throw new MissingFieldException("User", "clearTextPassword");

                if (string.IsNullOrEmpty(userObject.SamAccountName))
                    throw new MissingFieldException("User", "SamAccountName");

                if (string.IsNullOrEmpty(userObject.UserPrincipalName))
                    throw new MissingFieldException("User", "UserPrincipalName");

                if (string.IsNullOrEmpty(userObject.FirstName))
                    throw new MissingFieldException("User", "FirstName");

                if (string.IsNullOrEmpty(userObject.DisplayName))
                    throw new MissingFieldException("User", "DisplayName");

                if (string.IsNullOrEmpty(userObject.Name))
                    throw new MissingFieldException("User", "Name");

                // Check if the user exists
                pc = GetPrincipalContext(); // Used for querying purposes
                usr = UserPrincipalExt.FindByIdentity(pc, IdentityType.UserPrincipalName, userObject.UserPrincipalName);
                if (usr != null)
                    throw new PrincipalExistsException(userObject.UserPrincipalName);

                // Now we can create the user!
                userObject.SamAccountName = GetAvailableSamAccountName(userObject.UserPrincipalName);
                ctx = new PrincipalContext(ContextType.Domain, this._domainController, usersOU, this._username, this._password); // Used for creating new user
                usr = new UserPrincipalExt(ctx, userObject.SamAccountName, clearTextPassword, true);
                usr.UserPrincipalName = userObject.UserPrincipalName;
                usr.DisplayName = userObject.DisplayName;
                usr.Name = userObject.Name;
                usr.GivenName = userObject.FirstName;

                if (!string.IsNullOrEmpty(userObject.LastName))
                    usr.LastName = userObject.LastName;

                if (!string.IsNullOrEmpty(userObject.Department))
                    usr.Department = userObject.Department;

                usr.Save();

                // After we save we need to return some data
                userObject.ObjectGUID = (Guid)usr.Guid;
                userObject.DistinguishedName = usr.DistinguishedName;

                return userObject;
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error retrieving user {0}. Exception: {1}", userObject.UserPrincipalName, ex.ToString());
                throw;
            }
            finally
            {
                if (usr != null)
                    usr.Dispose();
            }
        }

        public void UpdateUser(User userObject)
        {
            UserPrincipal usr = null;

            try
            {
                if (string.IsNullOrEmpty(userObject.UserPrincipalName))
                    throw new MissingFieldException("User", "UserPrincipalName");

                if (string.IsNullOrEmpty(userObject.FirstName))
                    throw new MissingFieldException("User", "FirstName");

                if (string.IsNullOrEmpty(userObject.DisplayName))
                    throw new MissingFieldException("User", "DisplayName");

                log.DebugFormat("Updating user {0} values...", userObject.UserPrincipalName);

                pc = GetPrincipalContext();
                usr = UserPrincipal.FindByIdentity(pc, IdentityType.UserPrincipalName, userObject.UserPrincipalName);
                if (usr == null)
                    throw new NoMatchingPrincipalException(userObject.UserPrincipalName);

                DirectoryEntry deEntry = usr.GetUnderlyingObject() as DirectoryEntry;
                deEntry.Properties["givenName"].Value = userObject.FirstName;
                deEntry.Properties["DisplayName"].Value = userObject.DisplayName;
                
                SetPropertyValue(ref deEntry, "sn", userObject.LastName);
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
                SetPropertyValue(ref deEntry, "homePhone", userObject.HomePhone);
                SetPropertyValue(ref deEntry, "mobile", userObject.MobilePhone);
                SetPropertyValue(ref deEntry, "pager", userObject.Pager);
                SetPropertyValue(ref deEntry, "ipPhone", userObject.IPPhone);
                SetPropertyValue(ref deEntry, "physicalDeliveryOfficeName", userObject.Office);
                SetPropertyValue(ref deEntry, "info", userObject.Notes);
                SetPropertyValue(ref deEntry, "wWWHomePage", userObject.Webpage);

                deEntry.CommitChanges();
                log.InfoFormat("Successfully updated user {0}", userObject.UserPrincipalName);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error updating {0}. Exception: {1}", userObject.UserPrincipalName, ex.ToString());
                throw;
            }
            finally
            {
                if (usr != null)
                    usr.Dispose();
            }
        }

        public void UpdateUserAttribute(string userPrincipalName, string property, string newValue)
        {
            UserPrincipal usr = null;

            try
            {
                if (string.IsNullOrEmpty(userPrincipalName))
                    throw new MissingFieldException("Users", "UserPrincipalName");

                if (string.IsNullOrEmpty(property))
                    throw new MissingFieldException("Users", "Property");

                log.DebugFormat("Updating user {0} property {1} to {2}...", userPrincipalName, property, newValue);

                pc = GetPrincipalContext();
                usr = UserPrincipal.FindByIdentity(pc, IdentityType.UserPrincipalName, userPrincipalName);
                if (usr == null)
                    throw new NoMatchingPrincipalException(userPrincipalName);

                DirectoryEntry deEntry = usr.GetUnderlyingObject() as DirectoryEntry;
                SetPropertyValue(ref deEntry, property, newValue);

                deEntry.CommitChanges();
                log.InfoFormat("Successfully updated user {0} property {1} to {2}.", userPrincipalName, property, newValue);
            }
            catch (Exception ex)
            {
                log.InfoFormat("Error updating user {0} property {1}.", userPrincipalName, property);
                throw;
            }
            finally
            {
                if (usr != null)
                    usr.Dispose();
            }
        }

        public void Delete(string username)
        {
            UserPrincipal usr = null;

            try
            {
                pc = GetPrincipalContext();

                log.DebugFormat("Attempting to retrieve user {0}", username);
                usr = UserPrincipal.FindByIdentity(pc, IdentityType.UserPrincipalName, username);

                if (usr != null)
                {
                    usr.Delete();
                    log.InfoFormat("Successfully deleted user {0}", username);
                }
                else
                    log.InfoFormat("Attempted to delete user {0} but could not find the user in Active Directory. Assuming user was manually deleted... continuing...", username);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Failed to delete user {0}. Exception: {1}", username, ex.ToString());
                throw;
            }
            finally
            {
                if (usr != null)
                    usr.Dispose();
            }
        }

        public void ResetPassword(string username, string newClearTextPassword)
        {
            UserPrincipal usr = null;
            try
            {
                log.DebugFormat("Attempting to reset password for user {0}", username);

                if (string.IsNullOrEmpty(username))
                    throw new MissingFieldException("Users", "username");

                if (string.IsNullOrEmpty(newClearTextPassword))
                    throw new MissingFieldException("Users", "Password");

                pc = GetPrincipalContext();
                usr = UserPrincipal.FindByIdentity(pc, IdentityType.UserPrincipalName, username);
                if (usr == null)
                    throw new NoMatchingPrincipalException(username);

                usr.SetPassword(newClearTextPassword);

                log.InfoFormat("Successfully reset password for {0}", username);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error resetting password for {0}. Exception {1}.", username, ex.ToString());
                throw;
            }
            finally
            {
                if (usr != null)
                    usr.Dispose();
            }
        }

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

                log.DebugFormat("Attempting to add {0} to group {1}...", userPrincipalName, groupName);

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

                    log.InfoFormat("Successfully added {0} to group {1}.", userPrincipalName, groupName);
                }
                else
                    log.DebugFormat("Did not add {0} to group {1} because the user was already a member.", userPrincipalName, groupName);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error adding {0} to group {1}. Exception: {2}", userPrincipalName, groupName, ex.ToString());
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
                        log.DebugFormat("Attempting to add {0} to group {1}", userPrincipalName, group);

                        gp = GroupPrincipal.FindByIdentity(pc, IdentityType.Name, group);
                        if (gp == null)
                            log.WarnFormat("Unable to add {0} to group {1} because the group was not found.", userPrincipalName, group);
                        else
                        {
                            if (gp.Members.Contains(usr) == false)
                            {
                                gp.Members.Add(usr);
                                gp.Save();
                                log.DebugFormat("Successfully added {0} to group {1}", userPrincipalName, group);
                            }
                            else
                                log.DebugFormat("Unable to add {0} to group {1} because they were already a member", userPrincipalName, group);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error adding {0} to the following groups {1}. Exception: {2}", userPrincipalName, string.Join(", ", groupsName), ex.ToString());
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

                log.DebugFormat("Attempting to remove {0} from group {1}...", userPrincipalName, groupName);

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

                    log.InfoFormat("Successfully removed {0} from group {1}.", userPrincipalName, groupName);
                }
                else
                    log.DebugFormat("Did not remove {0} from group {1} because the user was not a member.", userPrincipalName, groupName);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error removing {0} from group {1}. Exception: {2}", userPrincipalName, groupName, ex.ToString());
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
                        log.DebugFormat("Attempting to remove {0} from group {1}", userPrincipalName, group);

                        gp = GroupPrincipal.FindByIdentity(pc, IdentityType.Name, group);
                        if (gp == null)
                            log.WarnFormat("Unable to remove {0} from group {1} because the group was not found.", userPrincipalName, group);
                        else
                        {
                            if (gp.Members.Contains(usr) == true)
                            {
                                gp.Members.Remove(usr);
                                gp.Save();
                                log.DebugFormat("Successfully removed {0} from group {1}", userPrincipalName, group);
                            }
                            else
                                log.DebugFormat("Unable to remove {0} from group {1} because they were not a member", userPrincipalName, group);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error removing {0} from the following groups {1}. Exception: {2}", userPrincipalName, string.Join(", ", groupsName), ex.ToString());
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

        public byte[] GetPhoto(string username)
        {
            UserPrincipal usr = null;

            try
            {
                pc = GetPrincipalContext();

                log.DebugFormat("Attempting to retrieve user photo for {0}", username);
                usr = UserPrincipal.FindByIdentity(pc, IdentityType.UserPrincipalName, username);

                DirectoryEntry tmp = (DirectoryEntry)usr.GetUnderlyingObject();

                if (tmp.Properties["thumbnailPhoto"] != null && tmp.Properties["thumbnailPhoto"].Count > 0)
                    return tmp.Properties["thumbnailPhoto"][0] as byte[];
                else
                    return null;
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error retrieving user photo for {0}. Exception: {1}", username, ex.ToString());
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
            if (dEntry.Properties[property] != null)
                return dEntry.Properties[property].Value;
            else
            {
                switch (expectedType.ToLower())
                {
                    case "long":
                    case "int":
                        return 0;
                    default:
                        return string.Empty;
                }
            }
        }

        private string GetAvailableSamAccountName(string userPrincipalName)
        {
            DirectorySearcher ds = null;

            try
            {
                log.DebugFormat("Attempting to find an available sAMAccountName for {0}.", userPrincipalName);

                // Get the first part of the user principal name
                string upnFirstPart = userPrincipalName.Split('@')[0];
                string sAMAccountName = upnFirstPart;

                de = GetDirectoryEntry();
                ds = new DirectorySearcher(de);
                ds.SearchScope = SearchScope.Subtree;
                ds.Filter = string.Format("(sAMAccountName={0})", upnFirstPart);

                int count = 0;
                while (ds.FindOne() != null)
                {
                    count = count + 1;

                    sAMAccountName = string.Format("{0}{1}", upnFirstPart, count.ToString());

                    ds.Filter = string.Format("(sAMAccountName={0})", sAMAccountName);
                }

                // We found our available sAMAccountName
                return sAMAccountName;
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error searching for available SamAccountName for {0}. Exception: {1} ", userPrincipalName, ex.ToString());
                throw;
            }
            finally
            {
                if (ds != null)
                    ds.Dispose();
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

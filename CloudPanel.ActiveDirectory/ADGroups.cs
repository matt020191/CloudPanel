using CloudPanel.Base.AD;
using CloudPanel.Base.Models.Database;
using log4net;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Text;

namespace CloudPanel.ActiveDirectory
{
    public class ADGroups : IDisposable
    {
        private readonly ILog log = LogManager.GetLogger(typeof(ADGroups));

        private string _username;
        private string _password;
        private string _domainController;
        private string _ldapStr;
        private bool _disposed;

        internal DirectoryEntry de = null;
        internal PrincipalContext pc = null;

        public ADGroups(string username, string decryptedPassword, string domainController)
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

        /// <summary>
        /// Creates a new security group and returns the object created
        /// </summary>
        /// <param name="parentOU"></param>
        /// <param name="group"></param>
        /// <returns></returns>
        public SecurityGroup Create(string parentOU, SecurityGroup group)
        {
            PrincipalContext ctx = null;
            GroupPrincipal grp = null;

            try
            {
                log.DebugFormat("Creating a new group {0} in path {1}", group.Name, parentOU);

                if (string.IsNullOrEmpty(parentOU))
                    throw new MissingFieldException("You must provide the parent organizational unit");

                if (string.IsNullOrEmpty(group.Name))
                    throw new MissingFieldException("The Name attribute of the group was not specified");

                if (string.IsNullOrEmpty(group.SamAccountName))
                    throw new MissingFieldException("The sAMAccountName was not specified");

                //if (group.SamAccountName.Length > 19)
                    //throw new ArgumentOutOfRangeException(group.SamAccountName);

                log.DebugFormat("Prerequisites were satisfied. Checking if group exists or not...");
                ctx = new PrincipalContext(ContextType.Domain, _domainController, _username, _password);
                grp = GroupPrincipal.FindByIdentity(ctx, IdentityType.Name, group.Name);

                if (grp == null)
                {
                    ctx = new PrincipalContext(ContextType.Domain, _domainController, parentOU, _username, _password);
                    grp = new GroupPrincipal(ctx, group.SamAccountName);
                    grp.Name = group.Name;
                    grp.IsSecurityGroup = true;
                    grp.GroupScope = group.IsUniversalGroup ? GroupScope.Universal : GroupScope.Global;

                    if (!string.IsNullOrEmpty(group.Description))
                        grp.Description = group.Description;

                    if (!string.IsNullOrEmpty(group.DisplayName))
                        grp.DisplayName = group.DisplayName;

                    grp.Save();
                }

                log.DebugFormat("Successfully created new group {0}", group.Name);
                group.DistinguishedName = grp.DistinguishedName;
                group.ObjectGUID = (Guid)grp.Guid;

                return group;
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error creating new group {0}. Exception: {1}", group.Name, ex.ToString());
                throw;
            }
            finally
            {
                if (grp != null)
                    grp.Dispose();

                if (ctx != null)
                    ctx.Dispose();
            }
        }

        /// <summary>
        /// Get a list of alll users that are a member of a certain security group
        /// </summary>
        /// <param name="groupName"></param>
        /// <returns></returns>
        public List<Users> GetUsers(string groupName)
        {
            GroupPrincipal grp = null;

            List<Users> users = new List<Users>();
            try
            {
                log.DebugFormat("Attempting to retrieve all users that are a direct member of {0}", groupName);

                pc = GetPrincipalContext();
                grp = GroupPrincipal.FindByIdentity(pc, IdentityType.Name, groupName);
                if (grp == null)
                    throw new NoMatchingPrincipalException(groupName);
                else
                {
                    log.DebugFormat("Found group {0}... Now attempting to retrieve direct members that are users.", groupName);
                    foreach (var obj in grp.GetMembers(false))
                    {
                        log.DebugFormat("Found object {0} of objectClass {1} when retrieving users from {2}", obj.Name, obj.StructuralObjectClass, groupName);

                        if (obj.StructuralObjectClass.IndexOf("user") >= 0)
                        {
                            log.DebugFormat("Object {0} is a user! Adding to our list.", obj.Name);
                            DirectoryEntry userDE = obj.GetUnderlyingObject() as DirectoryEntry;
                            users.Add(new Users()
                                {
                                    UserGuid = (Guid)userDE.Properties["objectGUID"].Value,
                                    UserPrincipalName = userDE.Properties["UserPrincipalName"].Value.ToString(),
                                    Name = userDE.Properties["Name"].Value.ToString(),
                                    DisplayName = userDE.Properties["DisplayName"].Value.ToString()
                                });
                        }
                        else
                            log.DebugFormat("Object {0} was skipped since it was not a user", obj.Name);
                    }
                }

                return users;
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error retrieving users that are a member of group {0}. Exception: {1}", groupName, ex.ToString());
                throw;
            }
            finally
            {
                if (grp != null)
                    grp.Dispose();
            }
        }

        /// <summary>
        /// Delete a security group
        /// </summary>
        /// <param name="groupName"></param>
        public void Delete(string groupName)
        {
            GroupPrincipal gp = null;

            try
            {
                if (string.IsNullOrEmpty(groupName))
                    throw new MissingFieldException("Users", "groupName");

                log.DebugFormat("Attempting to delete group {0}", groupName);

                pc = GetPrincipalContext();
                gp = GroupPrincipal.FindByIdentity(pc, IdentityType.Name, groupName);
                if (gp == null)
                    throw new NoMatchingPrincipalException(groupName);

                gp.Delete();
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error deleting group {0}. Exception: {1}", groupName, ex.ToString());
                throw;
            }
            finally
            {
                if (gp != null)
                    gp.Dispose();
            }
        }

        /// <summary>
        /// Add a user to a security group by the userPrincipalName
        /// </summary>
        /// <param name="groupName"></param>
        /// <param name="userPrincipalName"></param>
        public void AddUser(string groupName, string userPrincipalName)
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

        /// <summary>
        /// Add a list of users to a security group by the userGuid values
        /// </summary>
        /// <param name="groupName"></param>
        /// <param name="userGuids"></param>
        public void AddUser(string groupName, string[] userGuids)
        {
            GroupPrincipal gp = null;
            UserPrincipal usr = null;

            try
            {
                if (string.IsNullOrEmpty(groupName))
                    throw new MissingFieldException("Users", "groupName");

                if (userGuids == null || userGuids.Length < 1)
                    throw new MissingFieldException("Users", "userPrincipalName");

                log.DebugFormat("Attempting to add {0} to group {1}...", String.Join(",", userGuids), groupName);

                pc = GetPrincipalContext();
                gp = GroupPrincipal.FindByIdentity(pc, IdentityType.Name, groupName);
                if (gp == null)
                    throw new NoMatchingPrincipalException(groupName);

                foreach (var userGuid in userGuids)
                {
                    usr = UserPrincipal.FindByIdentity(pc, IdentityType.Guid, userGuid);
                    if (!gp.Members.Contains(usr))
                    {
                        gp.Members.Add(usr);
                        log.DebugFormat("Added {0} to group {1}", usr.UserPrincipalName, gp.Name);
                    }
                }

                gp.Save();
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error adding {0} to group {1}. Exception: {2}", String.Join(",", userGuids), groupName, ex.ToString());
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
        /// Add a user to a list of security groups
        /// </summary>
        /// <param name="groupsName"></param>
        /// <param name="userPrincipalName"></param>
        public void AddUser(string[] groupsName, string userPrincipalName)
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

        /// <summary>
        /// Removes a single user from a security group using the UserPrincipalName
        /// </summary>
        /// <param name="groupName"></param>
        /// <param name="userPrincipalName"></param>
        public void RemoveUser(string groupName, string userPrincipalName)
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

        /// <summary>
        /// Remove a single user from a list of security groups
        /// </summary>
        /// <param name="groupsName"></param>
        /// <param name="userPrincipalName"></param>
        public void RemoveUser(string[] groupsName, string userPrincipalName)
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

        /// <summary>
        /// Remove a list of users from a single security group
        /// </summary>
        /// <param name="groupName"></param>
        /// <param name="userGuids"></param>
        public void RemoveUser(string groupName, string[] userGuids)
        {
            GroupPrincipal gp = null;
            UserPrincipal usr = null;

            try
            {
                if (string.IsNullOrEmpty(groupName))
                    throw new MissingFieldException("Users", "groupName");

                if (userGuids == null || userGuids.Length < 1)
                    throw new MissingFieldException("Users", "userPrincipalName");

                log.DebugFormat("Attempting to remove {0} from group {1}...", String.Join(",", userGuids), groupName);

                pc = GetPrincipalContext();
                gp = GroupPrincipal.FindByIdentity(pc, IdentityType.Name, groupName);
                if (gp == null)
                    throw new NoMatchingPrincipalException(groupName);

                foreach (var userGuid in userGuids)
                {
                    usr = UserPrincipal.FindByIdentity(pc, IdentityType.Guid, userGuid);
                    if (gp.Members.Contains(usr))
                    {
                        gp.Members.Remove(usr);
                        log.DebugFormat("Removed {0} from group {1}", usr.UserPrincipalName, gp.Name);
                    }
                }

                gp.Save();
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error removing {0} from group {1}. Exception: {2}", String.Join(",", userGuids), groupName, ex.ToString());
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
        /// Add a security group to another security group
        /// </summary>
        /// <param name="groupName"></param>
        /// <param name="addingGroupName"></param>
        public void AddGroup(string groupName, string addingGroupName)
        {
            GroupPrincipal gp = null;
            GroupPrincipal gp2 = null;

            try
            {
                if (string.IsNullOrEmpty(groupName))
                    throw new MissingFieldException("Users", "groupName");

                if (string.IsNullOrEmpty(addingGroupName))
                    throw new MissingFieldException("Users", "addingGroupName");

                log.DebugFormat("Attempting to add {0} to group {1}...", addingGroupName, groupName);

                pc = GetPrincipalContext();
                gp = GroupPrincipal.FindByIdentity(pc, IdentityType.Name, groupName);
                if (gp == null)
                    throw new NoMatchingPrincipalException(groupName);

                gp2 = GroupPrincipal.FindByIdentity(pc, IdentityType.Name, addingGroupName);
                if (gp2 == null)
                    throw new NoMatchingPrincipalException(addingGroupName);
                
                if (!gp.Members.Contains(gp2))
                {
                    gp.Members.Add(gp2);
                    gp.Save();

                    log.InfoFormat("Successfully added {0} to group {1}.", addingGroupName, groupName);
                }
                else
                    log.DebugFormat("Did not add {0} to group {1} because the group was already a member.", addingGroupName, groupName);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error adding {0} to group {1}. Exception: {2}", addingGroupName, groupName, ex.ToString());
                throw;
            }
            finally
            {
                if (gp2 != null)
                    gp2.Dispose();

                if (gp != null)
                    gp.Dispose();
            }
        }

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

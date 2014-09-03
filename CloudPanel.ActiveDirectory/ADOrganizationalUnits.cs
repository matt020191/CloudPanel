using CloudPanel.Base.AD;
using CloudPanel.Base.Database.Models;
using log4net;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;

namespace CloudPanel.ActiveDirectory
{
    public class ADOrganizationalUnits : IDisposable
    {
        private readonly ILog log = LogManager.GetLogger(typeof(ADOrganizationalUnits));

        private string _username;
        private string _password;
        private string _domainController;
        private string _ldapStr;
        private bool _disposed;

        internal DirectoryEntry de = null;
        internal PrincipalContext pc = null;

        public ADOrganizationalUnits(string username, string decryptedPassword, string domainController)
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
                de = new DirectoryEntry(this._ldapStr, this._username, this._password);

            if (!string.IsNullOrEmpty(dnPath))
                de.Path = string.Format("{0}{1}", this._ldapStr, dnPath);
            else
                de.Path = this._ldapStr;

            log.DebugFormat("Directory entry retrieved path is {0}", de.Path);
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

        #region Actions

        public OrganizationalUnit Create(string parent, OrganizationalUnit newOU)
        {
            DirectoryEntry child = null;

            try
            {
                if (string.IsNullOrEmpty(newOU.Name))
                    throw new MissingFieldException("OrganizationalUnit", "Name");

                log.DebugFormat("Creating new organizational unit {0} in {1}", newOU.Name, parent);

                de = GetDirectoryEntry(parent);
                child = de.Children.Add("OU=" + newOU.Name, "OrganizationalUnit");

                // Add available properties
                log.Debug("Iterating through all the properties to add to the new organizational unit.");
                if (!string.IsNullOrEmpty(newOU.Description))
                    child.Properties["Description"].Value = newOU.Description;

                if (!string.IsNullOrEmpty(newOU.DisplayName))
                    child.Properties["displayName"].Value = newOU.DisplayName;

                if (!string.IsNullOrEmpty(newOU.Street))
                    child.Properties["street"].Value = newOU.Street;

                if (!string.IsNullOrEmpty(newOU.City))
                    child.Properties["l"].Value = newOU.City;

                if (!string.IsNullOrEmpty(newOU.State))
                    child.Properties["st"].Value = newOU.State;

                if (!string.IsNullOrEmpty(newOU.Country))
                    child.Properties["co"].Value = newOU.Country;

                if (!string.IsNullOrEmpty(newOU.CountryCode))
                    child.Properties["c"].Value = newOU.CountryCode;

                if (newOU.UPNSuffixes != null && newOU.UPNSuffixes.Length > 0)
                    foreach (var u in newOU.UPNSuffixes)
                        child.Properties["uPNSuffixes"].Add(u);

                log.Debug("Done going through all the properties. Now saving the organizational unit...");
                child.CommitChanges();
                log.DebugFormat("Successfully saved new organizational unit {0} in {1}... Retrieving new values...", newOU.Name, parent);

                // Set the values to send back
                newOU.DistinguishedName = child.Properties["distinguishedName"].Value.ToString();

                return newOU;
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error creating new organizational unit {0} in {1}. Exception: {2}", newOU.Name, parent, ex.ToString());
                throw;
            }
            finally
            {
                if (child != null)
                    child.Dispose();
            }
        }

        public void Update(OrganizationalUnit org)
        {
            try
            {
                if (string.IsNullOrEmpty(org.DistinguishedName))
                    throw new MissingFieldException("OrganizationalUnit", "DistinguishedName");

                if (string.IsNullOrEmpty(org.DisplayName))
                    throw new MissingFieldException("OrganizationalUnit", "DisplayName");

                log.DebugFormat("Updating organizational unit {0}", org.DistinguishedName);

                de = GetDirectoryEntry(org.DistinguishedName);

                // Update available properties
                if (!string.IsNullOrEmpty(org.DisplayName))
                    de.Properties["displayName"].Value = org.DisplayName;

                if (!string.IsNullOrEmpty(org.Description))
                    de.Properties["Description"].Value = org.Description;
                else
                    de.Properties["Description"].Clear();

                if (!string.IsNullOrEmpty(org.Street))
                    de.Properties["street"].Value = org.Street;
                else
                    de.Properties["street"].Clear();

                if (!string.IsNullOrEmpty(org.City))
                    de.Properties["l"].Value = org.City;
                else
                    de.Properties["l"].Clear();

                if (!string.IsNullOrEmpty(org.State))
                    de.Properties["st"].Value = org.State;
                else
                    de.Properties["st"].Clear();

                if (!string.IsNullOrEmpty(org.Country))
                    de.Properties["co"].Value = org.Country;
                else
                    de.Properties["co"].Clear();

                if (!string.IsNullOrEmpty(org.CountryCode))
                    de.Properties["c"].Value = org.CountryCode;
                else
                    de.Properties["c"].Clear();

                de.CommitChanges();
                log.DebugFormat("Successfully updated organizational unit {0}.", org.DistinguishedName);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error updating organizational unit {0}. Exception: {2}", org.DistinguishedName, ex.ToString());
                throw;
            }
        }

        public void Delete(string distinguishedName, bool safeDelete)
        {
            DirectorySearcher ds = null;

            try
            {
                if (string.IsNullOrEmpty(distinguishedName))
                    throw new MissingFieldException("OrganizationalUnits", "DistinguishedName");

                log.DebugFormat("Preparing to delete {0}.", distinguishedName);

                de = GetDirectoryEntry(distinguishedName);
                if (safeDelete)
                {
                    log.Debug("Safe delete was selected... checking for users...");

                    // Check for users and OU's
                    ds = new DirectorySearcher(de, "(objectClass=user)");
                    ds.SearchScope = SearchScope.Subtree;

                    SearchResultCollection sr = ds.FindAll();
                    if (sr.Count > 1)
                        throw new MultipleMatchesException(sr.Count.ToString()); // Do not delete if we found users in the OU
                    else
                    {
                        log.DebugFormat("No users were found in {0}. It is now safe to delete", distinguishedName);
                        de.DeleteTree();
                    }
                }
                else
                {
                    // Delete regardless if it contains users
                    de.DeleteTree();
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error deleting organizational unit {0}. Exception: {1}", distinguishedName, ex.ToString());
                throw;
            }
            finally
            {
                if (ds != null)
                    ds.Dispose();
            }
        }

        public void AddDomains(string distinguishedName, string[] domains)
        {
            DirectorySearcher dr = null;

            try
            {
                if (string.IsNullOrEmpty(distinguishedName))
                    throw new MissingFieldException("OrganizationalUnits", "DistinguishedName");

                if (domains == null || domains.Length < 1)
                    throw new MissingFieldException("OrganizationalUnits", "Domains");

                log.DebugFormat("Adding the following domains {0} to all organizational units under {1}", string.Join(", ", domains), distinguishedName);

                de = GetDirectoryEntry(distinguishedName);
                dr = new DirectorySearcher(de);
                dr.Filter = "(objectClass=OrganizationalUnit)";
                dr.SearchScope = SearchScope.Subtree;

                foreach (SearchResult sr in dr.FindAll())
                {
                    // Get our organizational unit
                    DirectoryEntry tmp = sr.GetDirectoryEntry();

                    // Get a list of current domains
                    PropertyValueCollection uPNSuffixes = tmp.Properties["uPNSuffixes"];

                    foreach (var d in domains)
                    {
                        if (!tmp.Properties["uPNSuffixes"].Contains(d))
                            tmp.Properties["uPNSuffixes"].Add(d);
                    }

                    // Save changes
                    tmp.CommitChanges();

                    log.DebugFormat("Successfully added domains {0} to {1}", string.Join(", ", domains), tmp.Properties["distinguishedName"].Value.ToString());
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error adding domains {0} to {1}. Exception: {2}", string.Join(", ", domains), distinguishedName, ex.ToString());
                throw;
            }
            finally
            {
                if (dr != null)
                    dr.Dispose();
            }
        }

        public void RemoveDomains(string distinguishedName, string[] domains)
        {
            DirectorySearcher dr = null;

            try
            {
                if (string.IsNullOrEmpty(distinguishedName))
                    throw new MissingFieldException("OrganizationalUnits", "DistinguishedName");

                if (domains == null || domains.Length < 1)
                    throw new MissingFieldException("OrganizationalUnits", "Domains");

                log.DebugFormat("Removing the following domains {0} to all organizational units under {1}", string.Join(", ", domains), distinguishedName);

                de = GetDirectoryEntry(distinguishedName);
                dr = new DirectorySearcher(de);
                dr.Filter = "(objectClass=OrganizationalUnit)";
                dr.SearchScope = SearchScope.Subtree;

                foreach (SearchResult sr in dr.FindAll())
                {
                    // Get our organizational unit
                    DirectoryEntry tmp = sr.GetDirectoryEntry();

                    // Get a list of current domains
                    PropertyValueCollection uPNSuffixes = tmp.Properties["uPNSuffixes"];

                    foreach (var d in domains)
                    {
                        if (tmp.Properties["uPNSuffixes"].Contains(d))
                            tmp.Properties["uPNSuffixes"].Remove(d);
                    }

                    // Save changes
                    tmp.CommitChanges();

                    log.InfoFormat("Successfully removed domains {0} from {1}", string.Join(", ", domains), tmp.Properties["distinguishedName"].Value.ToString());
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error removing domains {0} to {1}. Exception: {2}", string.Join(", ", domains), distinguishedName, ex.ToString());
                throw;
            }
            finally
            {
                if (dr != null)
                    dr.Dispose();
            }
        }

        public void AddRights(string distinguishedName, string groupName)
        {
            GroupPrincipal gp = null;
            try
            {
                if (string.IsNullOrEmpty(distinguishedName))
                    throw new MissingFieldException("OrganizationalUnit", "DistinguishedName");

                if (string.IsNullOrEmpty(groupName))
                    throw new MissingFieldException("OrganizationalUnit", "GroupName");

                log.DebugFormat("Adding rights to {0}", distinguishedName);

                de = GetDirectoryEntry(distinguishedName);
                pc = GetPrincipalContext();

                string nospaceGroup = groupName.Replace(" ", string.Empty);
                log.DebugFormat("Attempting to find group {0}", nospaceGroup);

                gp = GroupPrincipal.FindByIdentity(pc, IdentityType.Name, nospaceGroup);
                de.ObjectSecurity.AddAccessRule(
                    new ActiveDirectoryAccessRule(gp.Sid, ActiveDirectoryRights.ReadProperty, AccessControlType.Allow, ActiveDirectorySecurityInheritance.All));

                de.ObjectSecurity.AddAccessRule(
                    new ActiveDirectoryAccessRule(gp.Sid, ActiveDirectoryRights.ListObject, AccessControlType.Allow, ActiveDirectorySecurityInheritance.All));

                de.ObjectSecurity.AddAccessRule(
                    new ActiveDirectoryAccessRule(gp.Sid, ActiveDirectoryRights.ListChildren, AccessControlType.Deny, ActiveDirectorySecurityInheritance.All));

                de.CommitChanges();
                log.InfoFormat("Successfully adding access rights to {0} for group {1}", distinguishedName, nospaceGroup);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error adding rights for {0} to {1}. Exception: {2}", groupName, distinguishedName, ex.ToString());
                throw;
            }
            finally
            {
                if (gp != null)
                    gp.Dispose();
            }
        }

        public void RemoveRights(string distinguishedName, string identity)
        {
            try
            {
                if (string.IsNullOrEmpty(distinguishedName))
                    throw new MissingFieldException("OrganizationalUnit", "DistinguishedName");

                if (string.IsNullOrEmpty(identity))
                    throw new MissingFieldException("OrganizationalUnit", "GroupName");

                log.DebugFormat("Removing rights from {0} for {1}", distinguishedName, identity);

                bool modified = false;
                de = GetDirectoryEntry(distinguishedName);

                AuthorizationRuleCollection collection = de.ObjectSecurity.GetAccessRules(true, true, typeof(NTAccount));
                foreach (ActiveDirectoryAccessRule ar in collection)
                {
                    if (ar.IdentityReference.Value.Equals(identity, StringComparison.CurrentCultureIgnoreCase))
                    {
                        de.ObjectSecurity.ModifyAccessRule(AccessControlModification.RemoveAll, ar, out modified);
                    }
                }

                de.CommitChanges();

                if (modified)
                    log.InfoFormat("Successfully removed {0} from {1}", identity, distinguishedName);
                else
                    log.InfoFormat("Successfully ran but was unable to find matching identities for {0} on {1}", identity, distinguishedName);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error removing rights for {0} on {1}. Exception: {2}", identity, distinguishedName, ex.ToString());
                throw;
            }
        }

        public void SetCompanyRights(string distinguishedName, string securityGroup)
        {
            GroupPrincipal gp = null;
            try
            {
                if (string.IsNullOrEmpty(distinguishedName))
                    throw new MissingFieldException("OrganizationalUnit", "DistinguishedName");

                if (string.IsNullOrEmpty(securityGroup))
                    throw new MissingFieldException("OrganizationalUnit", "SecurityGroup");

                log.DebugFormat("Setting company rights on {0} for {1}", distinguishedName, securityGroup);
                pc = GetPrincipalContext();

                // Find the security group
                log.DebugFormat("Finding security group {0}", securityGroup);
                gp = GroupPrincipal.FindByIdentity(pc, IdentityType.Name, securityGroup);
                if (gp == null)
                    throw new Exception("Security group was not found.");
                else
                {
                    log.DebugFormat("Getting organizational unit {0} and removing inheritance", distinguishedName);
                    de = GetDirectoryEntry(distinguishedName);
                    de.ObjectSecurity.SetAccessRuleProtection(true, true);

                    // Remove Authenticated users
                    log.DebugFormat("Removing authenticated users from {0}", distinguishedName);
                    foreach (ActiveDirectoryAccessRule rule in de.ObjectSecurity.GetAccessRules(true, true, typeof(NTAccount)))
                    {
                        log.DebugFormat("Checking if {0} equals NT AUTHORITY\\Authenticated Users", rule.IdentityReference.Value);
                        if (rule.IdentityReference.Value.Equals(@"NT AUTHORITY\Authenticated Users"))
                        {
                            log.DebugFormat("***** FOUND MATCH {0} *****", rule.IdentityReference.Value);
                            de.ObjectSecurity.RemoveAccessRule(rule);
                        }
                    }

                    // Add security group to have read access
                    log.DebugFormat("Adding security group {0} to {1}", securityGroup, distinguishedName);
                    var newRule = new ActiveDirectoryAccessRule(gp.Sid, ActiveDirectoryRights.GenericRead, AccessControlType.Allow, ActiveDirectorySecurityInheritance.All);
                    de.ObjectSecurity.AddAccessRule(newRule);

                    // Save changes
                    log.DebugFormat("Saving changes..");
                    de.CommitChanges();
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error setting company rights on {0} for {1}", distinguishedName, securityGroup);
                throw;
            }
        }

        public List<Users> GetUsers(string distinguishedName)
        {
            DirectorySearcher dr = null;

            List<Users> foundUsers = new List<Users>();
            try
            {
                if (string.IsNullOrEmpty(distinguishedName))
                    throw new MissingFieldException("OrganizationalUnits", "DistinguishedName");

                log.DebugFormat("Retrieving a list of users from {0}", distinguishedName);

                de = GetDirectoryEntry(distinguishedName);
                dr = new DirectorySearcher(de, "(objectClass=user)", null, SearchScope.Subtree);

                foreach (var user in dr.FindAll())
                {
                    // Get our organizational unit
                    DirectoryEntry tmp = (DirectoryEntry)user;

                    Users foundUser = new Users();
                    foundUser.AccountExpires = GetPropertyValue(ref tmp, "accountExpires", "long");
                    foundUser.BadPasswordTime = GetPropertyValue(ref tmp, "badPasswordTime", "long");
                    foundUser.BadPwdCount = GetPropertyValue(ref tmp, "badPwdCount", "int");
                    foundUser.UserAccountControl = GetPropertyValue(ref tmp, "userAccountControl", "int");
                    foundUser.PwdLastSet = GetPropertyValue(ref tmp, "pwdLastSet", "long");
                    foundUser.SamAccountType = GetPropertyValue(ref tmp, "sAMAccountType", "int");
                    foundUser.UserGuid = GetPropertyValue(ref tmp, "objectGuid");
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

                    foundUsers.Add(foundUser);
                }

                return foundUsers;
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error retrieving users from {0}. Exception: {1}", distinguishedName, ex.ToString());
                throw;
            }
            finally
            {
                if (dr != null)
                    dr.Dispose();
            }
        }

        #endregion

        #region Private Methods

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

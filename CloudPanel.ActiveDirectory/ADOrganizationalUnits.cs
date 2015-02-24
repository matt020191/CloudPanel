using CloudPanel.ActiveDirectory.Extensions;
using CloudPanel.Base.AD;
using CloudPanel.Base.Models.Database;
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
        private readonly ILog logger = LogManager.GetLogger(typeof(ADOrganizationalUnits));

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
            logger.DebugFormat("Retrieving the DirectoryEntry... Path: {0}", dnPath);

            if (de == null)
                de = new DirectoryEntry(this._ldapStr, this._username, this._password);

            if (!string.IsNullOrEmpty(dnPath))
                de.Path = string.Format("{0}{1}", this._ldapStr, dnPath);
            else
                de.Path = this._ldapStr;

            logger.DebugFormat("Directory entry retrieved path is {0}", de.Path);
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

        #region Actions

        public OrganizationalUnit Create(string parent, OrganizationalUnit newOU)
        {
            DirectoryEntry child = null;
            DirectorySearcher ds = null;
            try
            {
                logger.DebugFormat("Creating new organizational unit {0} in {1}", newOU.Name, parent);

                if (string.IsNullOrEmpty(newOU.Name))
                    throw new MissingFieldException("OrganizationalUnit", "Name");

                de = GetDirectoryEntry(parent);

                // Lets see if the OU already exists
                ds = new DirectorySearcher(de);
                ds.Filter = string.Format("(&(objectClass=organizationalUnit)(OU={0}))", newOU.Name);
                ds.SearchScope = SearchScope.OneLevel;

                var searchResult = ds.FindOne();
                if (searchResult != null)
                {
                    logger.DebugFormat("Skipping creating OU {0} because it already exists", newOU.Name);

                    // Set the values to send back
                    newOU.DistinguishedName = searchResult.GetDirectoryEntry().Properties["DistinguishedName"].Value.ToString();
                    return newOU;
                }
                else
                {
                    child = de.Children.Add("OU=" + newOU.Name, "OrganizationalUnit");

                    // Add available properties
                    logger.Debug("Iterating through all the properties to add to the new organizational unit.");
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

                    logger.Debug("Done going through all the properties. Now saving the organizational unit...");
                    child.CommitChanges();
                    logger.DebugFormat("Successfully saved new organizational unit {0} in {1}... Retrieving new values...", newOU.Name, parent);

                    // Set the values to send back
                    newOU.DistinguishedName = child.Properties["distinguishedName"].Value.ToString();

                    return newOU;
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error creating new organizational unit {0} in {1}. Exception: {2}", newOU.Name, parent, ex.ToString());
                throw;
            }
            finally
            {
                if (child != null)
                    child.Dispose();
            }
        }

        public OrganizationalUnit GetOU(string orgUnitDN)
        {
            DirectoryEntry de = null;

            try
            {
                OrganizationalUnit company = new OrganizationalUnit();

                // Connect to AD
                logger.DebugFormat("Connecting to AD and binding to {0}", orgUnitDN);
                de = new DirectoryEntry("LDAP://" + _domainController + "/" + orgUnitDN, _username, _password);

                // Retrieve attributes
                Guid result = new Guid();
                Guid.TryParse(de.Properties["objectGUID"].Value.ToString(), out result);
                logger.DebugFormat("{0}", de.Properties["objectGUID"].Value.ToString());

                company.ObjectGUID = result;
                company.Name = de.Properties["name"].Value.ToString();
                company.DisplayName = de.Properties["name"].Value.ToString();
                company.DistinguishedName = de.Properties["distinguishedName"].Value.ToString();
                company.WhenCreated = de.Properties["whenCreated"].Value.ToString();

                if (de.Properties["uPNSuffixes"] != null && de.Properties["uPNSuffixes"].Count > 0)
                {
                    List<string> domains = new List<string>();

                    for (int i = 0; i < de.Properties["uPNSuffixes"].Count; i++)
                    {
                        if (!string.IsNullOrEmpty(de.Properties["uPNSuffixes"][i].ToString()))
                            domains.Add(de.Properties["uPNSuffixes"][i].ToString());
                    }

                    // Set company domains
                    company.UPNSuffixes = domains.ToArray();
                }

                // If we got here then the OU exist because it didn't throw an error
                return company;
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error getting organizational unit {0}: {1}", orgUnitDN, ex.ToString());
                throw;
            }
            finally
            {
                if (de != null)
                    de.Dispose();
            }
        }

        public List<OrganizationalUnit> GetChildOUs(string orgUnitDN)
        {
            DirectoryEntry de = null;
            DirectorySearcher ds = null;

            try
            {
                List<OrganizationalUnit> foundChildOUs = new List<OrganizationalUnit>();

                // DEBUG //
                logger.Debug("Checking for child OUs in " + orgUnitDN);

                // Connect to AD
                de = new DirectoryEntry("LDAP://" + _domainController + "/" + orgUnitDN, _username, _password);
                ds = new DirectorySearcher(de);
                ds.Filter = "(objectCategory=organizationalUnit)";
                ds.SearchScope = SearchScope.OneLevel;

                foreach (SearchResult sr in ds.FindAll())
                {
                    OrganizationalUnit company = new OrganizationalUnit();

                    // Create our DirectoryEntry object
                    DirectoryEntry found = sr.GetDirectoryEntry();
                    company.Name = found.Properties["name"].Value.ToString();
                    company.DistinguishedName = company.Name;
                    company.DistinguishedName = found.Properties["DistinguishedName"].Value.ToString();

                    if (found.Properties["DisplayName"].Value != null)
                        company.DisplayName = found.Properties["DisplayName"].Value.ToString();
                    else
                        company.DisplayName = company.Name;

                    // Add to our list but not if it is the parent ou
                    if (!company.DistinguishedName.Equals(orgUnitDN, StringComparison.CurrentCultureIgnoreCase))
                        foundChildOUs.Add(company);
                }

                // Return list of child OU's
                return foundChildOUs;
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error getting company child OU's in {0}: {1}", orgUnitDN, ex.ToString());
                throw;
            }
            finally
            {
                if (ds != null)
                    ds.Dispose();

                if (de != null)
                    de.Dispose();
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

                logger.DebugFormat("Updating organizational unit {0}", org.DistinguishedName);

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
                logger.DebugFormat("Successfully updated organizational unit {0}.", org.DistinguishedName);
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error updating organizational unit {0}. Exception: {2}", org.DistinguishedName, ex.ToString());
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

                logger.DebugFormat("Preparing to delete {0}.", distinguishedName);

                de = GetDirectoryEntry(distinguishedName);
                if (safeDelete)
                {
                    logger.Debug("Safe delete was selected... checking for users...");

                    // Check for users and OU's
                    ds = new DirectorySearcher(de, "(objectClass=user)");
                    ds.SearchScope = SearchScope.Subtree;

                    SearchResultCollection sr = ds.FindAll();
                    if (sr.Count > 1)
                        throw new MultipleMatchesException(sr.Count.ToString()); // Do not delete if we found users in the OU
                    else
                    {
                        logger.DebugFormat("No users were found in {0}. It is now safe to delete", distinguishedName);
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
                logger.ErrorFormat("Error deleting organizational unit {0}. Exception: {1}", distinguishedName, ex.ToString());
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

                logger.DebugFormat("Adding the following domains {0} to all organizational units under {1}", string.Join(", ", domains), distinguishedName);

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

                    logger.DebugFormat("Successfully added domains {0} to {1}", string.Join(", ", domains), tmp.Properties["distinguishedName"].Value.ToString());
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error adding domains {0} to {1}. Exception: {2}", string.Join(", ", domains), distinguishedName, ex.ToString());
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

                logger.DebugFormat("Removing the following domains {0} to all organizational units under {1}", string.Join(", ", domains), distinguishedName);

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

                    logger.InfoFormat("Successfully removed domains {0} from {1}", string.Join(", ", domains), tmp.Properties["distinguishedName"].Value.ToString());
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error removing domains {0} to {1}. Exception: {2}", string.Join(", ", domains), distinguishedName, ex.ToString());
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

                logger.DebugFormat("Adding rights to {0}", distinguishedName);

                de = GetDirectoryEntry(distinguishedName);
                pc = GetPrincipalContext();

                string nospaceGroup = groupName.Replace(" ", string.Empty);
                logger.DebugFormat("Attempting to find group {0}", nospaceGroup);

                gp = GroupPrincipal.FindByIdentity(pc, IdentityType.Name, nospaceGroup);
                de.ObjectSecurity.AddAccessRule(
                    new ActiveDirectoryAccessRule(gp.Sid, ActiveDirectoryRights.ReadProperty, AccessControlType.Allow, ActiveDirectorySecurityInheritance.All));

                de.ObjectSecurity.AddAccessRule(
                    new ActiveDirectoryAccessRule(gp.Sid, ActiveDirectoryRights.ListObject, AccessControlType.Allow, ActiveDirectorySecurityInheritance.All));

                de.ObjectSecurity.AddAccessRule(
                    new ActiveDirectoryAccessRule(gp.Sid, ActiveDirectoryRights.ListChildren, AccessControlType.Deny, ActiveDirectorySecurityInheritance.All));

                de.CommitChanges();
                logger.InfoFormat("Successfully adding access rights to {0} for group {1}", distinguishedName, nospaceGroup);
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error adding rights for {0} to {1}. Exception: {2}", groupName, distinguishedName, ex.ToString());
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

                logger.DebugFormat("Removing rights from {0} for {1}", distinguishedName, identity);

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
                    logger.InfoFormat("Successfully removed {0} from {1}", identity, distinguishedName);
                else
                    logger.InfoFormat("Successfully ran but was unable to find matching identities for {0} on {1}", identity, distinguishedName);
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error removing rights for {0} on {1}. Exception: {2}", identity, distinguishedName, ex.ToString());
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

                logger.DebugFormat("Setting company rights on {0} for {1}", distinguishedName, securityGroup);
                pc = GetPrincipalContext();

                // Find the security group
                logger.DebugFormat("Finding security group {0}", securityGroup);
                gp = GroupPrincipal.FindByIdentity(pc, IdentityType.Name, securityGroup);
                if (gp == null)
                    throw new Exception("Security group was not found.");
                else
                {
                    logger.DebugFormat("Getting organizational unit {0} and removing inheritance", distinguishedName);
                    de = GetDirectoryEntry(distinguishedName);
                    de.ObjectSecurity.SetAccessRuleProtection(true, true);

                    // Remove Authenticated users
                    logger.DebugFormat("Removing authenticated users from {0}", distinguishedName);
                    foreach (ActiveDirectoryAccessRule rule in de.ObjectSecurity.GetAccessRules(true, true, typeof(NTAccount)))
                    {
                        logger.DebugFormat("Checking if {0} equals NT AUTHORITY\\Authenticated Users", rule.IdentityReference.Value);
                        if (rule.IdentityReference.Value.Equals(@"NT AUTHORITY\Authenticated Users"))
                        {
                            logger.DebugFormat("***** FOUND MATCH {0} *****", rule.IdentityReference.Value);
                            de.ObjectSecurity.RemoveAccessRule(rule);
                        }
                    }

                    // Add security group to have read access
                    logger.DebugFormat("Adding security group {0} to {1}", securityGroup, distinguishedName);
                    var newRule = new ActiveDirectoryAccessRule(gp.Sid, ActiveDirectoryRights.GenericRead, AccessControlType.Allow, ActiveDirectorySecurityInheritance.All);
                    de.ObjectSecurity.AddAccessRule(newRule);

                    // Save changes
                    logger.DebugFormat("Saving changes..");
                    de.CommitChanges();
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error setting company rights on {0} for {1}", distinguishedName, securityGroup);
                throw;
            }
        }

        public List<Users> GetUsers(string distinguishedName)
        {
            UserPrincipalExt up = null;
            PrincipalSearcher sr = null;
            List<Users> foundUsers = new List<Users>();
            try
            {
                if (string.IsNullOrEmpty(distinguishedName))
                    throw new MissingFieldException("OrganizationalUnits", "DistinguishedName");

                logger.DebugFormat("Retrieving a list of users from {0}", distinguishedName);
                pc = new PrincipalContext(ContextType.Domain, _domainController, distinguishedName, _username, _password);
                up = new UserPrincipalExt(pc);
                sr = new PrincipalSearcher(up);

                var users = sr.FindAll();

                logger.DebugFormat("Found a total of {0} users", users.Count());
                foreach (var user in users)
                {
                    // Get our user
                    UserPrincipalExt tmp = user as UserPrincipalExt;
                    if (tmp != null)
                    {
                        Users foundUser = new Users();
                        foundUser.UserGuid = (Guid)tmp.Guid;
                        foundUser.Street = tmp.StreetAddress;
                        foundUser.City = tmp.City;
                        foundUser.State = tmp.State;
                        foundUser.PostalCode = tmp.ZipCode;
                        foundUser.Country = tmp.Country;
                        foundUser.Company = tmp.Company;
                        foundUser.Department = tmp.Department;
                        foundUser.Description = tmp.Description;
                        foundUser.Firstname = tmp.GivenName;
                        foundUser.Lastname = tmp.LastName;
                        foundUser.DisplayName = tmp.DisplayName;
                        foundUser.Name = tmp.Name;
                        foundUser.UserPrincipalName = tmp.UserPrincipalName;
                        foundUser.Fax = tmp.Fax;
                        foundUser.TelephoneNumber = tmp.TelephoneNumber;
                        foundUser.HomePhone = tmp.HomePhone;
                        foundUser.IPPhone = tmp.IPPhone;
                        foundUser.JobTitle = tmp.JobTitle;
                        foundUser.MobilePhone = tmp.MobilePhone;
                        foundUser.Office = tmp.Office;
                        foundUser.Pager = tmp.Pager;
                        foundUser.ScriptPath = tmp.ScriptPath;
                        foundUser.Webpage = tmp.Website;
                        foundUser.Email = tmp.EmailAddress;

                        foundUsers.Add(foundUser);
                    }
                }

                return foundUsers;
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error retrieving users from {0}. Exception: {1}", distinguishedName, ex.ToString());
                throw;
            }
            finally
            {
                if (up != null)
                    up.Dispose();

                if (sr != null)
                    sr.Dispose();
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

using CloudPanel.ActiveDirectory;
using CloudPanel.Base.AD;
using CloudPanel.Base.Config;
using CloudPanel.Base.Database.Models;
using CloudPanel.Code;
using CloudPanel.Database.EntityFramework;
using CloudPanel.Rollback;
using log4net;
using Nancy;
using Nancy.ModelBinding;
using Nancy.Security;
using System;
using System.Linq;
using System.Reflection;

namespace CloudPanel.Modules
{
    public class CompaniesModule : NancyModule
    {
        private static readonly ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public CompaniesModule() : base("/companies/{ResellerCode?}")
        {
            this.RequiresAuthentication();

            Get["/", c => c.Request.Accept("text/html")] = _ =>
                {
                    this.RequiresValidatedClaims(x => ValidateClaims.AllowSuperOrReseller(Context.CurrentUser, _.ResellerCode));

                    if (Settings.ResellersEnabled)
                    {
                        string resellerCode = _.ResellerCode;
                        this.Context.SetResellerCode(resellerCode);
                    }

                    return View["companies.cshtml"];
                };

            Get["/", c => !c.Request.Accept("text/html")] = _ =>
            {
                this.RequiresValidatedClaims(x => ValidateClaims.AllowSuperOrReseller(Context.CurrentUser, _.ResellerCode));

                #region Returns the companies json data based on the request
                CloudPanelContext db = null;
                try
                {
                    db = new CloudPanelContext(Settings.ConnectionString);
                    db.Database.Connection.Open();

                    // Retrieve all resellers from the database
                    var companies = (from d in db.Companies
                                     where !d.IsReseller
                                     orderby d.CompanyName
                                     select d).ToList();

                    if (Settings.ResellersEnabled) // If resellers are enabled then trim the results
                        companies = companies.Where(x => x.ResellerCode == _.ResellerCode).ToList();

                    int draw = 0, start = 0, length = 0, recordsTotal = companies.Count, recordsFiltered = companies.Count, orderColumn = 0;
                    string searchValue = "", orderColumnName = "";
                    bool isAscendingOrder = true;

                    if (Request.Query.draw.HasValue)
                    {
                        draw = Request.Query.draw;
                        start = Request.Query.start;
                        length = Request.Query.length;
                        orderColumn = Request.Query["order[0][column]"];
                        searchValue = Request.Query["search[value]"].HasValue ? Request.Query["search[value]"] : string.Empty;
                        isAscendingOrder = Request.Query["order[0][dir]"] == "asc" ? true : false;
                        orderColumnName = Request.Query["columns[" + orderColumn + "][data]"];

                        // See if we are using dataTables to search
                        if (!string.IsNullOrEmpty(searchValue))
                        {
                            companies = (from d in companies
                                         where d.CompanyCode.IndexOf(searchValue, StringComparison.InvariantCultureIgnoreCase) != -1 ||
                                               d.CompanyName.IndexOf(searchValue, StringComparison.InvariantCultureIgnoreCase) != -1 ||
                                               d.City.IndexOf(searchValue, StringComparison.InvariantCultureIgnoreCase) != -1 ||
                                               d.State.IndexOf(searchValue, StringComparison.InvariantCultureIgnoreCase) != -1 ||
                                               d.ZipCode.IndexOf(searchValue, StringComparison.InvariantCultureIgnoreCase) != -1 ||
                                               d.Country.IndexOf(searchValue, StringComparison.InvariantCultureIgnoreCase) != -1
                                         select d).ToList();
                            recordsFiltered = companies.Count;
                        }

                        if (isAscendingOrder)
                            companies = companies.OrderBy(x => x.GetType()
                                                    .GetProperty(orderColumnName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance).GetValue(x, null))
                                                    .Skip(start)
                                                    .Take(length)
                                                    .ToList();
                        else
                            companies = companies.OrderByDescending(x => x.GetType()
                                                    .GetProperty(orderColumnName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance).GetValue(x, null))
                                                    .Skip(start)
                                                    .Take(length)
                                                    .ToList();
                    }

                    return Negotiate.WithModel(new
                                    {
                                        draw = draw,
                                        recordsTotal = recordsTotal,
                                        recordsFiltered = recordsFiltered,
                                        data = companies
                                    });
                }
                catch (Exception ex)
                {
                    return Negotiate.WithModel(new { error = ex.Message })
                                    .WithStatusCode(HttpStatusCode.InternalServerError);
                }
                finally
                {
                    if (db != null)
                        db.Dispose();
                }
                #endregion
            };

            Post["/"] = _ =>
            {
                this.RequiresValidatedClaims(x => ValidateClaims.AllowSuperOrReseller(Context.CurrentUser, _.ResellerCode));

                #region Creates a new company

                CloudPanelContext db = null;
                ADOrganizationalUnits org = null;
                ADGroups groups = null;
                ReverseActions reverse = null;
                try
                {
                    logger.DebugFormat("Request to create company started...");
                    db = new CloudPanelContext(Settings.ConnectionString);

                    logger.DebugFormat("Binding form to company object");
                    var newCompany = this.Bind<Companies>();
                    newCompany.IsReseller = false;
                    newCompany.ExchEnabled = false;
                    newCompany.CitrixEnabled = false;
                    newCompany.LyncEnabled = false;

                    // Validate the required fields
                    if (string.IsNullOrEmpty(newCompany.CompanyName))
                        throw new MissingFieldException("Company name is required.");

                    if (string.IsNullOrEmpty(newCompany.AdminName))
                        throw new MissingFieldException("Contact name is required.");

                    if (string.IsNullOrEmpty(newCompany.AdminEmail))
                        throw new MissingFieldException("Contact email is required.");

                    if (newCompany.OrgPlanID == null || newCompany.OrgPlanID < 1)
                        throw new MissingFieldException("Company plan is required.");

                    if (!Request.Form.DomainName.HasValue)
                        throw new MissingFieldException("Domain name is required.");

                    // Lets get the company code
                    logger.DebugFormat("Generating new company code for {0}", newCompany.CompanyName);
                    string companyCode = CPStaticHelpers.GenerateCompanyCode(newCompany.CompanyName);

                    // Initialize classes needed
                    logger.DebugFormat("New company code for {0} is {1}", newCompany.CompanyName, companyCode);
                    org = new ADOrganizationalUnits(Settings.Username, Settings.DecryptedPassword, Settings.PrimaryDC);
                    reverse = new ReverseActions();

                    //
                    // Create the organizational units
                    //
                    Companies reseller = null;
                    if (Settings.ResellersEnabled)
                    {
                        logger.DebugFormat("Retrieving reseller information for {0}", _.ResellerCode);

                        string resellerCode = _.ResellerCode;
                        reseller = (from d in db.Companies
                                    where d.IsReseller
                                    where d.CompanyCode == resellerCode
                                    select d).FirstOrDefault();

                        if (reseller == null)
                            throw new Exception("Unable to find reseller.");
                    }

                    logger.DebugFormat("Populating OrganizationalUnit object");
                    var newCompanyOrg = new OrganizationalUnit()
                    {
                        Name = Settings.UseNameInsteadOfCompanyCode ? newCompany.CompanyName : companyCode,
                        Street = newCompany.Street,
                        City = newCompany.City,
                        State = newCompany.State,
                        PostalCode = newCompany.ZipCode,
                        Country = newCompany.Country,
                        DisplayName = newCompany.CompanyName,
                        AdminDisplayName = newCompany.AdminName,
                        AdminDescription = newCompany.AdminEmail,
                        UPNSuffixes = new string[] { Request.Form.DomainName }
                    };

                    logger.DebugFormat("Creating new organizational unit in Active Directory");
                    var createdOrg = org.Create((Settings.ResellersEnabled ? reseller.DistinguishedName : Settings.HostingOU), newCompanyOrg); // Create the new OU and get the extra information back
                    reverse.AddAction(Actions.CreateOrganizationalUnit, createdOrg.DistinguishedName); // Add to our rollback actions in case we need to remove

                    logger.DebugFormat("Creating Applications organizational unit in Active Directory");
                    var appOrg = org.Create(createdOrg.DistinguishedName, new OrganizationalUnit() { Name = Settings.ApplicationsOUName, UPNSuffixes = createdOrg.UPNSuffixes });
                    org.RemoveRights(appOrg.DistinguishedName, @"NT AUTHORITY\Authenticated Users");

                    logger.DebugFormat("Creating groups organizational unit in Active Directory");
                    var grpOrg = org.Create(createdOrg.DistinguishedName, new OrganizationalUnit() { Name = Settings.ExchangeGroupsOU, UPNSuffixes = createdOrg.UPNSuffixes });
                    org.RemoveRights(grpOrg.DistinguishedName, @"NT AUTHORITY\Authenticated Users");

                    logger.DebugFormat("Creating contacts organizational unit in Active Directory");
                    var contactOrg = org.Create(createdOrg.DistinguishedName, new OrganizationalUnit() { Name = Settings.ExchangeContactsOU, UPNSuffixes = createdOrg.UPNSuffixes });
                    org.RemoveRights(contactOrg.DistinguishedName, @"NT AUTHORITY\Authenticated Users");

                    logger.DebugFormat("Creating rooms organizational unit in Active Directory");
                    var roomOrg = org.Create(createdOrg.DistinguishedName, new OrganizationalUnit() { Name = Settings.ExchangeRoomsOU, UPNSuffixes = createdOrg.UPNSuffixes });
                    org.RemoveRights(roomOrg.DistinguishedName, @"NT AUTHORITY\Authenticated Users");

                    logger.DebugFormat("Creating resource organizational unit in Active Directory");
                    var resourceOrg = org.Create(createdOrg.DistinguishedName, new OrganizationalUnit() { Name = Settings.ExchangeResourceOU, UPNSuffixes = createdOrg.UPNSuffixes });
                    org.RemoveRights(resourceOrg.DistinguishedName, @"NT AUTHORITY\Authenticated Users");

                    if (!string.IsNullOrEmpty(Settings.UsersOU))
                    {
                        // May have chosen to put users in a different organizational unit
                        logger.DebugFormat("Creating Users organizational unit called {0}", Settings.UsersOU);
                        var userOrg = org.Create(createdOrg.DistinguishedName, new OrganizationalUnit() { Name = Settings.UsersOU, UPNSuffixes = createdOrg.UPNSuffixes });
                        org.RemoveRights(userOrg.DistinguishedName, @"NT AUTHORITY\Authenticated Users");
                    }

                    //
                    // Create Security Groups
                    //
                    logger.DebugFormat("Creating Admins security group");
                    groups = new ADGroups(Settings.Username, Settings.DecryptedPassword, Settings.PrimaryDC);
                    groups.Create(createdOrg.DistinguishedName, new SecurityGroup() { Name = "Admins@" + companyCode, SamAccountName = "Admins@" + companyCode });

                    logger.DebugFormat("Creating AllTSUsers security group");
                    groups.Create(createdOrg.DistinguishedName, new SecurityGroup() { Name = "AllTSUsers@" + companyCode, SamAccountName = "AllTSUsers@" + companyCode });

                    logger.DebugFormat("Creating AllUsers security group");
                    groups.Create(createdOrg.DistinguishedName, new SecurityGroup() { Name = "AllUsers@" + companyCode, SamAccountName = "AllUsers@" + companyCode });

                    // Add AllTSUsers@ company group to the hosting group and GPOAccess group
                    logger.DebugFormat("Adding terminal server group to hosting group");
                    groups.AddGroup("AllTSUsers@Hosting", "AllTSUsers@" + companyCode);

                    logger.DebugFormat("Adding terminal server group to gpo access group");
                    groups.AddGroup("GPOAccess@" + (Settings.ResellersEnabled ? reseller.CompanyCode : "Hosting"), "AllTSUsers@" + companyCode);


                    // Remove authenticated users rights and add AllUsers read rights
                    logger.DebugFormat("Setting company rights to the organizational unit");
                    org.SetCompanyRights(createdOrg.DistinguishedName, "AllUsers@" + companyCode);

                    //
                    // Add domain to database
                    //
                    logger.DebugFormat("Adding domain {0} to the database", Request.Form.DomainName);
                    db.Domains.Add(new Domains()
                    {
                        CompanyCode = companyCode,
                        Domain = Request.Form.DomainName,
                        IsAcceptedDomain = false,
                        IsLyncDomain = false,
                        IsDefault = true,
                        IsSubDomain = false
                    });

                    //
                    // Add company to database
                    //
                    logger.DebugFormat("Adding company {0} to the database", newCompany.CompanyName);
                    newCompany.CompanyCode = companyCode;
                    newCompany.Created = DateTime.Now;
                    newCompany.DistinguishedName = createdOrg.DistinguishedName;
                    db.Companies.Add(newCompany);

                    // Save
                    logger.DebugFormat("Saving database changes");
                    db.SaveChanges();

                    logger.InfoFormat("Successfully created new company {0}", newCompany.CompanyName);
                    return Negotiate.WithModel(new { success = "Successfully created new company", resellerCode = _.ResellerCode })
                                    .WithView("companies.cshtml")
                                    .WithStatusCode(HttpStatusCode.OK);
                }
                catch (Exception ex)
                {
                    logger.ErrorFormat("Error creating company: {0}", ex.ToString());

                    reverse.RollbackNow();
                    return Negotiate.WithModel(new { error = ex.Message, resellerCode = _.ResellerCode })
                                    .WithView("companies.cshtml")
                                    .WithStatusCode(HttpStatusCode.InternalServerError);
                }
                finally
                {
                    if (db != null)
                        db.Dispose();

                    if (org != null)
                        org.Dispose();

                    if (groups != null)
                        groups.Dispose();
                }

                #endregion
            };

            Put["/"] = _ =>
            {
                this.RequiresValidatedClaims(x => ValidateClaims.AllowSuperOrReseller(Context.CurrentUser, _.ResellerCode));

                #region Updates an existing company

                CloudPanelContext db = null;
                ADOrganizationalUnits org = null;
                try
                {
                    logger.DebugFormat("Request to update company started...");
                    db = new CloudPanelContext(Settings.ConnectionString);

                    logger.DebugFormat("Binding form to company object");
                    var updatedCompany = this.Bind<Companies>();

                    // Validate the required fields
                    if (string.IsNullOrEmpty(updatedCompany.CompanyName))
                        throw new MissingFieldException("Company name is required.");

                    if (string.IsNullOrEmpty(updatedCompany.AdminName))
                        throw new MissingFieldException("Contact name is required.");

                    if (string.IsNullOrEmpty(updatedCompany.AdminEmail))
                        throw new MissingFieldException("Contact email is required.");

                    if (string.IsNullOrEmpty(updatedCompany.CompanyCode))
                        throw new MissingFieldException("You must provide the company code to update");

                    // Initialize classes needed
                    logger.DebugFormat("Initializing org object");
                    org = new ADOrganizationalUnits(Settings.Username, Settings.DecryptedPassword, Settings.PrimaryDC);

                    // Get existing reseller from database
                    logger.DebugFormat("Looking for the existing company {0} in the database", updatedCompany.CompanyCode);
                    var existingCompany = (from d in db.Companies
                                            where !d.IsReseller
                                            where d.CompanyCode == updatedCompany.CompanyCode
                                            select d).FirstOrDefault();
                    if (existingCompany == null)
                        throw new Exception("Company " + updatedCompany.CompanyCode + " was not found.");

                    logger.DebugFormat("Creating new organizational unit object to update active directory on {0}", existingCompany.DistinguishedName);
                    OrganizationalUnit updatedOrg = new OrganizationalUnit();
                    updatedOrg.DistinguishedName = existingCompany.DistinguishedName;
                    updatedOrg.DisplayName = updatedCompany.CompanyName;
                    updatedOrg.Street = updatedCompany.Street;
                    updatedOrg.City = updatedCompany.City;
                    updatedOrg.State = updatedCompany.State;
                    updatedOrg.PostalCode = updatedCompany.ZipCode;
                    updatedOrg.Country = updatedCompany.Country;
                    updatedOrg.AdminDisplayName = updatedCompany.AdminName;
                    updatedOrg.AdminDescription = updatedCompany.AdminEmail;

                    logger.DebugFormat("Updating organizational unit in active directory");
                    org.Update(updatedOrg);

                    logger.DebugFormat("Updated values in database");
                    existingCompany.CompanyName = updatedCompany.CompanyName;
                    existingCompany.Street = updatedCompany.Street;
                    existingCompany.City = updatedCompany.City;
                    existingCompany.State = updatedCompany.State;
                    existingCompany.ZipCode = updatedCompany.ZipCode;
                    existingCompany.Country = updatedCompany.Country;
                    existingCompany.PhoneNumber = updatedCompany.PhoneNumber;
                    existingCompany.AdminName = updatedCompany.AdminName;
                    existingCompany.AdminEmail = updatedCompany.AdminEmail;
                    existingCompany.Website = updatedCompany.Website;

                    logger.DebugFormat("Saving data to the database");
                    db.SaveChanges();

                    logger.InfoFormat("Successfully updated company {0}", updatedCompany.CompanyName);
                    return Negotiate.WithModel(new { success = "Successfully updated existing company", resellerCode = _.ResellerCode })
                                    .WithView("companies.cshtml")
                                    .WithStatusCode(HttpStatusCode.OK);
                }
                catch (Exception ex)
                {
                    logger.ErrorFormat("Error updating company: {0}", ex.ToString());
                    return Negotiate.WithModel(new { error = ex.Message, resellerCode = _.ResellerCode })
                                    .WithView("companies.cshtml")
                                    .WithStatusCode(HttpStatusCode.InternalServerError);
                }
                finally
                {
                    if (db != null)
                        db.Dispose();

                    if (org != null)
                        org.Dispose();
                }

                #endregion
            };

            Delete["/"] = _ =>
            {
                this.RequiresValidatedClaims(x => ValidateClaims.AllowSuperOrReseller(Context.CurrentUser, _.ResellerCode));

                #region Deletes a company from the database and Active Directory
                CloudPanelContext db = null;
                ADOrganizationalUnits org = null;
                try
                {
                    string companyCode = Request.Form.CompanyCode;

                    logger.DebugFormat("Preparing to delete company {0}", Request.Form.CompanyCode);
                    db = new CloudPanelContext(Settings.ConnectionString);
                    db.Database.Connection.Open();

                    var company = (from d in db.Companies
                                   where !d.IsReseller
                                   where d.CompanyCode == companyCode
                                   select d).FirstOrDefault();
                    if (company == null)
                        throw new Exception("Unable to find company in database.");
                    else
                    {
                        if (company.ExchEnabled)
                            throw new Exception("It appears this company is enabled for Exchange. Before deleting please disable Exchange for this company.");

                        logger.DebugFormat("Initializing Active Directory class...");
                        org = new ADOrganizationalUnits(Settings.Username, Settings.DecryptedPassword, Settings.PrimaryDC);

                        logger.InfoFormat("Deleting company from Active Directory. Path {0}", company.DistinguishedName);
                        org.Delete(company.DistinguishedName, false);

                        logger.DebugFormat("Clearing Exchange contacts for {0}", company.CompanyCode);
                        var contacts = from d in db.Contacts where d.CompanyCode == companyCode select d;
                        if (contacts != null)
                        {
                            logger.InfoFormat("Found a total of {0} contacts in the database. Removing: {1}", contacts.Count(), String.Join(", ", contacts.Select(x => x.DistinguishedName)));
                            db.Contacts.RemoveRange(contacts);
                        }

                        logger.DebugFormat("Clearing Exchange distribution groups for {0}", company.CompanyCode);
                        var groups = from d in db.DistributionGroups where d.CompanyCode == companyCode select d;
                        if (groups != null)
                        {
                            logger.InfoFormat("Found a total of {0} groups in the database. Removing: {1}", contacts.Count(), String.Join(", ", groups.Select(x => x.DistinguishedName)));
                            db.DistributionGroups.RemoveRange(groups);
                        }

                        logger.DebugFormat("Clearing Domains for {0}", company.CompanyCode);
                        var domains = from d in db.Domains where d.CompanyCode == companyCode select d;
                        if (domains != null)
                        {
                            logger.InfoFormat("Found a total of {0} domains in the database. Removing: {1}", domains.Count(), String.Join(", ", domains.Select(x => x.Domain)));
                            db.Domains.RemoveRange(domains);
                        }

                        logger.DebugFormat("Clearing resource mailboxes for {0}", company.CompanyCode);
                        var resourceMailboxes = from d in db.ResourceMailboxes where d.CompanyCode == companyCode select d;
                        if (resourceMailboxes != null)
                        {
                            logger.InfoFormat("Found a total of {0} resource mailboxes in the database. Removing: {1}", resourceMailboxes.Count(), String.Join(", ", resourceMailboxes.Select(x => x.UserPrincipalName)));
                            db.ResourceMailboxes.RemoveRange(resourceMailboxes);
                        }

                        logger.DebugFormat("Clearing custom prices for {0}", company.CompanyCode);
                        var customPrices = from d in db.PriceOverride where d.CompanyCode == companyCode select d;
                        if (customPrices != null)
                        {
                            logger.InfoFormat("Found a total of {0} custom prices in the database.", customPrices.Count());
                            db.PriceOverride.RemoveRange(customPrices);
                        }

                        logger.DebugFormat("Clearing queues for {0}", company.CompanyCode);
                        var queues = from d in db.SvcQueue where d.CompanyCode == companyCode select d;
                        if (queues != null)
                        {
                            logger.InfoFormat("Found a total of {0} queues in the database.", queues.Count());
                            db.SvcQueue.RemoveRange(queues);
                        }

                        logger.DebugFormat("Getting list of users for {0}", company.CompanyCode);
                        var users = from d in db.Users where d.CompanyCode == companyCode select d;
                        if (users != null)
                        {
                            logger.InfoFormat("Found a total of {0} users in the database.", users.Count());

                            logger.DebugFormat("Removing user assignments to Citrix for {0}", company.CompanyCode);
                            var foundPlans = from d in db.UserPlansCitrix where users.Select(x => x.ID).Contains(d.UserID) select d;
                            if (foundPlans != null)
                                db.UserPlansCitrix.RemoveRange(foundPlans);
                        }

                        logger.DebugFormat("Removing company {0} from database", company.CompanyCode);
                        db.Companies.Remove(company);

                        logger.DebugFormat("Saving changes to database...");
                        db.SaveChanges();

                        // Check to see if the company that was deleted is what they have selected
                        // So we can remove it from being selected. This will hide the left menu bar if it was
                        var user = this.Context.CurrentUser as AuthenticatedUser;
                        if (user.SelectedCompanyCode.Equals(companyCode))
                        {
                            user.SelectedCompanyCode = string.Empty;
                            user.SelectedCompanyName = string.Empty;
                        }

                        return Negotiate.WithModel(new { success = "Company was deleted successfully" })
                                        .WithView("companies.cshtml")
                                        .WithStatusCode(HttpStatusCode.OK);
                    }
                }
                catch (Exception ex)
                {
                    logger.ErrorFormat("Error deleting company: {0}", ex.ToString());
                    return Negotiate.WithModel(new { error = ex.Message })
                                    .WithView("companies.cshtml")
                                    .WithStatusCode(HttpStatusCode.InternalServerError);
                }
                finally
                {
                    if (db != null)
                        db.Dispose();
                }
                #endregion
            };

            Get["/{CompanyCode}"] = _ =>
            {
                this.RequiresValidatedClaims(x => ValidateClaims.AllowSuperOrReseller(Context.CurrentUser, _.ResellerCode));

                #region Gets a specific company
                CloudPanelContext db = null;
                try
                {
                    db = new CloudPanelContext(Settings.ConnectionString);

                    // Make sure there are no companies assigned to this reseller
                    string companyCode = _.CompanyCode;
                    var company = (from d in db.Companies
                                    where !d.IsReseller
                                    where d.CompanyCode == companyCode
                                    select d).FirstOrDefault();

                    if (company == null)
                        throw new Exception("Unable to find company " + companyCode);
                    else
                    {
                        return Negotiate.WithModel(new { resellerCode = _.ResellerCode, company = company })
                                        .WithStatusCode(HttpStatusCode.OK);
                    }
                }
                catch (Exception ex)
                {
                    logger.ErrorFormat("Error retrieving company {0} from the database. Error: {1}", _.CompanyCode, ex.ToString());
                    return Negotiate.WithModel(new { resellerCode = _.ResellerCode, error = ex.Message })
                                    .WithStatusCode(HttpStatusCode.InternalServerError);
                }
                finally
                {
                    if (db != null)
                        db.Dispose();
                }
                #endregion
            };
        }
    }
}
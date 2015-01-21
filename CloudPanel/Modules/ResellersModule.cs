using CloudPanel.ActiveDirectory;
using CloudPanel.Base.AD;
using CloudPanel.Base.Config;
using CloudPanel.Base.Database.Models;
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
    public class ResellersModule : NancyModule
    {
        private static readonly ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public ResellersModule() : base("/resellers")
        {
            this.RequiresAnyClaim(new[] { "SuperAdmin" });

            Get["/", c => c.Request.Accept("text/html")] = _ =>
                {
                    return View["resellers.cshtml"];
                };

            Get["/", c => !c.Request.Accept("text/html")] = _ =>
                {
                    #region Returns the resellers view with model or json data based on the request
                    CloudPanelContext db = null;
                    try
                    {
                        logger.DebugFormat("Opening connection to database");
                        db = new CloudPanelContext(Settings.ConnectionString);
                        db.Database.Connection.Open();

                        // Retrieve all resellers from the database
                        logger.DebugFormat("Retrieving a list of resellers from the database");
                        var resellers = (from d in db.Companies
                                         where d.IsReseller
                                         orderby d.CompanyName
                                         select new
                                         {
                                             CompanyCode = d.CompanyCode,
                                             CompanyName = d.CompanyName,
                                             City = d.City,
                                             State = d.State,
                                             ZipCode = d.ZipCode,
                                             Country = d.Country,
                                             Created = d.Created,
                                             TotalCompanies = (from c in db.Companies
                                                               where c.ResellerCode == d.CompanyCode
                                                               select c).Count()
                                         }).ToList();

                        logger.DebugFormat("Found a total of {0} resellers in the database", resellers.Count);
                        int draw = 0, start = 0, length = 0, recordsTotal = resellers.Count, recordsFiltered = resellers.Count, orderColumn = 0;
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
                                logger.DebugFormat("Search value of '{0}' has been provided", searchValue);
                                resellers = (from d in resellers
                                             where d.CompanyCode.IndexOf(searchValue, StringComparison.InvariantCultureIgnoreCase) != -1 ||
                                                   d.CompanyName.IndexOf(searchValue, StringComparison.InvariantCultureIgnoreCase) != -1 ||
                                                   (d.City.Length > 0 && d.City.IndexOf(searchValue, StringComparison.InvariantCultureIgnoreCase) != -1) ||
                                                   (d.State.Length > 0 && d.State.IndexOf(searchValue, StringComparison.InvariantCultureIgnoreCase) != -1) ||
                                                   (d.ZipCode.Length > 0 && d.ZipCode.IndexOf(searchValue, StringComparison.InvariantCultureIgnoreCase) != -1) ||
                                                   (d.Country.Length > 0 && d.Country.IndexOf(searchValue, StringComparison.InvariantCultureIgnoreCase) != -1) 
                                             select d).ToList();
                                recordsFiltered = resellers.Count;
                            }

                            logger.DebugFormat("Total resellers returned after filtering is {0}. Sorting acsending? {1}", recordsFiltered, isAscendingOrder);
                            if (isAscendingOrder)
                                resellers = resellers.OrderBy(x => x.GetType()
                                                        .GetProperty(orderColumnName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance).GetValue(x, null))
                                                        .Skip(start)
                                                        .Take( (length > 0 ? length : resellers.Count) )
                                                        .ToList();
                            else
                                resellers = resellers.OrderByDescending(x => x.GetType()
                                                        .GetProperty(orderColumnName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance).GetValue(x, null))
                                                        .Skip(start)
                                                        .Take( (length > 0 ? length : resellers.Count) )
                                                        .ToList();
                        }

                        return Negotiate.WithModel(new
                                        {
                                            draw = draw,
                                            recordsTotal = recordsTotal,
                                            recordsFiltered = recordsFiltered,
                                            data = resellers
                                        })
                                        .WithStatusCode(HttpStatusCode.OK);
                    }
                    catch (Exception ex)
                    {
                        logger.ErrorFormat("Failed to retrieve all resellers: {0}", ex.ToString());
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
                    #region Creates a new reseller

                    CloudPanelContext db = null;
                    ADOrganizationalUnits org = null;
                    ADGroups groups = null;
                    ReverseActions reverse = null;
                    try
                    {
                        logger.DebugFormat("Request to create reseller started...");
                        db = new CloudPanelContext(Settings.ConnectionString);

                        logger.DebugFormat("Binding form to reseller object");
                        var newReseller = this.Bind<Companies>();
                        newReseller.IsReseller = true;
                        
                        // Validate the required fields
                        if (string.IsNullOrEmpty(newReseller.CompanyName))
                            throw new MissingFieldException("Company name is required.");

                        if (string.IsNullOrEmpty(newReseller.AdminName))
                            throw new MissingFieldException("Contact name is required.");

                        if (string.IsNullOrEmpty(newReseller.AdminEmail))
                            throw new MissingFieldException("Contact email is required.");

                        // Lets get the company code
                        logger.DebugFormat("Generating new reseller code for reseller {0}", newReseller.CompanyName);
                        string companyCode = CPStaticHelpers.GenerateCompanyCode(newReseller.CompanyName);

                        // Initialize classes needed
                        logger.DebugFormat("New reseller code for {0} is {1}", newReseller.CompanyName, companyCode);
                        org = new ADOrganizationalUnits(Settings.Username, Settings.DecryptedPassword, Settings.PrimaryDC);
                        groups = new ADGroups(Settings.Username, Settings.DecryptedPassword, Settings.PrimaryDC);
                        reverse = new ReverseActions();

                        // Create reseller in Active Directory
                        logger.DebugFormat("Populating OrganizationalUnit object");
                        var newResellerOrg = new OrganizationalUnit() {
                            Name = Settings.UseNameInsteadOfCompanyCode ? newReseller.CompanyName : companyCode,
                            Street = newReseller.Street,
                            City = newReseller.City,
                            State = newReseller.State,
                            PostalCode = newReseller.ZipCode,
                            Country = newReseller.Country,
                            DisplayName = newReseller.CompanyName,
                            AdminDisplayName = newReseller.AdminName,
                            AdminDescription = newReseller.AdminEmail
                        };

                        logger.DebugFormat("Creating new organizational unit in Active Directory");
                        var createdOrg = org.Create(Settings.HostingOU, newResellerOrg); // Create the new OU and get the extra information back
                        reverse.AddAction(Actions.CreateOrganizationalUnit, createdOrg.DistinguishedName); // Add to our rollback actions in case we need to remove

                        // Create the GPOAccess group
                        logger.DebugFormat("New organizational unit created for reseller {0}. New distinguished name is {1}. Populating SecurityGroup object.", newReseller.CompanyName, createdOrg.DistinguishedName);
                        var gpoAccessGroup = new SecurityGroup();
                        gpoAccessGroup.SamAccountName = string.Format("GPOAccess_{0}", companyCode);
                        gpoAccessGroup.DisplayName = string.Format("GPOAccess@{0}", companyCode);
                        gpoAccessGroup.Name = gpoAccessGroup.DisplayName;
                        
                        logger.DebugFormat("Creating new security group for {0} called {1}", newReseller.CompanyName, gpoAccessGroup.Name);
                        var createdGroup = groups.Create(createdOrg.DistinguishedName, gpoAccessGroup);
                        reverse.AddAction(Actions.CreateSecurityGroup, createdGroup.Name);

                        // Create the terminal server users group
                        logger.DebugFormat("Populating SecurityGroup object");
                        var allTSUsersGroup = new SecurityGroup();
                        allTSUsersGroup.SamAccountName = string.Format("AllTSUsers_{0}", companyCode);
                        allTSUsersGroup.DisplayName = string.Format("AllTSUsers@{0}", companyCode);
                        allTSUsersGroup.Name = allTSUsersGroup.DisplayName;

                        logger.DebugFormat("Creating new security group for {0} called {1}", newReseller.CompanyName, allTSUsersGroup.Name);
                        createdGroup = groups.Create(createdOrg.DistinguishedName, allTSUsersGroup);
                        reverse.AddAction(Actions.CreateSecurityGroup, createdGroup.Name);

                        // Add reseller to database
                        logger.DebugFormat("Adding reseller {0} to the database", newReseller.CompanyName);

                        newReseller.Created = DateTime.Now;
                        newReseller.CompanyCode = companyCode;
                        newReseller.DistinguishedName = createdOrg.DistinguishedName;
                        db.Companies.Add(newReseller);
                        db.SaveChanges();

                        logger.InfoFormat("Successfully created new reseller {0}", newReseller.CompanyName);
                        return Negotiate.WithModel(new { success = "Successfully created new reseller" })
                                        .WithView("resellers.cshtml")
                                        .WithStatusCode(HttpStatusCode.OK);
                    }
                    catch (Exception ex)
                    {
                        logger.ErrorFormat("Error creating reseller: {0}", ex.ToString());

                        reverse.RollbackNow();
                        return Negotiate.WithModel(new { error = ex.Message })
                                        .WithView("resellers.cshtml")
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
                    #region Updates an existing reseller

                    CloudPanelContext db = null;
                    ADOrganizationalUnits org = null;
                    try
                    {
                        logger.DebugFormat("Request to update reseller started...");
                        db = new CloudPanelContext(Settings.ConnectionString);

                        logger.DebugFormat("Binding form to reseller object");
                        var updatedReseller = this.Bind<Companies>();

                        // Validate the required fields
                        if (string.IsNullOrEmpty(updatedReseller.CompanyName))
                            throw new MissingFieldException("Company name is required.");

                        if (string.IsNullOrEmpty(updatedReseller.AdminName))
                            throw new MissingFieldException("Contact name is required.");

                        if (string.IsNullOrEmpty(updatedReseller.AdminEmail))
                            throw new MissingFieldException("Contact email is required.");

                        if (string.IsNullOrEmpty(updatedReseller.CompanyCode))
                            throw new MissingFieldException("You must provide the reseller code to update");

                        // Initialize classes needed
                        logger.DebugFormat("Initializing org object");
                        org = new ADOrganizationalUnits(Settings.Username, Settings.DecryptedPassword, Settings.PrimaryDC);
                        
                        // Get existing reseller from database
                        logger.DebugFormat("Looking for the existing reseller {0} in the database", updatedReseller.CompanyCode);
                        var existingReseller = (from d in db.Companies
                                                where d.IsReseller
                                                where d.CompanyCode == updatedReseller.CompanyCode
                                                select d).FirstOrDefault();
                        if (existingReseller == null)
                            throw new Exception("Reseller " + updatedReseller.CompanyCode + " was not found.");

                        logger.DebugFormat("Creating new organizational unit object to update active directory on {0}", existingReseller.DistinguishedName);
                        OrganizationalUnit updatedOrg = new OrganizationalUnit();
                        updatedOrg.DistinguishedName = existingReseller.DistinguishedName;
                        updatedOrg.DisplayName = updatedReseller.CompanyName;
                        updatedOrg.Street = updatedReseller.Street;
                        updatedOrg.City = updatedReseller.City;
                        updatedOrg.State = updatedReseller.State;
                        updatedOrg.PostalCode = updatedReseller.ZipCode;
                        updatedOrg.Country = updatedReseller.Country;
                        updatedOrg.AdminDisplayName = updatedReseller.AdminName;
                        updatedOrg.AdminDescription = updatedReseller.AdminEmail;

                        logger.DebugFormat("Updating organizational unit in active directory");
                        org.Update(updatedOrg);

                        logger.DebugFormat("Updated values in database");
                        existingReseller.CompanyName = updatedReseller.CompanyName;
                        existingReseller.Street = updatedReseller.Street;
                        existingReseller.City = updatedReseller.City;
                        existingReseller.State = updatedReseller.State;
                        existingReseller.ZipCode = updatedReseller.ZipCode;
                        existingReseller.Country = updatedReseller.Country;
                        existingReseller.AdminName = updatedReseller.AdminName;
                        existingReseller.AdminEmail = updatedReseller.AdminEmail;
                        existingReseller.Website = updatedReseller.Website;

                        logger.DebugFormat("Saving data to the database");
                        db.SaveChanges();

                        logger.InfoFormat("Successfully updated reseller {0}", updatedReseller.CompanyName);
                        return Negotiate.WithModel(new { success = "Successfully updated existing reseller" })
                                        .WithView("resellers.cshtml")
                                        .WithStatusCode(HttpStatusCode.OK);
                    }
                    catch (Exception ex)
                    {
                        logger.ErrorFormat("Error updating reseller: {0}", ex.ToString());
                        return Negotiate.WithModel(new { error = ex.Message })
                                        .WithView("resellers.cshtml")
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
                    #region Deletes a reseller from the database. It does not delete the OU since that could be very destructive
                    CloudPanelContext db = null;
                    ADOrganizationalUnits org = null;
                    try
                    {
                        logger.DebugFormat("Preparing to delete reseller");
                        db = new CloudPanelContext(Settings.ConnectionString);
                        db.Database.Connection.Open();

                        // Make sure there are no companies assigned to this reseller
                        string resellerCode = Request.Form.ResellerCode;
                        logger.DebugFormat("Making sure there are no companies under reseller {0}", resellerCode);
                        var companyCount = (from d in db.Companies
                                            where !d.IsReseller
                                            where d.ResellerCode == resellerCode
                                            select d.CompanyId).Count();

                        if (companyCount > 0)
                            throw new Exception("You cannot delete this reseller because it contains " + companyCount.ToString() + " companies");
                        else
                        {
                            logger.DebugFormat("No companies were found under reseller {0}. Pulling reseller from database", resellerCode);
                            var reseller = (from d in db.Companies
                                            where d.IsReseller
                                            where d.CompanyCode == resellerCode
                                            select d).FirstOrDefault();

                            if (reseller == null)
                                throw new Exception("Unable to find the reseller in the database.");
                            else
                            {
                                logger.DebugFormat("Removing {0} from Active Directory", reseller.DistinguishedName);
                                org = new ADOrganizationalUnits(Settings.Username, Settings.DecryptedPassword, Settings.PrimaryDC);
                                org.Delete(reseller.DistinguishedName, true);

                                logger.DebugFormat("Removing reseller {0} from database", reseller.CompanyName);
                                db.Companies.Remove(reseller);
                                db.SaveChanges();

                                // See if the current reseller selected is the reseller the user just deletected
                                // If it is then we need to clear it from the user context
                                var user = this.Context.CurrentUser as AuthenticatedUser;
                                if (user.SelectedResellerCode.Equals(resellerCode))
                                {
                                    user.SelectedResellerCode = string.Empty;
                                    user.SelectedResellerName = string.Empty;

                                    // Remove selected company from the context to since it will be under this reseller and couldn't exist
                                    user.SelectedCompanyCode = string.Empty;
                                    user.SelectedCompanyName = string.Empty;
                                }

                                return Negotiate.WithModel(new { success = "Reseller was deleted successfully" })
                                            .WithView("resellers.cshtml")
                                            .WithStatusCode(HttpStatusCode.OK);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.ErrorFormat("Error deleting reseller: {0}", ex.ToString());
                        return Negotiate.WithModel(new { error = ex.Message })
                                        .WithView("resellers.cshtml")
                                        .WithStatusCode(HttpStatusCode.InternalServerError);
                    }
                    finally
                    {
                        if (db != null)
                            db.Dispose();
                    }
                    #endregion
                };

            Get["/{ResellerCode}", c => c.Request.Accept("application/json")] = _ =>
                {
                    #region Gets a specific reseller
                    CloudPanelContext db = null;
                    try
                    {
                        db = new CloudPanelContext(Settings.ConnectionString);

                        string resellerCode = _.ResellerCode;
                        var reseller = (from d in db.Companies
                                            where d.IsReseller
                                            where d.CompanyCode == resellerCode
                                            select d).FirstOrDefault();

                        if (reseller == null)
                            throw new Exception("Unable to find reseller " + resellerCode);
                        else
                        {
                            return Negotiate.WithModel(new { reseller = reseller })
                                            .WithStatusCode(HttpStatusCode.OK);
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.ErrorFormat("Error retrieving reseller {0} from the database. Error: {1}", _.ResellerCode, ex.ToString());
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
        }
    }
}
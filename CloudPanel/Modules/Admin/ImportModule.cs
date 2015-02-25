using CloudPanel.ActiveDirectory;
using CloudPanel.Base.Config;
using CloudPanel.Database.EntityFramework;
using log4net;
using Nancy;
using Nancy.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using CloudPanel.Base.Models.Database;
using CloudPanel.Base.AD;
using Nancy.Responses.Negotiation;
using CloudPanel.Base.Models.ViewModels;
using CloudPanel.Exchange;
using CloudPanel.Base.Enums;
using Nancy.Security;

namespace CloudPanel.Modules.Admin
{
    public class ImportModule : NancyModule
    {
        private readonly ILog logger = LogManager.GetLogger(typeof(ImportModule));

        public ImportModule() : base("/import")
        {
            this.RequiresClaims(new[] { "SuperAdmin" });

            Get["/{ResellerCode}"] = _ =>
                {
                    #region Get a list of organizational units in the reseller OU that are not in the database
                    CloudPanelContext db = null;
                    ADOrganizationalUnits org = null;
                    try
                    {
                        string baseUrl = string.Format("OU={0},{1}", _.ResellerCode, Settings.HostingOU);

                        // Get all child OU's
                        org = new ADOrganizationalUnits(Settings.Username, Settings.DecryptedPassword, Settings.PrimaryDC);
                        var children = org.GetChildOUs(baseUrl);

                        // Get current companies
                        db = new CloudPanelContext(Settings.ConnectionString);
                        var sqlCompanies = (from d in db.Companies
                                            select d.DistinguishedName).ToList();

                        // Only select the ones that are not currently in the database
                        children = children.Where(x => !sqlCompanies.Any(s => s.Equals(x.DistinguishedName, StringComparison.CurrentCultureIgnoreCase)))
                                           .ToList();

                        return Negotiate.WithModel(new { Companies = children })
                                        .WithView("Admin/import_companies.cshtml");
                    }
                    catch (Exception ex)
                    {
                        logger.ErrorFormat("Error getting organizational units for {0}: {1}", _.ResellerCode, ex.ToString());
                        return Negotiate.WithModel(new { error = ex.Message })
                                        .WithStatusCode(HttpStatusCode.InternalServerError);
                    }
                    finally
                    {
                        if (org != null)
                            org.Dispose();

                        if (db != null)
                            db.Dispose();
                    }
                    #endregion
                };

            Post["/{ResellerCode}"] = _ =>
                {
                    #region Import companies
                    ADOrganizationalUnits org = null;
                    CloudPanelContext db = null;
                    ADGroups group = null;

                    dynamic powershell = null;
                    try
                    {
                        var checkedCompanies = this.Bind<List<ImportCompanyViewModel>>();
                        if (checkedCompanies.Count < 1)
                            throw new Exception("No companies were selected");

                        group = new ADGroups(Settings.Username, Settings.DecryptedPassword, Settings.PrimaryDC);
                        org = new ADOrganizationalUnits(Settings.Username, Settings.DecryptedPassword, Settings.PrimaryDC);
                        db = new CloudPanelContext(Settings.ConnectionString);
                        db.Database.Connection.Open();

                        // Get a list of Exchange accepted domains
                        List<Domains> acceptedDomains = null;
                        if (Settings.ExchangeModule)
                        {
                            powershell = ExchPowershell.GetClass();
                            acceptedDomains = powershell.Get_AcceptedDomains();
                        }

                        // List of companies to add to the database
                        List<Companies> addedCompanies = new List<Companies>();
                        foreach (var c in checkedCompanies)
                        {
                            if (c.IsChecked)
                            {
                                logger.DebugFormat("Getting company organizational unit {0}", c.DistinguishedName);
                                var company = org.GetOU(c.DistinguishedName);

                                var newCompany = new Companies();
                                newCompany.Created = DateTime.Parse(company.WhenCreated);
                                newCompany.DistinguishedName = company.DistinguishedName;
                                newCompany.CompanyCode = company.Name;
                                newCompany.OrgPlanID = c.CompanyPlan;
                                newCompany.ResellerCode = _.ResellerCode;
                                newCompany.CompanyName = company.DisplayName;
                                newCompany.Street = string.IsNullOrEmpty(company.Street) ? "Unknown" : company.Street;
                                newCompany.City = string.IsNullOrEmpty(company.City) ? "Unknown" : company.City;
                                newCompany.State = string.IsNullOrEmpty(company.State) ? "Unknown" : company.State;
                                newCompany.ZipCode = string.IsNullOrEmpty(company.PostalCode) ? "Unknown" : company.PostalCode;
                                newCompany.AdminName = string.IsNullOrEmpty(company.AdminDisplayName) ? "Unknown" : company.AdminDisplayName;
                                newCompany.AdminEmail = "Unknown";
                                newCompany.PhoneNumber = "Unknown";
                                newCompany.ExchEnabled = c.IsExchangeEnabled;

                                // Create missing organizational units
                                logger.DebugFormat("Creating organizational units");
                                org.Create(newCompany.DistinguishedName, new OrganizationalUnit() { Name = Settings.ExchangeOUName });
                                org.Create(newCompany.DistinguishedName, new OrganizationalUnit() { Name = Settings.ApplicationsOUName });
                                org.Create(newCompany.DistinguishedName, new OrganizationalUnit() { Name = Settings.ExchangeGroupsOU });
                                org.Create(newCompany.DistinguishedName, new OrganizationalUnit() { Name = Settings.ExchangeContactsOU });
                                org.Create(newCompany.DistinguishedName, new OrganizationalUnit() { Name = Settings.ExchangeRoomsOU });
                                org.Create(newCompany.DistinguishedName, new OrganizationalUnit() { Name = Settings.ExchangeResourceOU });

                                if (!string.IsNullOrEmpty(Settings.UsersOU))
                                    org.Create(newCompany.DistinguishedName, new OrganizationalUnit() { Name = Settings.UsersOU });

                                // Create security groups
                                logger.DebugFormat("Creating security groups");
                                string cc = newCompany.CompanyCode;
                                group.Create(c.DistinguishedName, new SecurityGroup() { Name = "AllTSUsers@" + cc, SamAccountName = "AllTSUsers@" + cc }, true);
                                group.Create(c.DistinguishedName, new SecurityGroup() { Name = "AllUsers@" + cc, SamAccountName = "AllUsers@" + cc }, true);
                                group.Create(c.DistinguishedName, new SecurityGroup() { Name = "Admins@" + cc, SamAccountName = "Admins@" + cc }, true);

                                // Add company to database
                                db.Companies.Add(newCompany);

                                // Add all domains
                                foreach (var domain in company.UPNSuffixes)
                                {
                                    var domainExist = (from d in db.Domains
                                                       where d.Domain == domain
                                                       select d).Count();

                                    if (domainExist > 0)
                                        throw new Exception("Domain " + domain + " already exists in the database");
                                    else
                                    {
                                        // Add domain to the database
                                        Domains newDomain = new Domains();
                                        newDomain.CompanyCode = newCompany.CompanyCode;
                                        newDomain.Domain = domain;
                                        newDomain.DomainType = DomainType.Default;
                                        newDomain.IsLyncDomain = false;
                                        newDomain.IsSubDomain = false;

                                        // Check if set to enable Exchange to see if the accepted domain exist
                                        if (c.IsExchangeEnabled)
                                        {
                                            var acceptedDomain = acceptedDomains.Where(x => x.Domain == newDomain.Domain).FirstOrDefault();
                                            if (acceptedDomain != null)
                                            {
                                                // Accepted domain exists. Set the values
                                                newDomain.DomainType = acceptedDomain.DomainType;
                                                newDomain.IsAcceptedDomain = true;
                                            }
                                        }
                                        
                                        // Add to database
                                        db.Domains.Add(newDomain);
                                    }
                                }
                            }
                        }

                        db.SaveChanges();
                        string redirect = string.Format("~/import/{0}", _.ResellerCode);
                        return Negotiate.WithModel(new { success = "Successfully imported companies" })
                                        .WithMediaRangeResponse(new MediaRange("text/html"), this.Response.AsRedirect(redirect));
                    }
                    catch (Exception ex)
                    {
                        logger.ErrorFormat("Error importing companies: {0}", ex.ToString());
                        return Negotiate.WithModel(new { error = ex.Message })
                                        .WithView("error/500.cshtml")
                                        .WithStatusCode(HttpStatusCode.InternalServerError);
                    }
                    finally
                    {
                        if (powershell != null)
                            powershell.Dispose();

                        if (group != null)
                            group.Dispose();

                        if (org != null)
                            org.Dispose();

                        if (db != null)
                            db.Dispose();
                    }
                    #endregion
                };
        }
    }
}
using CloudPanel.Base.Config;
using CloudPanel.Base.Database.Models;
using CloudPanel.Database.EntityFramework;
using log4net;
using Nancy;
using Nancy.ModelBinding;
using Nancy.Security;
using System;
using System.Linq;

namespace CloudPanel.Modules
{
    public class CompanyOverviewModule : NancyModule
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(CompanyOverviewModule));

        public CompanyOverviewModule() : base("/company/{CompanyCode}/overview")
        {
            this.RequiresAuthentication();

            Get["/"] = _ =>
            {
                string companyCode = _.CompanyCode;
                this.Context.SetCompanyCode(companyCode);
                logger.DebugFormat("Setting selected company code for {0} to {1}", this.Context.CurrentUser.UserName, _.CompanyCode);

                CloudPanelContext db = null;
                try
                {
                    db = new CloudPanelContext(Settings.ConnectionString);
                    db.Database.Connection.Open();

                    logger.DebugFormat("Getting company {0} from the database", companyCode);
                    var company = (from d in db.Companies
                                   where !d.IsReseller
                                   where d.CompanyCode == companyCode
                                   select d).FirstOrDefault();

                    logger.DebugFormat("Looking up company plan {0} for {1}", company.OrgPlanID, companyCode);
                    var companyPlan = (from d in db.Plans_Organization
                                       where d.OrgPlanID == company.OrgPlanID
                                       select d).FirstOrDefault();

                    // Pull data for overview graph
                    logger.DebugFormat("Querying total users for {0}", companyCode);
                    var totalUsers = (from d in db.Users where d.CompanyCode == companyCode select d.ID).Count();

                    logger.DebugFormat("Querying total domains for {0}", companyCode);
                    var totalDomains = (from d in db.Domains where d.CompanyCode == companyCode select d.DomainID).Count();

                    logger.DebugFormat("Querying total mailboxes for {0}", companyCode);
                    var totalMailboxes = (from d in db.Users where d.CompanyCode == companyCode where d.MailboxPlan > 0 select d.ID).Count();

                    logger.DebugFormat("Querying total contacts for {0}", companyCode);
                    var totalContacts = (from d in db.Contacts where d.CompanyCode == companyCode select d).Count();

                    logger.DebugFormat("Querying total groups for {0}", companyCode);
                    var totalGroups = (from d in db.DistributionGroups where d.CompanyCode == companyCode select d.ID).Count();

                    return Negotiate.WithModel(new 
                                    {
                                        company = company,
                                        companyPlan = companyPlan,
                                        totalUsers = totalUsers,
                                        totalDomains = totalDomains,
                                        totalMailboxes = totalMailboxes,
                                        totalContacts = totalContacts,
                                        totalGroups = totalGroups
                                    })
                                    .WithView("Company/company_overview.cshtml");
                }
                catch (Exception ex)
                {
                    logger.ErrorFormat("Error getting company overview page for {0}: {1}", _.CompanyCode, ex.ToString());

                    ViewBag.error = ex.ToString();
                    return Negotiate.WithModel(new { error = ex.Message })
                                    .WithView("error.cshtml");
                }
                finally
                {
                    if (db != null)
                        db.Dispose();
                }
            };

            Put["/"] = _ =>
            {
                CloudPanelContext db = null;
                try
                {
                    db = new CloudPanelContext(Settings.ConnectionString);
                    db.Database.Connection.Open();

                    string companyCode = _.CompanyCode;
                    var company = (from d in db.Companies
                                   where !d.IsReseller
                                   where d.CompanyCode == companyCode
                                   select d).FirstOrDefault();

                    if (company == null)
                        throw new Exception("Unable to find company in database");
                    else
                    {
                        var updatedCompany = this.Bind<Companies>();

                        if (!string.IsNullOrEmpty(updatedCompany.CompanyName))
                            company.CompanyName = updatedCompany.CompanyName;
                        
                        if (!string.IsNullOrEmpty(updatedCompany.AdminName))
                            company.AdminName = updatedCompany.AdminName;
                        
                        if (!string.IsNullOrEmpty(updatedCompany.AdminEmail))
                            company.AdminEmail = updatedCompany.AdminEmail;

                        if (updatedCompany.OrgPlanID > 0) // Do not update company if its equal to or less than zero
                            company.OrgPlanID = updatedCompany.OrgPlanID;
                        
                        company.PhoneNumber = updatedCompany.PhoneNumber;
                        company.Website = updatedCompany.Website;
                        db.SaveChanges();

                        string returnLocation = string.Format("~/company/{0}/overview", companyCode);
                        return this.Response.AsRedirect(returnLocation);
                    }
                }
                catch (Exception ex)
                {
                    logger.ErrorFormat("Error updating company overview settings for {0}: {1}", _.CompanyCode, ex.ToString());

                    ViewBag.error = ex.ToString();
                    return Negotiate.WithModel(new { error = ex.Message })
                                    .WithView("error.cshtml");
                }
                finally
                {
                    if (db != null)
                        db.Dispose();
                }
            };
        }
    }
}
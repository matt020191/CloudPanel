using CloudPanel.Base.Config;
using CloudPanel.Database.EntityFramework;
using Nancy;
using Nancy.Security;
using Nancy.ModelBinding;
using CloudPanel.Code;
using System;
using System.Linq;
using CloudPanel.Base.Database.Models;
using log4net;

namespace CloudPanel.Modules
{
    public class OverviewModule : NancyModule
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(OverviewModule));

        public OverviewModule() : base("/company/{CompanyCode}/overview")
        {
            this.RequiresAuthentication();

            Get["/"] = _ =>
            {
                string companyCode = _.CompanyCode;
                this.RequiresAnyClaim(new[] { "SuperAdmin", "ResellerAdmin", companyCode });

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
                                    .WithView("Company/overview.cshtml");
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

            Put["/", c => !c.Request.Accept("text/html")] = _ =>
            {
                string companyCode = _.CompanyCode;
                this.RequiresAnyClaim(new[] { "SuperAdmin", "ResellerAdmin", companyCode });

                CloudPanelContext db = null;
                try
                {
                    db = new CloudPanelContext(Settings.ConnectionString);
                    db.Database.Connection.Open();

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
                        {
                            if (this.Context.IsSuperOrResellerAdmin()) // Only update value if they are super or reseller admin
                                company.OrgPlanID = updatedCompany.OrgPlanID;

                        }
                        
                        company.PhoneNumber = updatedCompany.PhoneNumber;
                        company.Website = updatedCompany.Website;
                        db.SaveChanges();

                        return Negotiate.WithModel(new { success = "Successfully updated company values" })
                                        .WithStatusCode(HttpStatusCode.OK);
                    }
                }
                catch (Exception ex)
                {
                    logger.ErrorFormat("Error updating company overview settings for {0}: {1}", _.CompanyCode, ex.ToString());
                    return Negotiate.WithModel(new { error = ex.Message })
                                    .WithStatusCode(HttpStatusCode.InternalServerError);
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
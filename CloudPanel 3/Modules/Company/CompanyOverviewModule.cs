using CloudPanel.Base.Config;
using CloudPanel.Database.EntityFramework;
using Nancy;
using Nancy.Security;
using System;
using System.Linq;

namespace CloudPanel.Modules
{
    public class CompanyOverviewModule : NancyModule
    {
        public CompanyOverviewModule() : base("/company/{CompanyCode}/overview")
        {
            this.RequiresAuthentication();

            Get["/"] = _ =>
            {
                string companyCode = _.CompanyCode;
                NancyContextHelpers.SetSelectedCompanyCode(this.Context, _.CompanyCode);

                CloudPanelContext db = null;
                try
                {
                    db = new CloudPanelContext(Settings.ConnectionString);
                    db.Database.Connection.Open();

                    var company = (from d in db.Companies
                                   where !d.IsReseller
                                   where d.CompanyCode == companyCode
                                   select d).FirstOrDefault();

                    var companyPlan = (from d in db.Plans_Organization
                                       where d.OrgPlanID == company.OrgPlanID
                                       select d).FirstOrDefault();

                    // Pull data for overview graph
                    var totalUsers = (from d in db.Users where d.CompanyCode == companyCode select d.ID).Count();
                    var totalDomains = (from d in db.Domains where d.CompanyCode == companyCode select d.DomainID).Count();
                    var totalMailboxes = (from d in db.Users where d.CompanyCode == companyCode where d.MailboxPlan > 0 select d.ID).Count();
                    var totalContacts = (from d in db.Contacts where d.CompanyCode == companyCode select d.ID).Count();
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
                                    .WithMediaRangeModel("application/json", new
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
                    throw;
                }
                finally
                {
                    if (db != null)
                        db.Dispose();
                }
            };

            Post["/{CompanyCode}"] = _ =>
            {
                return HttpStatusCode.InternalServerError;
            };

            Put["/{CompanyCode}"] = _ =>
            {
                return HttpStatusCode.InternalServerError;
            };

            Delete["/{CompanyCode}"] = _ =>
            {
                return HttpStatusCode.InternalServerError;
            };
        }
    }
}
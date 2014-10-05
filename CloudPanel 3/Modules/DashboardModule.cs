using CloudPanel.Base.Config;
using CloudPanel.Database.EntityFramework;
using Nancy;
using Nancy.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CloudPanel.Modules
{
    public class DashboardModule : NancyModule
    {
        public DashboardModule() : base("/dashboard")
        {
            this.RequiresAuthentication();

            Get["/"] = _ =>
                {
                    CloudPanelContext db = null;
                    try
                    {
                        db = new CloudPanelContext(Settings.ConnectionString);
                        db.Database.Connection.Open();

                        // Get totals
                        var resellers = (from d in db.Companies where d.IsReseller select d.CompanyId).Count();
                        var companies = (from d in db.Companies where !d.IsReseller select d.CompanyId).Count();
                        var users = (from d in db.Users select d.ID).Count();
                        var mailboxes = (from d in db.Users where d.MailboxPlan > 0 select d.ID).Count();

                        // Calculate mailbox allocated space
                        var mailboxAllocated = (from d in db.Users
                                                join d1 in db.Plans_ExchangeMailbox on d.MailboxPlan equals d1.MailboxPlanID into d1Tmp
                                                from data1 in d1Tmp.DefaultIfEmpty()
                                                where d.MailboxPlan > 0
                                                select new
                                                {
                                                    AdditionalMB = d.AdditionalMB == null ? 0 : d.AdditionalMB,
                                                    DefaultMB = data1.MailboxSizeMB
                                                }).ToList();

                        long totalAllocated = 0;
                        mailboxAllocated.ForEach(x => totalAllocated += ((int)x.AdditionalMB + x.DefaultMB));
                        if (totalAllocated > 0)
                            totalAllocated = (totalAllocated * 1024) * 1024; // Convert from megabytes to bytes

                        return Negotiate.WithModel(new
                                    {
                                        totalResellers = resellers,
                                        totalCompanies = companies,
                                        totalUsers = users,
                                        totalMailboxes = mailboxes,
                                        totalAllocated = CPStaticHelpers.FormatBytes(totalAllocated)
                                    })
                                    .WithMediaRangeModel("application/json", new
                                    {
                                        totalResellers = resellers,
                                        totalCompanies = companies,
                                        totalUsers = users,
                                        totalMailboxes = mailboxes,
                                        totalAllocated = CPStaticHelpers.FormatBytes(totalAllocated)
                                    })
                                    .WithView("dashboard_super.cshtml");
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
        }
    }
}
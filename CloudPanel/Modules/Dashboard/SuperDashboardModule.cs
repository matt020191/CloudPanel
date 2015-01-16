using CloudPanel.Base.Config;
using CloudPanel.Database.EntityFramework;
using Nancy;
using Nancy.Security;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Objects;
using System.Linq;
using System.Web;

namespace CloudPanel.Modules.Dashboard
{
    public class SuperDashboardModule : NancyModule
    {
        public SuperDashboardModule() : base("/dashboard")
        {
            this.RequiresClaims(new[] { "SuperAdmin" });

            Get["/"] = _ =>
                {
                    return View["Dashboard/dashboard_super.cshtml"];
                };

            Get["/all"] = _ =>
                {
                    using (CloudPanelContext db = new CloudPanelContext(Settings.ConnectionString))
                    {
                        db.Database.Connection.Open();

                        var resellers = db.Companies.Where(x => x.IsReseller).Count();
                        var companies = db.Companies.Where(x => !x.IsReseller).Count();
                        var users = db.Users.Count();
                        var mailboxes = db.Users.Where(x => x.MailboxPlan > 0).Count();

                        var mailboxAllocated = db.Users.Where(x => x.MailboxPlan > 0)
                                                       .Join(db.Plans_ExchangeMailbox,
                                                                s => s.MailboxPlan, s2 => s2.MailboxPlanID,
                                                                (s, s2) => new { User = s, Plan = s2 })
                                                                .Select(x => new
                                                                {
                                                                    User = x.User.UserGuid,
                                                                    Size = x.Plan.MailboxSizeMB + (x.User.AdditionalMB == null ? 0 : (int)x.User.AdditionalMB)
                                                                })
                                                                .Sum(x => x.Size);

                        var mailboxUsed = db.Users.Where(x => x.MailboxPlan > 0)
                                                  .Join(db.StatMailboxSize,
                                                           s => s.UserGuid, s2 => s2.UserGuid,
                                                           (s, s2) => new { User = s, Size = s2 })
                                                  .Select(x => new
                                                  {
                                                      User = x.User.UserGuid,
                                                      Size = x.Size.TotalItemSizeInBytes
                                                  }).Sum(x => x.Size);

                        string allocatedReadableSize = ByteSize.ByteSize.FromMegaBytes(mailboxAllocated).ToString("#.##");
                        string usedReadableSize = ByteSize.ByteSize.FromBytes(mailboxUsed).ToString("#.##");
                        return Negotiate.WithModel(new {
                            resellers = resellers,
                            companies = companies,
                            users = users,
                            mailboxes = mailboxes,
                            mailboxAllocated = allocatedReadableSize,
                            mailboxUsed = usedReadableSize
                        });
                    }
                };

            Get["/history/{MONTHS:int}"] = _ =>
                {
                    int months = _.MONTHS;

                    using (CloudPanelContext db = new CloudPanelContext(Settings.ConnectionString))
                    {
                        var statistics = (from d in db.Statistics
                                          where d.Retrieved >= (DateTime)DbFunctions.AddMonths(DateTime.Now, -months)
                                          group d by DbFunctions.TruncateTime(d.Retrieved) into g
                                          select new 
                                          {
                                              Retrieved = DbFunctions.TruncateTime(g.Key),
                                              UserCount = g.Sum(x => x.UserCount),
                                              MailboxCount = g.Sum(x => x.MailboxCount),
                                              CitrixCount = g.Sum(x => x.CitrixCount)
                                          }).ToList();

                        return Negotiate.WithModel(statistics);
                    }
                };

            Get["/exchange/databases"] = _ =>
                {
                    using (CloudPanelContext db = new CloudPanelContext(Settings.ConnectionString))
                    {
                        var databases = (from d in db.SvcMailboxDatabaseSizes
                                         where d.Retrieved == db.SvcMailboxDatabaseSizes.Max(x => x.Retrieved)
                                         select d).ToList();

                        return Negotiate.WithModel(databases);
                    }
                };

            Get["/resellers/{TOP:int}"] = _ =>
                {
                    int top = _.TOP;
                    using (CloudPanelContext db = new CloudPanelContext(Settings.ConnectionString))
                    {
                        var resellers = (from d in db.Companies
                                         where d.IsReseller
                                         select new
                                         {
                                             ResellerCode = d.CompanyCode,
                                             ResellerName = d.CompanyName,
                                             CompanyCount = db.Companies.Where(x => 
                                                             x.ResellerCode == d.CompanyCode).Count()
                                         }).ToList().Take(top);

                        return Negotiate.WithModel(resellers);
                    }
                };
        }
    }
}
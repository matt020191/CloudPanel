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
    public class AdminDashboardModule : NancyModule
    {
        public AdminDashboardModule() : base("/dashboard")
        {
            //this.RequiresClaims(new[] { "SuperAdmin" });

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

                        return Response.AsJson(statistics, HttpStatusCode.OK);
                    }
                };

            Get["/exchange/databases"] = _ =>
                {
                    using (CloudPanelContext db = new CloudPanelContext(Settings.ConnectionString))
                    {
                        var databases = (from d in db.SvcMailboxDatabaseSizes
                                         where d.Retrieved == db.SvcMailboxDatabaseSizes.Max(x => x.Retrieved)
                                         select d).ToList();

                        return Response.AsJson(databases, HttpStatusCode.OK);
                    }
                };
        }
    }
}
using CloudPanel.Base.Config;
using CloudPanel.Base.Charting;
using CloudPanel.Database.EntityFramework;
using Nancy;
using Nancy.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using log4net;

namespace CloudPanel.Modules
{
    public class SuperChartsModule : NancyModule
    {
        private static readonly ILog logger = log4net.LogManager.GetLogger(typeof(SuperChartsModule));

        public SuperChartsModule() : base("/charts/super")
        {
            //this.RequiresAuthentication();
            //this.RequiresAnyClaim(new[] { "SuperAdmin" });

            Get["/customers/top/{X:int}"] = _ =>
            {
                CloudPanelContext db = null;
                try
                {
                    int top = _.X;
                    logger.DebugFormat("Getting top {0} customers", top);

                    db = new CloudPanelContext(Settings.ConnectionString);
                    var topCustomers = (from d in db.Users
                                        group d by d.CompanyCode into grp
                                        select new
                                        {
                                            CompanyCode = grp.Key,
                                            Count = grp.Select(x => x.UserGuid).Distinct().Count()
                                        }).OrderByDescending(x => x.Count).Take(top).ToList();

                    logger.DebugFormat("Found a total of {0} companies", topCustomers.Count());
                    List<TopXCustomers> topX = new List<TopXCustomers>();
                    foreach (var data in topCustomers)
                    {
                        topX.Add(new TopXCustomers()
                            {
                                CompanyCode = data.CompanyCode,
                                TotalUsers = data.Count,
                                CompanyName = (from d in db.Companies where d.CompanyCode == data.CompanyCode select d.CompanyName).FirstOrDefault(),
                                TotalMailboxes = (from d in db.Users where d.CompanyCode == data.CompanyCode where d.MailboxPlan > 0 select d.ID).Count()
                            });
                    }

                    return Response.AsJson(new
                    {
                        data = topX
                    });
                }
                catch (Exception ex)
                {
                    logger.ErrorFormat("Error getting top X customers: {0}", ex.ToString());
                    throw;
                }
                finally
                {
                    if (db != null)
                        db.Dispose();
                }
            };

            Get["/history/months/{MONTHS:int}"] = _ =>
            {
                CloudPanelContext db = null;
                try
                {
                    int m = _.MONTHS;

                    logger.DebugFormat("Getting history chart for the last {0} months", m);
                    db = new CloudPanelContext(Settings.ConnectionString);
                    db.Database.Connection.Open();

                    logger.DebugFormat("Getting user statistics...");
                    string[] months = new string[m];

                    int?[] userCount = new int?[m];
                    int?[] exchCount = new int?[m];
                    int?[] citrixCount = new int?[m];

                    DateTime yearAgo = DateTime.Now.AddMonths(-m + 1);
                    for (int i = 0; i < m; i++)
                    {
                        months[i] = yearAgo.ToString("MMM yy");
                        userCount[i] = GetUserCount(ref db, yearAgo);

                        if (Settings.ExchangeModule)
                            exchCount[i] = GetMailboxCount(ref db, yearAgo);

                        if (Settings.CitrixModule)
                            citrixCount[i] = GetCitrixCount(ref db, yearAgo);

                        yearAgo = yearAgo.AddMonths(1);
                    }

                    return Response.AsJson(new
                    {
                        months = months,
                        users = userCount,
                        exchange = exchCount,
                        citrix = citrixCount
                    });
                }
                catch (Exception ex)
                {
                    logger.ErrorFormat("Error getting history overview for {0} month(s): {1}", _.MONTHS, ex.ToString());
                    throw;
                }
                finally
                {
                    if (db != null)
                        db.Dispose();
                }
            };
        }

        private int? GetUserCount(ref CloudPanelContext db, DateTime pickDate)
        {
            int value = 0;

            if (pickDate.Month == DateTime.Now.Month && pickDate.Year == DateTime.Now.Year)
                value = (from u in db.Users
                         select u.ID).Count();
            else
                value = (from d in db.Stats_UserCount
                         where d.StatDate.Month == pickDate.Month
                         where d.StatDate.Year == pickDate.Year
                         orderby d.StatDate descending
                         select d.UserCount).FirstOrDefault();

            if (value == 0)
                return null;
            else
                return value;
        }

        private int? GetMailboxCount(ref CloudPanelContext db, DateTime pickDate)
        {
            int value = 0;

            if (pickDate.Month == DateTime.Now.Month && pickDate.Year == DateTime.Now.Year)
                value = (from u in db.Users
                         where u.MailboxPlan > 0
                         select u.ID).Count();
            else
                value = (from d in db.Stats_ExchCount
                         where d.StatDate.Month == pickDate.Month
                         where d.StatDate.Year == pickDate.Year
                         orderby d.StatDate descending
                         select d.UserCount).FirstOrDefault();

            if (value == 0)
                return null;
            else
                return value;
        }

        private int? GetCitrixCount(ref CloudPanelContext db, DateTime pickDate)
        {
            int value = 0;

            if (pickDate.Month == DateTime.Now.Month && pickDate.Year == DateTime.Now.Year)
                value = (from u in db.UserPlansCitrix
                         select u.UserID).Distinct().Count();
            else
                value = (from d in db.Stats_CitrixCount
                         where d.StatDate.Month == pickDate.Month
                         where d.StatDate.Year == pickDate.Year
                         orderby d.StatDate descending
                         select d.UserCount).FirstOrDefault();

            if (value == 0)
                return null;
            else
                return value;
        }
    }
}
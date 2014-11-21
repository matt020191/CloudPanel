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
    public class ChartsModule : NancyModule
    {
        private static readonly ILog logger = log4net.LogManager.GetLogger(typeof(ChartsModule));

        public ChartsModule() : base("/charts")
        {
            //this.RequiresAuthentication();
            //this.RequiresAnyClaim(new[] { "SuperAdmin" });

            Get["/customers/top/{X:int}"] = _ =>
            {
                #region Get top X customers based on the amount of users
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
                #endregion
            };

            Get["/history/months/{MONTHS:int}"] = _ =>
            {
                #region Get the history of growth for the area chart
                CloudPanelContext db = null;
                try
                {
                    int m = _.MONTHS;

                    logger.DebugFormat("Getting history chart for the last {0} months", m);
                    db = new CloudPanelContext(Settings.ConnectionString);
                    db.Database.Connection.Open();

                    // Variables
                    string[] months = new string[m];
                    int?[] userCount = new int?[m];
                    int?[] exchCount = new int?[m];
                    int?[] citrixCount = new int?[m];

                    // Get company codes
                    List<string> companyCodes = null;
                    
                    // Get the company code the user belongs to if they are a company admin
                    if (this.Context.IsSuperAdmin())
                    {
                        // Gets all the companies company codes in the database
                        companyCodes = (from d in db.Companies where !d.IsReseller select d.CompanyCode).ToList();
                    }
                    else if (this.Context.IsResellerAdmin())
                    {
                        // Only get companies that belong to the current reseller
                        string userCompanyCode = this.Context.GetCompanyCodeMembership();
                        var resellerCode = (from d in db.Companies where d.CompanyCode == userCompanyCode select d.ResellerCode).FirstOrDefault();
                        companyCodes = (from d in db.Companies where !d.IsReseller where d.ResellerCode == resellerCode select d.CompanyCode).ToList();
                    }

                    logger.DebugFormat("Getting user statistics...");
                    DateTime yearAgo = DateTime.Now.AddMonths(-m + 1);
                    for (int i = 0; i < m; i++)
                    {
                        // Get the month name
                        months[i] = yearAgo.ToString("MMM yy");

                        // Get the user counts
                        userCount[i] = GetUserCount(ref db, yearAgo, companyCodes);

                        if (Settings.ExchangeModule)
                            exchCount[i] = GetMailboxCount(ref db, yearAgo, companyCodes);

                        if (Settings.CitrixModule)
                            citrixCount[i] = GetCitrixCount(ref db, yearAgo, companyCodes);

                        yearAgo = yearAgo.AddMonths(1);
                    }

                    return Negotiate.WithModel(new { months = months, users = userCount, exchange = exchCount, citrix = citrixCount });
                }
                catch (Exception ex)
                {
                    logger.ErrorFormat("Error getting history overview for {0} month(s): {1}", _.MONTHS, ex.ToString());
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

        private int? GetUserCount(ref CloudPanelContext db, DateTime pickDate, List<string> companyCodes)
        {
            int value = 0;

            foreach (var c in companyCodes)
            {
                if (!string.IsNullOrEmpty(c))
                {
                    if (pickDate.Month == DateTime.Now.Month && pickDate.Year == DateTime.Now.Year)
                        value = (from d in db.Users where d.CompanyCode == c select d.ID).Count();
                    else 
                        value = (from d in db.Statistics
                                 where d.Retrieved.Month == pickDate.Month
                                 where d.Retrieved.Year == pickDate.Year
                                 where d.CompanyCode == c
                                 orderby d.Retrieved descending
                                 select d.UserCount).FirstOrDefault();

                }
            }

            if (value == 0)
                return null;
            else
                return value;
        }

        private int? GetMailboxCount(ref CloudPanelContext db, DateTime pickDate, List<string> companyCodes)
        {
            int value = 0;

            foreach (var c in companyCodes)
            {
                if (!string.IsNullOrEmpty(c))
                {
                    if (pickDate.Month == DateTime.Now.Month && pickDate.Year == DateTime.Now.Year)
                        value = (from d in db.Users where d.CompanyCode == c where d.MailboxPlan > 0 select d.ID).Count();
                    else 
                        value = (from d in db.Statistics
                                 where d.Retrieved.Month == pickDate.Month
                                 where d.Retrieved.Year == pickDate.Year
                                 where d.CompanyCode == c
                                 orderby d.Retrieved descending
                                 select d.MailboxCount).FirstOrDefault();

                }
            }

            if (value == 0)
                return null;
            else
                return value;
        }

        private int? GetCitrixCount(ref CloudPanelContext db, DateTime pickDate, List<string> companyCodes)
        {
            int value = 0;

            foreach (var c in companyCodes)
            {
                if (!string.IsNullOrEmpty(c))
                {
                    int[] userIds = (from d in db.Users where d.CompanyCode == c select d.ID).ToArray();

                    if (pickDate.Month == DateTime.Now.Month && pickDate.Year == DateTime.Now.Year)
                        value = (from d in db.UserPlansCitrix where userIds.Contains(d.UserID) select d.UserID).Distinct().Count();
                    else
                        value = (from d in db.Statistics
                                 where d.Retrieved.Month == pickDate.Month
                                 where d.Retrieved.Year == pickDate.Year
                                 where d.CompanyCode == c
                                 orderby d.Retrieved descending
                                 select d.CitrixCount).FirstOrDefault();

                }
            }

            if (value == 0)
                return null;
            else
                return value;
        }
    }
}
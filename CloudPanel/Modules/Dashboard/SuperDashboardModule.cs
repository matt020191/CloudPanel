using CloudPanel.Base.Config;
using CloudPanel.Database.EntityFramework;
using log4net;
using Nancy;
using Nancy.Security;
using System;
using System.Data.Entity;
using System.Linq;

namespace CloudPanel.Modules.Dashboard
{
    public class SuperDashboardModule : NancyModule
    {
        private static readonly ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public SuperDashboardModule() : base("/dashboard")
        {
            this.RequiresClaims(new[] { "SuperAdmin" });

            Get["/"] = _ =>
                {
                    return View["Dashboard/dashboard_super.cshtml"];
                };

            Get["/all"] = _ =>
                {
                    #region Get dashboard data
                    CloudPanelContext db = null;

                    try
                    {
                        db = new CloudPanelContext(Settings.ConnectionString);
                        db.Database.Connection.Open();

                        logger.DebugFormat("Querying top resellers, companies, users, and mailboxes");
                        var resellers = db.Companies.Where(x => x.IsReseller).Count();
                        var companies = db.Companies.Where(x => !x.IsReseller).Count();
                        var users = db.Users.Count();
                        var mailboxes = db.Users.Where(x => x.MailboxPlan > 0).Count();

                        logger.DebugFormat("Adding all mailbox allocated data");
                        var mailboxAllocated = (from d in db.Users
                                                join size in db.Plans_ExchangeMailbox on d.MailboxPlan equals size.MailboxPlanID into plan
                                                from mailboxPlan in plan.DefaultIfEmpty()
                                                where d.MailboxPlan > 0
                                                select 
                                                    mailboxPlan == null ? 0 : 
                                                    mailboxPlan.MailboxSizeMB
                                               ).DefaultIfEmpty().Sum();

                        logger.DebugFormat("Adding all mailbox used data");
                        var mailboxUsed = (from d in db.Users
                                           join size in db.StatMailboxSize.OrderByDescending(x => x.Retrieved) on d.UserGuid equals size.UserGuid into stat
                                           from mailboxSize in stat.DefaultIfEmpty().Take(1)
                                           where d.MailboxPlan > 0
                                           select
                                                 mailboxSize == null ? 0 : mailboxSize.TotalItemSizeInBytes
                                          ).DefaultIfEmpty().Sum();

                        logger.DebugFormat("Formatting sizes {0} and {1} to readable format", mailboxAllocated, mailboxUsed);
                        string allocatedReadableSize = ByteSize.ByteSize.FromMegaBytes(mailboxAllocated).ToString("#.##");
                        string usedReadableSize = ByteSize.ByteSize.FromBytes(mailboxUsed).ToString("#.##");
                        return Negotiate.WithModel(new
                        {
                            resellers = resellers,
                            companies = companies,
                            users = users,
                            mailboxes = mailboxes,
                            mailboxAllocated = allocatedReadableSize,
                            mailboxUsed = usedReadableSize
                        });
                    }
                    catch (Exception ex)
                    {
                        logger.ErrorFormat("Error getting dashboard data: {0}", ex.ToString());
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

            Get["/history/{MONTHS:int}"] = _ =>
                {
                    #region Get history data
                    int months = _.MONTHS;
                    CloudPanelContext db = null;
                    try
                    {
                        db = new CloudPanelContext(Settings.ConnectionString);
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
                    catch (Exception ex)
                    {
                        logger.ErrorFormat("Error pulling history data: {0}", ex.Message);
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

            Get["/exchange/databases"] = _ =>
                {
                    using (CloudPanelContext db = new CloudPanelContext(Settings.ConnectionString))
                    {
                        var databases = (from d in db.StatMailboxDatabaseSizes
                                         where d.Retrieved == db.StatMailboxDatabaseSizes.Max(x => x.Retrieved)
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
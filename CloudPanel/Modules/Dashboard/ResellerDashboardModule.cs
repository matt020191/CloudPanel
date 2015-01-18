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
    public class ResellerDashboardModule : NancyModule
    {
        private static readonly ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public ResellerDashboardModule() : base("/dashboard")
        {
            this.RequiresClaims(new[] { "ResellerAdmin" });

            Get["/"] = _ =>
            {
                return View["Dashboard/dashboard_reseller.cshtml"];
            };

            Get["/all"] = _ =>
            {
                #region Get dashboard data
                CloudPanelContext db = null;

                try
                {
                    var resellerUser = Context.CurrentUser as AuthenticatedUser;

                    db = new CloudPanelContext(Settings.ConnectionString);
                    db.Database.Connection.Open();

                    logger.DebugFormat("Querying top companies, users, and mailboxes");
                    var companies = db.Companies.Where(x => 
                                     x.ResellerCode == resellerUser.ResellerCode &&
                                     x.IsReseller == false).ToList();

                    var users = from d in db.Users
                                join c in companies on d.CompanyCode equals c.CompanyName
                                select d;

                    var companyCount = companies.Count;
                    var userCount = users.Count();
                    var mailboxCount = users.Where(x => x.MailboxPlan > 0).Count();

                    logger.DebugFormat("Adding all mailbox allocated data");
                    var mailboxAllocated = (from d in users
                                            join size in db.Plans_ExchangeMailbox on d.MailboxPlan equals size.MailboxPlanID into plan
                                            from mailboxPlan in plan.DefaultIfEmpty()
                                            where d.MailboxPlan > 0
                                            select
                                                mailboxPlan == null ? 0 :
                                                mailboxPlan.MailboxSizeMB
                                           ).DefaultIfEmpty().Sum();

                    logger.DebugFormat("Adding all mailbox used data");
                    var mailboxUsed = (from d in users
                                       join size in db.StatMailboxSize on d.UserGuid equals size.UserGuid into stat
                                       from mailboxSize in stat.DefaultIfEmpty()
                                       where d.MailboxPlan > 0
                                       select
                                             mailboxSize == null ? 0 :
                                             mailboxSize.TotalItemSizeInBytes
                                      ).DefaultIfEmpty().Sum();

                    logger.DebugFormat("Formatting sizes {0} and {1} to readable format", mailboxAllocated, mailboxUsed);
                    string allocatedReadableSize = ByteSize.ByteSize.FromMegaBytes(mailboxAllocated).ToString("#.##");
                    string usedReadableSize = ByteSize.ByteSize.FromBytes(mailboxUsed).ToString("#.##");
                    return Negotiate.WithModel(new
                    {
                        companies = companyCount,
                        users = userCount,
                        mailboxes = mailboxCount,
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
                var resellerUser = Context.CurrentUser as AuthenticatedUser;

                int months = _.MONTHS;
                CloudPanelContext db = null;
                try
                {
                    db = new CloudPanelContext(Settings.ConnectionString);
                    var statistics = (from d in db.Statistics
                                      where d.Retrieved >= (DateTime)DbFunctions.AddMonths(DateTime.Now, -months)
                                      where d.ResellerCode == resellerUser.ResellerCode
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

            Get["/companies/{TOP:int}"] = _ =>
            {
                var resellerUser = Context.CurrentUser as AuthenticatedUser;

                int top = _.TOP;
                using (CloudPanelContext db = new CloudPanelContext(Settings.ConnectionString))
                {
                    var companies = (from d in db.Companies
                                     where d.ResellerCode == resellerUser.ResellerCode
                                     where d.IsReseller != true
                                     select new
                                     {
                                         CompanyCode = d.CompanyCode,
                                         CompanyName = d.CompanyName,
                                         UserCount = db.Users.Where(x => 
                                                            x.CompanyCode == d.CompanyName).Count(),
                                         MailboxCount = db.Users.Where(x => 
                                                               x.CompanyCode == d.CompanyCode &&
                                                               x.MailboxPlan > 0).Count()
                                     }).ToList().Take(top);

                    return Negotiate.WithModel(companies);
                }
            };
        }
    }
}
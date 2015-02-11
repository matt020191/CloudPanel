using CloudPanel.Base.Config;
using CloudPanel.Database.EntityFramework;
using log4net;
using Nancy;
using Nancy.Security;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace CloudPanel.Modules.Dashboard
{
    public class DashboardModule : NancyModule
    {
        private static readonly ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public DashboardModule() : base("/dashboard")
        {
            Get["/"] = _ =>
                {
                    this.RequiresAnyClaim(new[] { "SuperAdmin", "ResellerAdmin" });

                    var user = Context.CurrentUser as AuthenticatedUser;
                    if (user.HasClaim("SuperAdmin"))
                    {
                        return View["Dashboard/dashboard_super.cshtml"];
                    }
                    else if (user.HasClaim("ResellerAdmin"))
                    {
                        return View["Dashboard/dashboard_reseller.cshtml"];
                    }
                    else
                    {
                        return this.Response.AsRedirect("~/company/" + user.CompanyCode + "/overview");
                    }
                };

            Get["/admin/all"] = _ =>
                {
                    this.RequiresAnyClaim(new[] { "SuperAdmin" });

                    #region Get dashboard data
                    CloudPanelContext db = null;
                    try
                    {
                        var user = Context.CurrentUser as AuthenticatedUser;

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

            Get["/admin/history/{MONTHS:int}"] = _ =>
                {
                    this.RequiresAnyClaim(new[] { "SuperAdmin" });

                    #region Get history data
                    int months = _.MONTHS;
                    CloudPanelContext db = null;
                    try
                    {
                        db = new CloudPanelContext(Settings.ConnectionString);
                        var statistics = (from d in db.Statistics
                                          where d.Retrieved >= (DateTime)DbFunctions.AddMonths(DateTime.Now, -months)
                                          //group d by DbFunctions.TruncateTime(d.Retrieved) into g
                                          group d by d.Retrieved into g
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

            Get["/admin/exchange/databases"] = _ =>
                {
                    this.RequiresAnyClaim(new[] { "SuperAdmin" });

                    #region Get Exchange databases
                    using (CloudPanelContext db = new CloudPanelContext(Settings.ConnectionString))
                    {
                        var databases = (from d in db.StatMailboxDatabaseSizes
                                         where d.Retrieved == db.StatMailboxDatabaseSizes.Max(x => x.Retrieved)
                                         select d).ToList();

                        return Negotiate.WithModel(databases);
                    }
                    #endregion
                };

            Get["/admin/resellers/{TOP:int}"] = _ =>
                {
                    this.RequiresAnyClaim(new[] { "SuperAdmin" });

                    #region Get top resellers
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
                    #endregion
                };

            Get["/reseller/all"] = _ =>
                {
                    this.RequiresAnyClaim(new[] { "SuperAdmin", "ResellerAdmin" });

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

            Get["/reseller/history/{MONTHS:int}"] = _ =>
                {
                    this.RequiresAnyClaim(new[] { "SuperAdmin", "ResellerAdmin" });

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

            Get["/reseller/companies/{TOP:int}"] = _ =>
                {
                    this.RequiresAnyClaim(new[] { "SuperAdmin", "ResellerAdmin" });

                    #region Get tops companies
                    int top = _.TOP;
                    var resellerUser = Context.CurrentUser as AuthenticatedUser;
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
                    #endregion
                };
        }
    }
}
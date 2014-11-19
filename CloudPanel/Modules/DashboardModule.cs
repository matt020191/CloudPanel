using CloudPanel.Base.Charting;
using CloudPanel.Base.Config;
using CloudPanel.Base.Database.Models;
using CloudPanel.Base.Exchange;
using CloudPanel.Database.EntityFramework;
using CloudPanel.Exchange;
using log4net;
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
        private readonly ILog logger = LogManager.GetLogger(typeof(DashboardModule));

        public DashboardModule() : base("/dashboard")
        {
            this.RequiresAuthentication();

            Get["/", c => c.Request.Accept("text/html")] = _ =>
            {
                if (this.Context.IsSuperAdmin())
                    return View["Dashboard/dashboard_super.cshtml"];

                if (this.Context.IsResellerAdmin())
                    return View["Dashboard/dashboard_reseller.cshtml"];

                return View["Dashboard/dashboard_admin.cshtml"];
            };

            Get["/", c => (!c.Request.Accept("text/html") && c.IsSuperAdmin())] = _ =>
            {
                CloudPanelContext db = null;
                try
                {
                    db = new CloudPanelContext(Settings.ConnectionString);
                    db.Database.Connection.Open();

                    var companies = from d in db.Companies select d;
                    var users = from d in db.Users select d;

                    int totalResellers = 0, totalCompanies = 0, totalUsers = 0, totalMailboxes = 0;
                    totalResellers = (from d in companies where d.IsReseller select d.CompanyId).Count();
                    totalCompanies = (from d in companies where !d.IsReseller select d.CompanyId).Count();
                    totalUsers = users.Count();
                    totalMailboxes = (from d in users where d.MailboxPlan > 0 select d.ID).Count();

                    // Get all mailboxes
                    var sqlMailboxes = (from d in users
                                            join d1 in db.Plans_ExchangeMailbox on d.MailboxPlan equals d1.MailboxPlanID into d1tmp
                                            from data1 in d1tmp.DefaultIfEmpty()
                                            where d.MailboxPlan > 0
                                            select new
                                            {
                                                UserPrincipalName = d.UserPrincipalName,
                                                AdditionalMB = d.AdditionalMB == null ? 0 : d.AdditionalMB,
                                                PlanMB = data1.MailboxSizeMB
                                            }).ToList();

                    // Get all allocated mailbox space
                    long totalUsed = 0;
                    sqlMailboxes.ForEach(x =>
                    {
                        long total = 0;
                        total = (from d in db.SvcMailboxSizes
                                 where d.UserPrincipalName == x.UserPrincipalName
                                 orderby d.Retrieved descending
                                 select d.TotalItemSizeInBytes).FirstOrDefault();

                        totalUsed += total;
                    });

                    // Find latest mailbox size
                    long mailboxAllocated = 0;
                    sqlMailboxes.ForEach(x => mailboxAllocated += ((int)x.AdditionalMB + x.PlanMB));

                    // Convert MB to bytes
                    mailboxAllocated = (mailboxAllocated * 1024) * 1024;

                    return Negotiate.WithModel(new
                    {
                        TotalResellers = totalResellers,
                        TotalCompanies = totalCompanies,
                        TotalUsers = totalUsers,
                        TotalMailboxes = totalMailboxes,
                        TotalMailboxAllocated = CPStaticHelpers.FormatBytes(mailboxAllocated),
                        TotalMailboxUsed = CPStaticHelpers.FormatBytes(totalUsed)
                    });
                }
                catch (Exception ex)
                {
                    logger.ErrorFormat("Error pulling dashboard stats: {0}", ex.ToString());
                    return Negotiate.WithModel(new { error = ex.Message })
                                    .WithStatusCode(HttpStatusCode.InternalServerError);
                }
                finally
                {
                    if (db != null)
                        db.Dispose();
                }
            };

            Get["/exchange/databases"] = _ =>
                {
                    dynamic powershell = null;
                    CloudPanelContext db = null;
                    try
                    {
                        powershell = ExchPowershell.GetClass();

                        List<MailboxDatabase> databases = powershell.Get_MailboxDatabases();
                        if (databases != null)
                        {
                            db = new CloudPanelContext(Settings.ConnectionString);
                            foreach (var d in databases)
                            {
                                db.SvcMailboxDatabaseSizes.Add(new SvcMailboxDatabaseSizes()
                                    {
                                         DatabaseName = d.Identity,
                                         Server = d.Server,
                                         DatabaseSizeInBytes = d.DatabaseSizeInBytes,
                                         DatabaseSize = d.DatabaseSize,
                                         Retrieved = d.Retrieved
                                    });
                            }

                            db.SaveChanges();
                        }

                        return Negotiate.WithModel(new { databases = databases });
                    }
                    catch (Exception ex)
                    {
                        logger.ErrorFormat("Error getting exchange databases: {0}", ex.ToString());
                        return Negotiate.WithModel(new { error = ex.Message })
                                        .WithStatusCode(HttpStatusCode.InternalServerError);
                    }
                    finally
                    {
                        if (db != null)
                            db.Dispose();

                        if (powershell != null)
                            powershell.Dispose();
                    }
                };

            Get["/customers/top/{X:int}"] = _ =>
                {
                    CloudPanelContext db = null;
                    try
                    {
                        int top = _.X;
                        logger.DebugFormat("Getting top {0} customers", top);

                        db = new CloudPanelContext(Settings.ConnectionString);
                        db.Database.Connection.Open();

                        var topCustomers = (from d in db.Users
                                            group d by d.CompanyCode into grp
                                            select new TopXCustomers
                                            {
                                                CompanyCode = grp.Key,
                                                CompanyName = string.Empty,
                                                ResellerCode = string.Empty,
                                                TotalUsers = grp.Select(x => x.UserGuid).Distinct().Count()
                                            }).OrderByDescending(x => x.TotalUsers).Take(top).ToList();

                        logger.DebugFormat("Found a total of {0} companies", topCustomers.Count());
                        foreach (var data in topCustomers)
                        {
                            var company = (from d in db.Companies
                                           where d.CompanyCode == data.CompanyCode
                                           select d).FirstOrDefault();

                            if (company != null)
                            {
                                data.CompanyName = company.CompanyName;
                                data.ResellerCode = company.ResellerCode;
                            }
                        }

                        return Negotiate.WithModel(new { data = topCustomers });
                    }
                    catch (Exception ex)
                    {
                        logger.ErrorFormat("Error getting top X customers: {0}", ex.ToString());
                        return Negotiate.WithModel(new { error = ex.Message })
                                        .WithStatusCode(HttpStatusCode.InternalServerError);
                    }
                    finally
                    {
                        if (db != null)
                            db.Dispose();
                    }
                };

            Get["/customers/top/mailboxes/{X:int}"] = _ =>
            {
                CloudPanelContext db = null;
                try
                {
                    int top = _.X;
                    logger.DebugFormat("Getting top {0} customers mailboxes", top);

                    db = new CloudPanelContext(Settings.ConnectionString);
                    db.Database.Connection.Open();

                    var topMailboxes = (from d in db.Users
                                        where d.MailboxPlan > 0
                                        group d by d.CompanyCode into grp
                                        select new TopXCustomers
                                        {
                                            CompanyCode = grp.Key,
                                            CompanyName = string.Empty,
                                            ResellerCode = string.Empty,
                                            TotalUsers = grp.Select(x => x.UserGuid).Distinct().Count()
                                        }).OrderByDescending(x => x.TotalUsers).Take(top).ToList();

                    logger.DebugFormat("Found a total of {0} mailboxes", topMailboxes.Count());
                    foreach (var data in topMailboxes)
                    {
                        var company = (from d in db.Companies
                                       where d.CompanyCode == data.CompanyCode
                                       select d).FirstOrDefault();

                        if (company != null)
                        {
                            data.CompanyName = company.CompanyName;
                            data.ResellerCode = company.ResellerCode;
                        }
                    }

                    return Negotiate.WithModel(new { data = topMailboxes });
                }
                catch (Exception ex)
                {
                    logger.ErrorFormat("Error getting top X mailboxes: {0}", ex.ToString());
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
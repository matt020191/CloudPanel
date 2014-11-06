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

            Get["/exchange/databases"] = _ =>
                {
                    dynamic powershell = null;
                    try
                    {
                        powershell = ExchPowershell.GetClass();

                        List<MailboxDatabase> databases = powershell.Get_MailboxDatabases();
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
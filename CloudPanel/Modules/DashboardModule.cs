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

                else  if (this.Context.IsResellerAdmin())
                    return View["Dashboard/dashboard_reseller.cshtml"];

                else 
                    return View["Dashboard/dashboard_admin.cshtml"];
            };

            Get["/", c => (!c.Request.Accept("text/html") && c.IsSuperAdmin())] = _ =>
                {
                    #region Gets data if the user is a super admin
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

                        CloudPanel.Modules.StatModule.UpdateCounts();
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
                    #endregion
                };

            Get["/", c => (!c.Request.Accept("text/html") && c.IsResellerAdmin())] = _ =>
                {
                    #region Gets data if the user is a reseller admin
                    CloudPanelContext db = null;
                    try
                    {
                        db = new CloudPanelContext(Settings.ConnectionString);
                        db.Database.Connection.Open();

                        logger.DebugFormat("Getting company code for user");
                        string companyCode = this.Context.GetCompanyCodeMembership();

                        logger.DebugFormat("Company code was {0}. Getting the company from the database", companyCode);
                        var company = (from d in db.Companies where d.CompanyCode == companyCode select d).FirstOrDefault();

                        logger.DebugFormat("Getting all companies for reseller code {0}", company.ResellerCode);
                        string resellercode = company.ResellerCode;
                        var companies = (from d in db.Companies
                                         where d.ResellerCode == resellercode
                                         select d).ToList();

                        logger.DebugFormat("Parsing company codes");
                        string[] companyCodes = companies.Select(x => x.CompanyCode).ToArray();

                        logger.DebugFormat("Counting all the values...");
                        int totalCompanies = 0, totalUsers = 0, totalMailboxes = 0;
                        totalCompanies = companies.Count();
                        totalUsers = (from d in db.Users where companyCodes.Contains(d.CompanyCode) select d.ID).Count();
                        totalMailboxes = (from d in db.Users where companyCodes.Contains(d.CompanyCode) where d.MailboxPlan > 0 select d.ID).Count();

                        // Get all mailboxes
                        logger.DebugFormat("Getting all mailboxes...");
                        long totalUsed = 0, mailboxAllocated = 0;
                        if (Settings.ExchangeModule)
                        {
                            var sqlMailboxes = (from d in db.Users
                                                join d1 in db.Plans_ExchangeMailbox on d.MailboxPlan equals d1.MailboxPlanID into d1tmp
                                                from data1 in d1tmp.DefaultIfEmpty()
                                                where companyCodes.Contains(d.CompanyCode)
                                                where d.MailboxPlan > 0
                                                select new
                                                {
                                                    UserPrincipalName = d.UserPrincipalName,
                                                    AdditionalMB = d.AdditionalMB == null ? 0 : d.AdditionalMB,
                                                    PlanMB = data1.MailboxSizeMB
                                                }).ToList();

                            // Get all allocated mailbox space
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
                            sqlMailboxes.ForEach(x => mailboxAllocated += ((int)x.AdditionalMB + x.PlanMB));

                            // Convert MB to bytes
                            mailboxAllocated = (mailboxAllocated * 1024) * 1024;
                        }
                        else
                            logger.DebugFormat("Exchange module not enabled");

                        return Negotiate.WithModel(new
                        {
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
                    #endregion
                };

            Get["/exchange/databases"] = _ =>
                {
                    this.RequiresAnyClaim(new[] { "SuperAdmin" });

                    #region Gets the mailbox database sizes from Exchange
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

                                logger.DebugFormat("Processed database name {0}", d.Identity);
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
                    #endregion
                };

            Get["/customers/top/{X:int}"] = _ =>
                {
                    this.RequiresAnyClaim(new[] { "SuperAdmin", "ResellerAdmin" });

                    #region Gets the top X customers
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
                    #endregion
                };

            Get["/customers/top/mailboxes/{X:int}"] = _ =>
            {
                this.RequiresAnyClaim(new[] { "SuperAdmin", "ResellerAdmin" });

                #region Gets the top X mailboxes 
                CloudPanelContext db = null;
                try
                {
                    int top = _.X;
                    logger.DebugFormat("Getting top {0} customers mailboxes", top);

                    db = new CloudPanelContext(Settings.ConnectionString);
                    db.Database.Connection.Open();

                    logger.DebugFormat("Getting company code for user");
                    string companyCode = this.Context.GetCompanyCodeMembership();

                    logger.DebugFormat("Company code was {0}. Getting the company from the database", companyCode);
                    var company = (from d in db.Companies where d.CompanyCode == companyCode select d).FirstOrDefault();

                    var topMailboxes = new List<TopXCustomers>();
                    if (this.Context.IsSuperAdmin())
                    {
                        logger.DebugFormat("User requesting top mailboxes is a super admin");
                        topMailboxes = (from d in db.Users
                                        where d.MailboxPlan > 0
                                        group d by d.CompanyCode into grp
                                            select new TopXCustomers
                                            {
                                                CompanyCode = grp.Key,
                                                CompanyName = string.Empty,
                                                ResellerCode = string.Empty,
                                                TotalUsers = grp.Select(x => x.UserGuid).Distinct().Count()
                                            }).OrderByDescending(x => x.TotalUsers).Take(top).ToList();
                    }
                    else
                    {
                        //
                        // Is a reseller admin so only gather data for companies that beong to this reseller.
                        //
                        logger.DebugFormat("User requesting top mailboxes is a reseller admin. Getting all company codes");
                        var companyCodes = (from d in db.Companies where d.ResellerCode == company.ResellerCode select d.CompanyCode).ToList();

                        topMailboxes = (from d in db.Users
                                        where d.MailboxPlan > 0
                                        group d by d.CompanyCode into grp
                                        select new TopXCustomers
                                        {
                                            CompanyCode = grp.Key,
                                            CompanyName = string.Empty,
                                            ResellerCode = string.Empty,
                                            TotalUsers = grp.Select(x => x.UserGuid).Distinct().Count()
                                        }).OrderByDescending(x => x.TotalUsers)
                                            .Where(x => companyCodes.Contains(x.CompanyCode))
                                            .Take(top)
                                            .ToList();
                    }

                    logger.DebugFormat("Found a total of {0} mailboxes", topMailboxes.Count());
                    foreach (var data in topMailboxes)
                    {
                        var company1= (from d in db.Companies
                                       where d.CompanyCode == data.CompanyCode
                                       select d).FirstOrDefault();

                        if (company1 != null)
                        {
                            data.CompanyName = company1.CompanyName;
                            data.ResellerCode = company1.ResellerCode;
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
                #endregion
            };
        }
    }
}
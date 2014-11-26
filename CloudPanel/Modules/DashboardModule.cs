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

            Get["/", c => (!c.Request.Accept("text/html") && !c.IsSuperOrResellerAdmin())] = _ =>
                {
                    #region Gets data if the user is a super admin
                    CloudPanelContext db = null;
                    try
                    {
                        db = new CloudPanelContext(Settings.ConnectionString);
                        db.Database.Connection.Open();

                        var companyCode = this.Context.GetCompanyCodeMembership();
                        var users = (from d in db.Users where d.CompanyCode == companyCode select d).ToList();

                        int totalUsers = 0, totalAdmins = 0, totalDomains = 0, totalMailboxes = 0;
                        totalUsers = (from d in users select d).Count();
                        totalAdmins = (from d in users where d.IsCompanyAdmin == true select d).Count();
                        totalDomains = (from d in db.Domains where d.CompanyCode == companyCode select d).Count();
                        totalMailboxes = (from d in users where d.MailboxPlan > 0 select d).Count();

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
                            TotalUsers = totalUsers,
                            TotalAdmins = totalAdmins,
                            TotalDomains = totalDomains,
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
                        logger.DebugFormat("User getting the history graph is a super admin");
                        companyCodes = (from d in db.Companies
                                        where d.IsReseller != true
                                        select d.CompanyCode).ToList();
                    }
                    else if (this.Context.IsResellerAdmin())
                    {
                        logger.DebugFormat("User getting the history graph is a reseller admin");
                        var loggedInUserCompanyCode = this.Context.GetCompanyCodeMembership();
                        var loggedInUserResellerCode = (from d in db.Companies
                                                        where d.CompanyCode == loggedInUserCompanyCode
                                                        select d.ResellerCode).FirstOrDefault();
                        companyCodes = (from d in db.Companies
                                        where d.IsReseller != true
                                        where d.ResellerCode == loggedInUserResellerCode
                                        select d.CompanyCode).ToList();
                    }
                    else
                    {
                        logger.DebugFormat("User getting the history graph is a company admin");
                        companyCodes = new List<string>() { this.Context.GetCompanyCodeMembership() };
                    }

                    logger.DebugFormat("Getting user statistics...");
                    DateTime yearAgo = DateTime.Now.AddMonths(-m + 1);
                    for (int i = 0; i < m; i++)
                    {
                        // Get the month name
                        months[i] = yearAgo.ToString("MMM yy");
                        logger.DebugFormat("Current month {0}", months[i]);

                        // Get the user counts
                        userCount[i] = GetUserCount(ref db, yearAgo, companyCodes);
                        logger.DebugFormat("User count for month {0} is {1}", months[i], userCount[i]);

                        if (Settings.ExchangeModule)
                        {
                            exchCount[i] = GetMailboxCount(ref db, yearAgo, companyCodes);
                            logger.DebugFormat("Exchange count for month {0} is {1}", months[i], exchCount[i]);
                        }

                        if (Settings.CitrixModule)
                        {
                            citrixCount[i] = GetCitrixCount(ref db, yearAgo, companyCodes);
                            logger.DebugFormat("Citrix count for month {0} is {1}", months[i], citrixCount[i]);
                        }

                        yearAgo = yearAgo.AddMonths(1);
                    }

                    return Negotiate.WithModel(new { 
                        months = months, 
                        users = userCount, 
                        exchange = exchCount, 
                        citrix = citrixCount 
                    });
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

        /// <summary>
        /// Gets the user count for the specified month and company codes
        /// </summary>
        /// <param name="db"></param>
        /// <param name="pickDate"></param>
        /// <param name="companyCodes"></param>
        /// <returns></returns>
        private int? GetUserCount(ref CloudPanelContext db, DateTime pickDate, List<string> companyCodes)
        {
            int value = 0;

            foreach (var c in companyCodes)
            {
                if (!string.IsNullOrEmpty(c))
                {
                    logger.DebugFormat("Processing user count for {0}", c);
                    if (pickDate.Month == DateTime.Now.Month && pickDate.Year == DateTime.Now.Year)
                        value = (from d in db.Users 
                                 where d.CompanyCode == c 
                                 select d.ID).Count();
                    else
                        value = (from d in db.Statistics
                                 where d.Retrieved.Month == pickDate.Month
                                 where d.Retrieved.Year == pickDate.Year
                                 where d.CompanyCode == c
                                 orderby d.Retrieved descending
                                 select d.UserCount).FirstOrDefault();

                }
            }

            if (value > 0)
                return value;
            else
                return null;
        }

        /// <summary>
        /// Gets the mailbox count for the specified month and company codes
        /// </summary>
        /// <param name="db"></param>
        /// <param name="pickDate"></param>
        /// <param name="companyCodes"></param>
        /// <returns></returns>
        private int? GetMailboxCount(ref CloudPanelContext db, DateTime pickDate, List<string> companyCodes)
        {
            int value = 0;

            foreach (var c in companyCodes)
            {
                if (!string.IsNullOrEmpty(c))
                {
                    logger.DebugFormat("Processing Exchange count for {0}", c);
                    if (pickDate.Month == DateTime.Now.Month && pickDate.Year == DateTime.Now.Year)
                        value = (from d in db.Users 
                                 where d.CompanyCode == c 
                                 where d.MailboxPlan > 0 
                                 select d.ID).Count();
                    else
                        value = (from d in db.Statistics
                                 where d.Retrieved.Month == pickDate.Month
                                 where d.Retrieved.Year == pickDate.Year
                                 where d.CompanyCode == c
                                 orderby d.Retrieved descending
                                 select d.MailboxCount).FirstOrDefault();

                }
            }

            if (value > 0)
                return value;
            else
                return null;
        }

        /// <summary>
        /// Gets the citrix count for the specified month and company codes
        /// </summary>
        /// <param name="db"></param>
        /// <param name="pickDate"></param>
        /// <param name="companyCodes"></param>
        /// <returns></returns>
        private int? GetCitrixCount(ref CloudPanelContext db, DateTime pickDate, List<string> companyCodes)
        {
            int value = 0;

            foreach (var c in companyCodes)
            {
                if (!string.IsNullOrEmpty(c))
                {
                    logger.DebugFormat("Processing Citrix count for {0}", c);
                    int[] userIds = (from d in db.Users 
                                     where d.CompanyCode == c 
                                     select d.ID).ToArray();

                    if (pickDate.Month == DateTime.Now.Month && pickDate.Year == DateTime.Now.Year)
                        value = (from d in db.UserPlansCitrix 
                                 where userIds.Contains(d.UserID) 
                                 select d.UserID).Distinct().Count();
                    else
                        value = (from d in db.Statistics
                                 where d.Retrieved.Month == pickDate.Month
                                 where d.Retrieved.Year == pickDate.Year
                                 where d.CompanyCode == c
                                 orderby d.Retrieved descending
                                 select d.CitrixCount).FirstOrDefault();

                }
            }

            if (value > 0)
                return value;
            else
                return null;
        }
    }
}
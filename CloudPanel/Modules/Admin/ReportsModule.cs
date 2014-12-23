using CloudPanel.Base.Config;
using CloudPanel.Base.Reports;
using CloudPanel.Database.EntityFramework;
using log4net;
using Nancy;
using Nancy.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CloudPanel.Modules.Admin
{
    public class ReportsModule : NancyModule
    {
        private static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public ReportsModule() : base("/admin/reports")
        {
            this.RequiresClaims(new[] { "SuperAdmin" });
            
            Get["/", c => c.Request.Accept("text/html")] = _ =>
            {
                return View["Admin/reports.cshtml"];
            };

            Get["/overview/exchange", c => !c.Request.Accept("text/html")] = _ =>
            {
                #region Gets exchange overview data
                try
                {
                    var overviewData = ExchangeOverview();

                    int draw = 0, start = 0, length = 0, recordsTotal = overviewData.Count, recordsFiltered = overviewData.Count, orderColumn = 0;
                    string searchValue = "", orderColumnName = "";
                    bool isAscendingOrder = true;

                    if (Request.Query.draw.HasValue)
                    {
                        draw = Request.Query.draw;
                        start = Request.Query.start;
                        length = Request.Query.length;
                        orderColumn = Request.Query["order[0][column]"];
                        searchValue = Request.Query["search[value]"].HasValue ? Request.Query["search[value]"] : string.Empty;
                        isAscendingOrder = Request.Query["order[0][dir]"] == "asc" ? true : false;
                        orderColumnName = Request.Query["columns[" + orderColumn + "][data]"];

                        // See if we are using dataTables to search
                        logger.DebugFormat("Search value was {0}", searchValue);
                        if (!string.IsNullOrEmpty(searchValue))
                        {
                            overviewData = (from d in overviewData
                                            where d.CompanyName.IndexOf(searchValue, StringComparison.InvariantCultureIgnoreCase) != -1 ||
                                                  d.CompanyCode.IndexOf(searchValue, StringComparison.InvariantCultureIgnoreCase) != -1 ||
                                                  d.MailboxPlans.Any(x => x.MailboxPlanName.IndexOf(searchValue, StringComparison.InvariantCultureIgnoreCase) != -1)
                                            select d).ToList();

                            recordsFiltered = overviewData.Count;
                            logger.DebugFormat("Total records filtered was {0}", recordsFiltered);
                        }

                        if (isAscendingOrder)
                            overviewData = overviewData.OrderBy(x => x.GetType()
                                                    .GetProperty(orderColumnName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance).GetValue(x, null))
                                                    .Skip(start)
                                                    .Take(length > 0 ? length : overviewData.Count)
                                                    .ToList();
                        else
                            overviewData = overviewData.OrderByDescending(x => x.GetType()
                                                    .GetProperty(orderColumnName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance).GetValue(x, null))
                                                    .Skip(start)
                                                    .Take(length > 0 ? length : overviewData.Count)
                                                    .ToList();
                    }

                    // Get total costs and mailbox plans
                    Dictionary<string, int> mailboxPlans = new Dictionary<string, int>();
                    decimal totalCost = 0, totalPrice = 0, totalProfit = 0;
                    overviewData.ForEach(x =>
                       {
                           totalCost = decimal.Add(totalCost, x.MailboxCost);
                           totalPrice = decimal.Add(totalPrice, x.MailboxPrice);
                           totalProfit = decimal.Add(totalProfit, x.Profit);

                           if (x.MailboxPlans != null)
                           {
                               x.MailboxPlans.ForEach(m =>
                                   {
                                       if (mailboxPlans.ContainsKey(m.MailboxPlanName))
                                       {
                                           int value = 0;
                                           mailboxPlans.TryGetValue(m.MailboxPlanName, out value);

                                           value += m.UserCount;
                                           mailboxPlans[m.MailboxPlanName] = value;
                                       }
                                       else
                                           mailboxPlans.Add(m.MailboxPlanName, m.UserCount);
                                   });
                           }
                       });

                    return Negotiate.WithModel(new
                    {
                        draw = draw,
                        recordsTotal = recordsTotal,
                        recordsFiltered = recordsFiltered,
                        data = overviewData,
                        totalCost = totalCost,
                        totalPrice = totalPrice,
                        totalProfit = totalProfit,
                        mailboxPlans = mailboxPlans
                    }).WithStatusCode(HttpStatusCode.OK);
                }
                catch (Exception ex)
                {
                    logger.ErrorFormat("Error getting exchange overview reprot: {0}", ex.ToString());
                    return Negotiate.WithModel(new { error = ex.Message })
                                    .WithStatusCode(HttpStatusCode.InternalServerError);
                }
                #endregion
            };
        }

        public static List<ExchangeOverview> ExchangeOverview()
        {
            CloudPanelContext db = null;
            try
            {
                db = new CloudPanelContext(Settings.ConnectionString);
                db.Database.Connection.Open();

                logger.DebugFormat("Retrieving all Exchange plans from the database");
                var mailboxPlans = (from d in db.Plans_ExchangeMailbox
                                    orderby d.MailboxPlanName
                                    select d).ToList();

                logger.DebugFormat("Retrieving all Archive plans from the database");
                var archivePlans = (from d in db.Plans_ExchangeArchiving
                                    orderby d.DisplayName
                                    select d).ToList();

                logger.DebugFormat("Get all companies from the database");
                var companies = (from d in db.Companies
                                 orderby d.CompanyName
                                 select d).ToList();

                var exchangeOverview = new List<ExchangeOverview>();
                logger.DebugFormat("Looping through the companies to gather the information");
                foreach (var c in companies)
                {
                    var data = new ExchangeOverview();
                    data.CompanyCode = c.CompanyCode;
                    data.CompanyName = c.CompanyName;

                    logger.DebugFormat("Gathering users for {0}", c.CompanyCode);
                    var companyUsers = (from d in db.Users where d.CompanyCode == c.CompanyCode select d).ToList();

                    logger.DebugFormat("Counting data for {0}", c.CompanyCode);
                    data.UserCount = companyUsers.Count;
                    data.MailboxCount = companyUsers.Where(x => x.MailboxPlan > 0).Count();
                    data.ArchiveCount = companyUsers.Where(x => x.ArchivePlan > 0).Count();

                    logger.DebugFormat("Counting Exchange mailbox plan info for {0}", c.CompanyCode);
                    data.MailboxPlans = new List<Base.Database.Models.Plans_ExchangeMailbox>();
                    data.ArchivePlans = new List<Base.Database.Models.Plans_ExchangeArchiving>();

                    foreach (var u in companyUsers.Where(x => x.MailboxPlan > 0))
                    {
                        logger.DebugFormat("Getting mailbox plan for {0}", u.UserPrincipalName);
                        var plan = (from d in mailboxPlans
                                    where d.MailboxPlanID == u.MailboxPlan
                                    select d).FirstOrDefault();

                        logger.DebugFormat("Checking for a custom price for plan {0} and company {1}", plan.MailboxPlanName, u.CompanyCode);
                        var customPrice = (from d in db.PriceOverride
                                           where d.CompanyCode == u.CompanyCode
                                           where d.PlanID == plan.MailboxPlanID
                                           where d.Product == "Exchange"
                                           select d).FirstOrDefault();
                        if (customPrice != null)
                            plan.CustomPrice = customPrice.Price;

                        if (plan == null)
                            logger.WarnFormat("User {0} appears to have an invalid plan ID: {1}", u.UserPrincipalName, u.MailboxPlan);
                        else
                        {
                            var existingPlan = data.MailboxPlans.Where(x => x.MailboxPlanID == plan.MailboxPlanID).FirstOrDefault();
                            if (existingPlan != null)
                                existingPlan.UserCount++;
                            else
                            {
                                var addPlan = plan;
                                addPlan.UserCount = 1;
                                data.MailboxPlans.Add(addPlan);
                            }
                        }
                    }

                    // Add to our list
                    exchangeOverview.Add(data);
                }

                return exchangeOverview;
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error getting the Exchange overview: {0}", ex.ToString());
                return null;
            }
            finally
            {
                if (db != null)
                    db.Dispose();
            }
        }
    }
}
using CloudPanel.Base.Config;
using CloudPanel.Base.Reports;
using CloudPanel.Database.EntityFramework;
using log4net;
using Nancy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CloudPanel.Modules.Admin
{
    public class ReportsModule : NancyModule
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(ReportsModule));

        public ReportsModule() : base("/admin/reports")
        {
            Get["/", c => c.Request.Accept("text/html")] = _ =>
            {
                return View["Admin/reports.cshtml"];
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
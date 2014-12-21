using CloudPanel.Base.Config;
using CloudPanel.Base.Database.Models;
using CloudPanel.Database.EntityFramework;
using log4net;
using Nancy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CloudPanel.Modules
{
    public class StatModule : NancyModule
    {
        private static readonly ILog logger = LogManager.GetLogger("Default");

        public StatModule() : base("/stats")
        {

        }

        public static void UpdateCounts()
        {
            CloudPanelContext db = null;
            try
            {
                db = new CloudPanelContext(Settings.ConnectionString);
                db.Database.Connection.Open();

                // Get all companies
                var companies = (from d in db.Companies where !d.IsReseller 
                                 select new { ResellerCode = d.ResellerCode, CompanyCode = d.CompanyCode }).ToList();

                // Get all user id's
                var users = (from d in db.Users select d).ToList();

                // Use same DateTime on all values
                DateTime now = DateTime.Now;

                // Loop through all companies getting statistics
                logger.DebugFormat("Found a total of {0} companies for updating counts", companies.Count);
                foreach (var c in companies)
                {
                    logger.DebugFormat("Processing company {0} statistics", c.CompanyCode);

                    int[] userIds = (from d in users where d.CompanyCode == c.CompanyCode select d.ID).ToArray();

                    var newStat = new Statistics();
                    newStat.UserCount = (from d in db.Users where d.CompanyCode == c.CompanyCode select d.ID).Count();
                    newStat.MailboxCount = (from d in db.Users where d.CompanyCode == c.CompanyCode where d.MailboxPlan > 0 select d.ID).Count();
                    newStat.CitrixCount = (from d in db.UserPlansCitrix
                                           where userIds.Contains(d.UserID)
                                           select d.UserID).Distinct().Count();

                    newStat.ResellerCode = c.ResellerCode;
                    newStat.CompanyCode = c.CompanyCode;
                    newStat.Retrieved = now;

                    db.Statistics.Add(newStat);

                    logger.DebugFormat("Added statistics for company {0}", c.CompanyCode);
                }

                db.SaveChanges();
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error updating counts: {0}", ex.ToString());
            }
            finally
            {
                if (db != null)
                    db.Dispose();
            }
        }
    }
}
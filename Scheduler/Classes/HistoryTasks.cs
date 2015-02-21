using CloudPanel.Base.Config;
using CloudPanel.Base.Models.Database;
using CloudPanel.Database.EntityFramework;
using log4net;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Timers;
using PropertyConfig = Scheduler.Properties.Settings;

namespace Scheduler.Classes
{
    public class HistoryTasks
    {
        private static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static CPTimer _companyStats = null;

        public static void StartTimers()
        {
            //  Company statistics 
            int companyStatsMin = PropertyConfig.Default.History_Statistics;
            logger.DebugFormat("Starting company statistics history timer with {0}min interval", companyStatsMin);
            _companyStats = new CPTimer("HistoryStatistics");
            _companyStats.Interval = companyStatsMin > 0 ? new TimeSpan(0, companyStatsMin, 0).TotalMilliseconds : new TimeSpan(0, 1440, 0).TotalMilliseconds;
            _companyStats.Elapsed += Timer_Elapsed;
            _companyStats.Start();

            if (companyStatsMin == 0)
                Timer_Elapsed(_companyStats, null);
            else if (companyStatsMin == -1)
                _companyStats.Stop();
        }

        static void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            CPTimer senderTimer = sender as CPTimer;
            logger.DebugFormat("Timer {0} elapsed. Executing...", senderTimer.Name);

            if (senderTimer.Name == "HistoryStatistics")
            {
                _companyStats.Stop();
                UpdateCompanyStatistics();
                _companyStats.Start();
            }
        }

        public static void UpdateCompanyStatistics()
        {
            CloudPanelContext db = null;
            try
            {
                db = new CloudPanelContext(Settings.ConnectionString);
                db.Database.Connection.Open();

                // Get all companies
                var companies = (from d in db.Companies
                                 where !d.IsReseller
                                 select new
                                 {
                                     ResellerCode = d.ResellerCode,
                                     CompanyCode = d.CompanyCode
                                 }).ToList();

                // Use same DateTime on all values
                DateTime now = DateTime.Now;

                // Loop through all companies getting statistics
                logger.DebugFormat("Found a total of {0} companies for updating counts", companies.Count);
                List<Statistics> statistics = new List<Statistics>();
                foreach (var c in companies)
                {
                    logger.DebugFormat("Processing company {0} statistics", c.CompanyCode);

                    var users = db.Users
                                  .Include(x => x.CitrixDesktopGroups)
                                  .Where(x => x.CompanyCode == c.CompanyCode)
                                  .ToList();

                    var newStat = new Statistics();
                    newStat.UserCount = users.Count;
                    newStat.MailboxCount = users.Where(x => x.MailboxPlan > 0).Count();
                    newStat.CitrixCount = users.Where(x => x.CitrixDesktopGroups.Count > 0).Count();
                    newStat.ResellerCode = c.ResellerCode;
                    newStat.CompanyCode = c.CompanyCode;
                    newStat.Retrieved = now;

                    logger.DebugFormat("Added statistics for company {0}", c.CompanyCode);
                    statistics.Add(newStat);
                }

                db.Statistics.AddRange(statistics);
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

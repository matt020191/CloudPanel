using CloudPanel.Base.Config;
using CloudPanel.Base.Models.Database;
using CloudPanel.Base.Exchange;
using CloudPanel.Database.EntityFramework;
using CloudPanel.Exchange;
using log4net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Data.Entity;
using System.Timers;
using PropertyConfig = Scheduler.Properties.Settings;
using Scheduler.Classes;

namespace Scheduler
{
    public class ExchangeTasks
    {
        private static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static CPTimer _mailboxSizesTimer = null;
        public static CPTimer _databaseSizesTimer = null;
        public static CPTimer _messageLogCountTimer = null;

        public static void StartTimers()
        {
            // Mailbox sizes timer
            int mailboxSizesMin = PropertyConfig.Default.Exchange_RetrieveMailboxSizes;
            logger.DebugFormat("Starting mailbox sizes timer with {0}min interval", mailboxSizesMin);
            _mailboxSizesTimer = new CPTimer("MailboxSizes");
            _mailboxSizesTimer.Interval = mailboxSizesMin > 0 ? new TimeSpan(0, mailboxSizesMin, 0).TotalMilliseconds : new TimeSpan(0, 720, 0).TotalMilliseconds;
            _mailboxSizesTimer.Elapsed += Timer_Elapsed;
            _mailboxSizesTimer.Start();

            if (mailboxSizesMin == 0)
                Timer_Elapsed(_mailboxSizesTimer, null);
            else if (mailboxSizesMin == -1)
                _mailboxSizesTimer.Stop();

            // Mailbox database sizes timer
            int databaseSizesMin = PropertyConfig.Default.Exchange_RetrieveDatabaseSizes;
            logger.DebugFormat("Starting mailbox database sizes timer with {0}min interval", databaseSizesMin);
            _databaseSizesTimer = new CPTimer("DatabaseSizes");
            _databaseSizesTimer.Interval = databaseSizesMin > 0 ? new TimeSpan(0, databaseSizesMin, 0).TotalMilliseconds : new TimeSpan(0, 720, 0).TotalMilliseconds;
            _databaseSizesTimer.Elapsed += Timer_Elapsed;
            _databaseSizesTimer.Start();

            if (databaseSizesMin == 0)
                Timer_Elapsed(_databaseSizesTimer, null);
            else if (databaseSizesMin == -1)
                _databaseSizesTimer.Stop();

            // Message log count timer
            int messageLogMin = PropertyConfig.Default.Exchange_RetrieveMessageCounts;
            logger.DebugFormat("Starting message log counter timer with {0}min interval", messageLogMin);
            _messageLogCountTimer = new CPTimer("MessageLogCount");
            _messageLogCountTimer.Interval = messageLogMin > 0 ? new TimeSpan(0, messageLogMin, 0).TotalMilliseconds : new TimeSpan(24, 0, 0).TotalMilliseconds;
            _messageLogCountTimer.Elapsed += Timer_Elapsed;
            _messageLogCountTimer.Start();

            if (messageLogMin == 0)
                Timer_Elapsed(_messageLogCountTimer, null);
            else if (messageLogMin == -1)
                _messageLogCountTimer.Stop();
        }

        static void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            CPTimer senderTimer = sender as CPTimer;
            logger.DebugFormat("Timer {0} elapsed. Executing...", senderTimer.Name);

            if (senderTimer.Name == "MailboxSizes")
            {
                _mailboxSizesTimer.Stop();
                GetMailboxSizes();
                _mailboxSizesTimer.Start();
            }
            else if (senderTimer.Name == "DatabaseSizes")
            {
                _databaseSizesTimer.Stop();
                GetMailboxDatabaseSizes();
                _databaseSizesTimer.Start();
            }
            else if (senderTimer.Name == "MessageLogCount")
            {
                _messageLogCountTimer.Stop();
                GetTotalMessageLogs();
                _messageLogCountTimer.Start();
            }
        }

        /// <summary>
        /// Gets the mailbox sizes and archive sizes from Exchange and stores in the database
        /// </summary>
        static void GetMailboxSizes()
        {
            dynamic powershell = null;
            CloudPanelContext db = null;
            try
            {
                db = new CloudPanelContext(Settings.ConnectionString);
                db.Database.Connection.Open();

                logger.DebugFormat("Getting all users with mailboxes from the database");
                var mailboxes = (from d in db.Users
                                 where d.MailboxPlan > 0
                                 select d).ToList();

                if (mailboxes == null)
                    EventLog.WriteEntry("CloudPanel Scheduler", "No mailboxes were found to retrieve the mailbox sizes.", EventLogEntryType.Warning);
                else
                {
                    logger.DebugFormat("Getting all mailboxes with archiving from the database");
                    var archives = (from d in mailboxes
                                    where d.ArchivePlan > 0
                                    select d).ToList();

                    logger.DebugFormat("Getting mailbox guids and archive guids from our list");
                    Guid[] mailboxGuids = (from d in mailboxes select d.UserGuid).ToArray();
                    Guid[] mailboxArchiveGuids = (from d in archives select d.UserGuid).ToArray();

                    logger.DebugFormat("Retrieving sizes from the database");
                    powershell = ExchPowershell.GetClass();
                    List<StatMailboxSizes> mbxSizes = powershell.Get_AllMailboxSizes(mailboxGuids);
                    List<StatMailboxArchiveSizes> mbxArchiveSizes = powershell.Get_AllMailboxArchiveSizes(mailboxArchiveGuids);

                    if (mbxSizes.Count > 0)
                    {
                        int successCount = 0, failedCount = 0;

                        mbxSizes.ForEach(x =>
                            {
                                try
                                {
                                    // UserPrincipalName is required and the calling method doesn't send that back. Need to add back from our list
                                    x.UserPrincipalName = (from d in mailboxes 
                                                           where d.UserGuid.Equals(x.UserGuid) 
                                                           select d.UserPrincipalName).Single();

                                    db.StatMailboxSize.Add(x);
                                    successCount += 1;
                                }
                                catch (Exception ex)
                                {
                                    logger.WarnFormat("Error adding {0} mailbox size to database: {1}", x.UserPrincipalName, ex.ToString());
                                    failedCount += 1;
                                }
                            });

                        logger.DebugFormat("Processed {0}SUCCESS / {1}FAILED", successCount, failedCount);
                    }


                    if (mbxArchiveSizes.Count > 0)
                    {
                        int successCount = 0;
                        int failedCount = 0;
                        mbxArchiveSizes.ForEach(x =>
                            {
                                try
                                {
                                    // UserPrincipalName is required and the calling method doesn't send that back. Need to add back from our list
                                    x.UserPrincipalName = (from d in mailboxes 
                                                           where d.UserGuid.Equals(x.UserGuid) 
                                                           select d.UserPrincipalName).First();

                                    db.StatMailboxArchiveSize.Add(x);
                                    successCount += 1;
                                }
                                catch (Exception ex)
                                {
                                    logger.WarnFormat("Error adding {0} mailbox archive size to database: {1}", x.UserPrincipalName, ex.ToString());
                                    failedCount += 1;
                                }
                            });

                        logger.DebugFormat("Processed {0}SUCCESS / {1}FAILED", successCount, failedCount);
                    }

                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry("CloudPanel Scheduler", string.Format("Error querying mailbox sizes: {0}", ex.ToString()) );
                logger.ErrorFormat("Error getting mailbox sizes: {0}", ex.ToString());
            }
            finally
            {
                if (db != null)
                    db.Dispose();

                if (powershell != null)
                    powershell.Dispose();
            }
        }

        /// <summary>
        /// Gets the mailbox database sizes from Exchange
        /// </summary>
        static void GetMailboxDatabaseSizes()
        {
            dynamic powershell = null;
            CloudPanelContext db = null;
            try
            {
                logger.DebugFormat("Querying Exchange database sizes");
                powershell = ExchPowershell.GetClass();

                List<MailboxDatabase> databases = powershell.Get_MailboxDatabases();
                if (databases != null)
                {
                    logger.DebugFormat("Found a total of {0} databases", databases.Count);

                    DateTime retrieved = DateTime.Now;
                    db = new CloudPanelContext(Settings.ConnectionString);
                    foreach (var d in databases)
                    {
                        logger.DebugFormat("Adding mailbox database {0} to the database", d.Identity);
                        db.StatMailboxDatabaseSizes.Add(new StatMailboxDatabaseSizes()
                        {
                            DatabaseName = d.Identity,
                            Server = d.Server,
                            DatabaseSizeInBytes = d.DatabaseSizeInBytes,
                            DatabaseSize = d.DatabaseSize,
                            Retrieved = retrieved
                        });
                    }

                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry("CloudPanel Scheduler", string.Format("Error querying mailbox database sizes: {0}", ex.ToString()));
                logger.ErrorFormat("Error getting mailbox database sizes: {0}", ex.ToString());
            }
            finally
            {
                if (db != null)
                    db.Dispose();

                if (powershell != null)
                    powershell.Dispose();
            }
        }

        /// <summary>
        /// Gets total sent and receives messages for the current day
        /// </summary>
        static void GetTotalMessageLogs()
        {
            dynamic powershell = null;
            CloudPanelContext db = null;
            try
            {
                logger.DebugFormat("Querying message logs");
                powershell = ExchPowershell.GetClass();

                DateTime start = DateTime.Now.AddHours(-24);
                DateTime end = DateTime.Now;

                logger.DebugFormat("Querying total sent logs");
                List<MessageTrackingLog> sentLogs = powershell.Get_TotalSentMessages(start, end);

                logger.DebugFormat("Querying total received logs");
                List<MessageTrackingLog> receivedLogs = powershell.Get_TotalReceivedMessages(start, end);

                logger.DebugFormat("Querying all mailbox users");
                db = new CloudPanelContext(Settings.ConnectionString);
                db.Database.Connection.Open();

                List<Users> allUsers = (from d in db.Users
                                        where d.MailboxPlan > 0
                                        select d).ToList();

                logger.DebugFormat("Start totalling the counts on a per user basis");
                List<StatMessageTrackingCount> messages = new List<StatMessageTrackingCount>();

                if (allUsers.Count > 0)
                {
                    allUsers.ForEach(x =>
                        {
                            var totalSentLogs = (from d in sentLogs
                                                 where d.Users.Contains(x.Email)
                                                 select d).ToList();

                            var totalReceivedLogs = (from d in receivedLogs
                                                     where d.Users.Contains(x.Email)
                                                     select d).ToList();

                            var newTrackingCount = new StatMessageTrackingCount();
                            newTrackingCount.UserID = x.ID;
                            newTrackingCount.Start = start;
                            newTrackingCount.End = end;
                            newTrackingCount.TotalSent = totalSentLogs.Count;
                            newTrackingCount.TotalReceived = totalReceivedLogs.Count;
                            newTrackingCount.TotalBytesSent = newTrackingCount.TotalSent > 0 ? totalSentLogs.Select(d => d.TotalBytes).Sum() : 0;
                            newTrackingCount.TotalBytesReceived = newTrackingCount.TotalReceived > 0 ? totalReceivedLogs.Select(d => d.TotalBytes).Sum() : 0;
                            messages.Add(newTrackingCount);
                        });

                    db.StatMessageTrackingCount.AddRange(messages);
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry("CloudPanel Scheduler", string.Format("Error querying sent messages: {0}", ex.ToString()));
                logger.ErrorFormat("Error getting message logs: {0}", ex.ToString());
            }
            finally
            {
                if (db != null)
                    db.Dispose();

                if (powershell != null)
                    powershell.Dispose();
            }
        }
    }
}

using CloudPanel.Base.Config;
using CloudPanel.Base.Database.Models;
using CloudPanel.Base.Exchange;
using CloudPanel.Database.EntityFramework;
using CloudPanel.Exchange;
using log4net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Scheduler
{
    public class ExchangeTasks
    {
        private static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Gets the mailbox sizes and archive sizes from Exchange and stores in the database
        /// </summary>
        public static void GetMailboxSizes()
        {
            dynamic powershell = null;
            CloudPanelContext db = null;
            try
            {
                db = new CloudPanelContext(Settings.ConnectionString);
                db.Database.Connection.Open();

                var mailboxes = (from d in db.Users
                                 where d.MailboxPlan > 0
                                 select d).ToList();

                if (mailboxes == null)
                    EventLog.WriteEntry("CloudPanel Scheduler", "No mailboxes were found to retrieve the mailbox sizes.", EventLogEntryType.Warning);
                else
                {
                    var archives = (from d in mailboxes
                                    where d.ArchivePlan > 0
                                    select d).ToList();

                    Guid[] mailboxGuids = (from d in mailboxes select d.UserGuid).ToArray();
                    Guid[] mailboxArchiveGuids = (from d in archives select d.UserGuid).ToArray();

                    powershell = ExchPowershell.GetClass();
                    List<StatMailboxSizes> mbxSizes = powershell.Get_AllMailboxSizes(mailboxGuids);
                    List<StatMailboxArchiveSizes> mbxArchiveSizes = powershell.Get_AllMailboxArchiveSizes(mailboxArchiveGuids);

                    if (mbxSizes.Count > 0)
                    {
                        mbxSizes.ForEach(x =>
                            {
                                try
                                {
                                    // UserPrincipalName is required and the calling method doesn't send that back. Need to add back from our list
                                    x.UserPrincipalName = (from d in mailboxes where d.UserGuid.Equals(x.UserGuid) select d.UserPrincipalName).First();

                                    db.StatMailboxSize.Add(x);
                                    db.SaveChanges();
                                }
                                catch (Exception ex)
                                {
                                    EventLog.WriteEntry("CloudPanel Scheduler", string.Format("Error adding {0} mailbox size to database: {1}", x.UserPrincipalName, ex.ToString()) );
                                }
                            });
                    }


                    if (mbxArchiveSizes.Count > 0)
                    {
                        mbxArchiveSizes.ForEach(x =>
                            {
                                try
                                {
                                    // UserPrincipalName is required and the calling method doesn't send that back. Need to add back from our list
                                    x.UserPrincipalName = (from d in mailboxes where d.UserGuid.Equals(x.UserGuid) select d.UserPrincipalName).First();

                                    db.StatMailboxArchiveSize.Add(x);
                                    db.SaveChanges();
                                }
                                catch (Exception ex)
                                {
                                    EventLog.WriteEntry("CloudPanel Scheduler", string.Format("Error adding {0} mailbox archive size to database: {1}", x.UserPrincipalName, ex.ToString()) );
                                }
                            });
                    }
                }
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry("CloudPanel Scheduler", string.Format("Error querying mailbox sizes: {0}", ex.ToString()) );
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
        public static void GetMailboxDatabaseSizes()
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
            }
            catch (Exception ex)
            {
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

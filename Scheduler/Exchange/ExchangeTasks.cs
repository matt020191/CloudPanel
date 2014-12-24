using CloudPanel.Base.Config;
using CloudPanel.Base.Database.Models;
using CloudPanel.Database.EntityFramework;
using CloudPanel.Exchange;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                logger.DebugFormat("Retrieving mailbox sizes");
                db = new CloudPanelContext(Settings.ConnectionString);
                db.Database.Connection.Open();

                var mailboxes = (from d in db.Users
                                 where d.MailboxPlan > 0
                                 select d).ToList();

                if (mailboxes == null)
                    logger.WarnFormat("No mailboxes were found to retrieve the mailbox sizes");
                else
                {
                    var archives = (from d in mailboxes
                                    where d.ArchivePlan > 0
                                    select d).ToList();

                    logger.DebugFormat("Gathering all the user Guids");
                    Guid[] mailboxGuids = (from d in mailboxes select d.UserGuid).ToArray();
                    Guid[] mailboxArchiveGuids = (from d in archives select d.UserGuid).ToArray();

                    logger.DebugFormat("Found a total of {0} mailboxes to query", mailboxGuids.Length);
                    powershell = ExchPowershell.GetClass();

                    logger.DebugFormat("Querying mailbox sizes");
                    List<StatMailboxSizes> mbxSizes = powershell.Get_AllMailboxSizes(mailboxGuids);

                    logger.DebugFormat("Querying archive sizes");
                    List<StatMailboxArchiveSizes> mbxArchiveSizes = powershell.Get_AllMailboxArchiveSizes(mailboxArchiveGuids);

                    logger.DebugFormat("Adding mailbox sizes to database");
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
                                    logger.ErrorFormat("Error adding {0} mailbox size to database: {1}", x.UserPrincipalName, ex.ToString());
                                }
                            });
                    }

                    logger.DebugFormat("Adding mailbox archive sizes to database");
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
                                    logger.ErrorFormat("Error adding {0} mailbox archive size to database: {1}", x.UserPrincipalName, ex.ToString());
                                }
                            });
                    }
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error querying mailbox sizes: {0}", ex.ToString());
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

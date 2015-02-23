using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using Nancy.Security;
using Nancy;
using CloudPanel.Database.EntityFramework;
using CloudPanel.Base.Config;
using CloudPanel.Base.Exchange;
using CloudPanel.Exchange;
using CloudPanel.Base.Models.Database;

namespace CloudPanel.Modules.Admin
{
    public class ExchangeModule : NancyModule
    {
        private readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public ExchangeModule() : base("/exchange")
        {
            Get["/databases"] = _ =>
                {
                    this.RequiresAnyClaim(new[] { "SuperAdmin" });

                    #region Gets the mailbox database sizes from SQL
                    CloudPanelContext db = null;
                    try
                    {
                        db = new CloudPanelContext(Settings.ConnectionString);

                        var databases = (from d in db.StatMailboxDatabaseSizes
                                         where d.Retrieved == db.StatMailboxDatabaseSizes.Max(x => x.Retrieved)
                                         select d).ToList();

                        return Negotiate.WithModel(databases);
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
                    }
                    #endregion
                };

            Get["/databases/now"] = _ =>
                {
                    this.RequiresAnyClaim(new[] { "SuperAdmin" });

                    #region Gets the mailbox database sizes from Exchange
                    dynamic powershell = null;
                    CloudPanelContext db = null;
                    try
                    {
                        powershell = ExchPowershell.GetClass();

                        List<MailboxDatabase> databases = powershell.Get_MailboxDatabases();
                        List<StatMailboxDatabaseSizes> returnDatabases = new List<StatMailboxDatabaseSizes>();
                        if (databases != null)
                        {
                            DateTime retrieved = DateTime.Now;
                            db = new CloudPanelContext(Settings.ConnectionString);
                            foreach (var d in databases)
                            {
                                var newDatabase = new StatMailboxDatabaseSizes();
                                newDatabase.DatabaseName = d.Identity;
                                newDatabase.Server = d.Server;
                                newDatabase.DatabaseSizeInBytes = d.DatabaseSizeInBytes;
                                newDatabase.DatabaseSize = d.DatabaseSize;
                                newDatabase.Retrieved = d.Retrieved;

                                returnDatabases.Add(newDatabase);
                                logger.DebugFormat("Processed database name {0}", d.Identity);
                            }

                            db.StatMailboxDatabaseSizes.AddRange(returnDatabases);
                            db.SaveChanges();
                        }

                        return Negotiate.WithModel(returnDatabases);
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

            Post["/messagelogs"] = _ =>
                {
                    this.RequiresAnyClaim(new[] { "SuperAdmin" });

                    #region Gets message logs from Exchange
                    dynamic powershell = null;
                    CloudPanelContext db = null;
                    try
                    {
                        logger.DebugFormat("Querying message logs");
                        if (!Request.Form.Start.HasValue)
                            throw new MissingFieldException("", "Start");

                        if (!Request.Form.End.HasValue)
                            throw new MissingFieldException("", "End");

                        powershell = ExchPowershell.GetClass();
                        DateTime start = DateTime.Parse(Request.Form.Start.Value);
                        DateTime end = DateTime.Parse(Request.Form.End.Value);

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

                        return Negotiate.WithModel(new { success = "Successfully updated message log count." })
                                        .WithStatusCode(HttpStatusCode.OK);
                    }
                    catch (Exception ex)
                    {
                        logger.ErrorFormat("Error getting message logs: {0}", ex.ToString());
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
        }
    }
}
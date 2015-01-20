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
using CloudPanel.Base.Database.Models;

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
                                     select new MailboxDatabase()
                                     {
                                         Identity = d.DatabaseName,
                                         Server = d.Server,
                                         DatabaseSize = d.DatabaseSize,
                                         DatabaseSizeInBytes = d.DatabaseSizeInBytes,
                                         Retrieved = d.Retrieved
                                     }).ToList();

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
                    if (databases != null)
                    {
                        db = new CloudPanelContext(Settings.ConnectionString);
                        foreach (var d in databases)
                        {
                            db.StatMailboxDatabaseSizes.Add(new StatMailboxDatabaseSizes()
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
        }
    }
}
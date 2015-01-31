using CloudPanel.Base.Config;
using CloudPanel.Base.Database.Models;
using CloudPanel.Database.EntityFramework;
using CloudPanel.Exchange;
using log4net;
using Nancy;
using Nancy.ModelBinding;
using Nancy.Security;
using Nancy.ViewEngines.Razor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CloudPanel.Modules.PlansModules
{
    public class ActiveSyncPlanModule : NancyModule
    {
        private static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public ActiveSyncPlanModule() : base("/plans/exchange/activesync")
        {
            this.RequiresClaims(new[] { "SuperAdmin" });

            Get["/new"] = _ =>
                {
                    var plan = new Plans_ExchangeActiveSync();
                    return View["Plans/activesync.cshtml", new { Plan = GetDefaultPlan() }];
                };

            Post["/new"] = _ =>
                {
                    #region Creates a new Activesync policy
                    CloudPanelContext db = null;
                    dynamic powershell = null;
                    try
                    {
                        // Bind to the forum
                        logger.DebugFormat("Binding Activesync plan to form...");
                        var plan = this.Bind<Plans_ExchangeActiveSync>();
                        plan.ExchangeName = plan.DisplayName;

                        // Update Exchange
                        logger.DebugFormat("Adding Activesync plan {0} in Exchange", plan.DisplayName);
                        powershell = ExchPowershell.GetClass();
                        powershell.New_ActiveSyncPolicy(plan);

                        // Update SQL
                        db = new CloudPanelContext(Settings.ConnectionString);
                        db.Plans_ExchangeActiveSync.Add(plan);
                        db.SaveChanges();

                        var blankPlan = new Plans_ExchangeActiveSync();
                        return View["Plans/activesync.cshtml", new { Plan = blankPlan, success = "Successfully created plan" }];
                    }
                    catch (Exception ex)
                    {
                        logger.ErrorFormat("Error updating Activesync policy {0}: {1}", _.ID, ex.ToString());
                        return Negotiate.WithModel(new { error = ex.Message })
                                        .WithView("Error/500.cshtml")
                                        .WithStatusCode(HttpStatusCode.InternalServerError);
                    }
                    finally
                    {
                        if (powershell != null)
                            powershell.Dispose();

                        if (db != null)
                            db.Dispose();
                    }
                    #endregion
                };

            Get["/{ID:int}"] = _ =>
                {
                    #region Gets a specific Activesync policy
                    CloudPanelContext db = null;
                    try
                    {
                        db = new CloudPanelContext(Settings.ConnectionString);

                        int id = _.ID;
                        var plan = (from d in db.Plans_ExchangeActiveSync
                                    where d.ASID == id
                                    select d).FirstOrDefault();

                        return View["Plans/activesync.cshtml", new { Plan = plan }];
                    }
                    catch (Exception ex)
                    {
                        logger.ErrorFormat("Error getting activesync plan {0}: {1}", _.ID, ex.ToString());
                        return Negotiate.WithModel(new { error = ex.Message })
                                        .WithView("Error/500.cshtml")
                                        .WithStatusCode(HttpStatusCode.InternalServerError);
                    }
                    finally
                    {
                        if (db != null)
                            db.Dispose();
                    }
                    #endregion
                };

            Post["/{ID:int}"] = _ =>
                {
                    #region Updates an existing Activesync policy
                    CloudPanelContext db = null;
                    dynamic powershell = null;
                    try
                    {
                        // Bind to the forum
                        logger.DebugFormat("Binding Activesync plan to form...");
                        var plan = this.Bind<Plans_ExchangeActiveSync>(new[] { "ASID", "IsEnterpriseCAL" });
                        plan.ExchangeName = plan.DisplayName;

                        // Get from SQL
                        int id = _.ID;
                        db = new CloudPanelContext(Settings.ConnectionString);
                        var sqlPlan = (from d in db.Plans_ExchangeActiveSync
                                       where d.ASID == id
                                       select d).FirstOrDefault();

                        // Update Exchange
                        logger.DebugFormat("Updating Activesync plan {0} in Exchange", id);
                        powershell = ExchPowershell.GetClass();
                        powershell.Set_ActiveSyncPolicy(sqlPlan.ExchangeName, plan);

                        // Update SQL
                        logger.DebugFormat("Combining the form values to the SQL values");
                        foreach (var p in plan.GetType().GetProperties())
                        {
                            if (!p.Name.Equals("ASID"))
                            {
                                object value = p.GetValue(plan, null);
                                sqlPlan.GetType()
                                       .GetProperty(p.Name)
                                       .SetValue(sqlPlan, value, null);
                            }
                        }

                        db.SaveChanges();
                        return View["Plans/activesync.cshtml", new { Plan = plan, success = "Successfully updated plan" }];
                    }
                    catch (Exception ex)
                    {
                        logger.ErrorFormat("Error updating Activesync policy {0}: {1}", _.ID, ex.ToString());
                        return Negotiate.WithModel(new { error = ex.Message })
                                        .WithView("Error/500.cshtml")
                                        .WithStatusCode(HttpStatusCode.InternalServerError);
                    }
                    finally
                    {
                        if (powershell != null)
                            powershell.Dispose();

                        if (db != null)
                            db.Dispose();
                    }
                    #endregion
                };

            Get["/all"] = _ =>
                {
                    #region Gets all Activesync plans from the database
                    dynamic powershell = null;
                    CloudPanelContext db = null;
                    try
                    {
                        powershell = ExchPowershell.GetClass();
                        List<Plans_ExchangeActiveSync> policies = powershell.Get_ActiveSyncPolicies();

                        if (policies != null)
                        {
                            logger.DebugFormat("Found a total of {0} activesync policies. Adding / updating the database", policies.Count);

                            db = new CloudPanelContext(Settings.ConnectionString);
                            db.Database.Connection.Open();

                            foreach (var p in policies)
                            {
                                logger.DebugFormat("Checking if activesync plan {0} is already in the database", p.DisplayName);
                                var sqlPolicy = (from d in db.Plans_ExchangeActiveSync
                                                 where d.DisplayName == p.DisplayName || d.ExchangeName == p.ExchangeName
                                                 select d).FirstOrDefault();

                                if (sqlPolicy == null)
                                {
                                    logger.DebugFormat("Activesync plan {0} was not in the database. Adding...", p.DisplayName);
                                    db.Plans_ExchangeActiveSync.Add(p);
                                }
                                else
                                {
                                    logger.DebugFormat("Activesync plan {0} was already in the database. Updating values from Exchange...", p.DisplayName);
                                    sqlPolicy.AllowNonProvisionableDevices = p.AllowNonProvisionableDevices;
                                    sqlPolicy.RefreshIntervalInHours = p.RefreshIntervalInHours;
                                    sqlPolicy.RequirePassword = p.RequirePassword;
                                    sqlPolicy.RequireAlphanumericPassword = p.RequireAlphanumericPassword;
                                    sqlPolicy.EnablePasswordRecovery = p.EnablePasswordRecovery;
                                    sqlPolicy.RequireEncryptionOnDevice = p.RequireEncryptionOnDevice;
                                    sqlPolicy.RequireEncryptionOnStorageCard = p.RequireEncryptionOnStorageCard;
                                    sqlPolicy.AllowSimplePassword = p.AllowSimplePassword;
                                    sqlPolicy.NumberOfFailedAttempted = p.NumberOfFailedAttempted;
                                    sqlPolicy.MinimumPasswordLength = p.MinimumPasswordLength;
                                    sqlPolicy.InactivityTimeoutInMinutes = p.InactivityTimeoutInMinutes;
                                    sqlPolicy.PasswordExpirationInDays = p.PasswordExpirationInDays;
                                    sqlPolicy.EnforcePasswordHistory = p.EnforcePasswordHistory;
                                    sqlPolicy.IncludePastCalendarItems = p.IncludePastCalendarItems;
                                    sqlPolicy.IncludePastEmailItems = p.IncludePastEmailItems;
                                    sqlPolicy.LimitEmailSizeInKB = p.LimitEmailSizeInKB;
                                    sqlPolicy.AllowDirectPushWhenRoaming = p.AllowDirectPushWhenRoaming;
                                    sqlPolicy.AllowHTMLEmail = p.AllowHTMLEmail;
                                    sqlPolicy.AllowAttachmentsDownload = p.AllowAttachmentsDownload;
                                    sqlPolicy.MaximumAttachmentSizeInKB = p.MaximumAttachmentSizeInKB;
                                    sqlPolicy.AllowRemovableStorage = p.AllowRemovableStorage;
                                    sqlPolicy.AllowCamera = p.AllowCamera;
                                    sqlPolicy.AllowWiFi = p.AllowWiFi;
                                    sqlPolicy.AllowInfrared = p.AllowInfrared;
                                    sqlPolicy.AllowInternetSharing = p.AllowInternetSharing;
                                    sqlPolicy.AllowRemoteDesktop = p.AllowRemoteDesktop;
                                    sqlPolicy.AllowDesktopSync = p.AllowDesktopSync;
                                    sqlPolicy.AllowBluetooth = p.AllowBluetooth;
                                    sqlPolicy.AllowBrowser = p.AllowBrowser;
                                    sqlPolicy.AllowConsumerMail = p.AllowConsumerMail;
                                    sqlPolicy.AllowTextMessaging = p.AllowTextMessaging;
                                    sqlPolicy.AllowUnsignedApplications = p.AllowUnsignedApplications;
                                    sqlPolicy.AllowUnsignedInstallationPackages = p.AllowUnsignedInstallationPackages;
                                }

                                db.SaveChanges();
                            }
                        }

                        return Negotiate.WithModel(new { policies = policies })
                                        .WithStatusCode(HttpStatusCode.OK);
                    }
                    catch (Exception ex)
                    {
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

        public static List<Plans_ExchangeActiveSync> GetPlans()
        {
            CloudPanelContext db = null;
            try
            {
                db = new CloudPanelContext(Settings.ConnectionString);
                
                var plans = (from d in db.Plans_ExchangeActiveSync
                             orderby d.DisplayName
                             select d).ToList();

                return plans;
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Failed to get activesync plans: {0}", ex.ToString());
                throw;
            }
            finally
            {
                if (db != null)
                    db.Dispose();
            }
        }

        public static IHtmlString GetPlansWithOptions(int? selectedValue)
        {
            var sb = new StringBuilder();

            CloudPanelContext db = null;
            try
            {
                db = new CloudPanelContext(Settings.ConnectionString);

                var plans = (from d in db.Plans_ExchangeActiveSync
                             orderby d.DisplayName
                             select d).ToList();

                foreach (var p in plans)
                {
                    logger.DebugFormat("Processing plan {0}", p.DisplayName);
                    string format = string.Format("<option value='{0}' {1}>{2}</option>",
                                                      p.ASID,
                                                      (selectedValue != null && selectedValue == p.ASID) ? "selected" : "",
                                                      p.DisplayName);

                    sb.AppendLine(format);
                }

                return new NonEncodedHtmlString(sb.ToString());
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Failed to get activesync plans with options: {0}", ex.ToString());
                throw;
            }
            finally
            {
                if (db != null)
                    db.Dispose();
            }
        }

        public static Plans_ExchangeActiveSync GetDefaultPlan()
        {
            return new Plans_ExchangeActiveSync()
            {
                AllowBluetooth = "Allow",
                AllowBrowser = true,
                AllowCamera = true,
                AllowConsumerMail = true,
                AllowDesktopSync = true,
                AllowHTMLEmail = true,
                AllowInternetSharing = true,
                AllowInfrared = true,
                AllowNonProvisionableDevices = true,
                AllowSimplePassword = false,
                AllowRemoteDesktop = true,
                RequireAlphanumericPassword = false,
                AllowRemovableStorage = true,
                AllowTextMessaging = true,
                AllowUnsignedApplications = true,
                AllowUnsignedInstallationPackages = true,
                AllowWiFi = true,
                AllowAttachmentsDownload = true,
                RequireEncryptionOnDevice = false,
                RequirePassword = false,
                PasswordExpirationInDays = 0,
                EnforcePasswordHistory = 0,
                RefreshIntervalInHours = 0,
                MaximumAttachmentSizeInKB = 0,
                NumberOfFailedAttempted = 4,
                InactivityTimeoutInMinutes = 15,
                MinimumPasswordLength = 4,
                MinDevicePasswordComplexCharacters = 0,
                RequireEncryptionOnStorageCard = false,
                EnablePasswordRecovery = false
            };
        }
    }
}
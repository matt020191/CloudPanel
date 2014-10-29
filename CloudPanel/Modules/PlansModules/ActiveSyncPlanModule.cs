using CloudPanel.Base.Config;
using CloudPanel.Base.Database.Models;
using CloudPanel.Database.EntityFramework;
using CloudPanel.Exchange;
using log4net;
using Nancy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CloudPanel.Modules.PlansModules
{
    public class ActiveSyncPlanModule : NancyModule
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(ActiveSyncPlanModule));

        public ActiveSyncPlanModule() : base("/plans/exchange/activesync")
        {
            Get["/new"] = _ =>
                {
                    return View["Plans/activesync.cshtml"];
                };

            Get["/all"] = _ =>
                {
                    dynamic powershell = null;
                    CloudPanelContext db = null;
                    try
                    {
                        powershell = ExchPowershell.GetClass();
                        List<Plans_ExchangeActiveSync> policies = powershell.Get_ActiveSyncPolicies();

                        if (policies != null)
                        {
                            logger.DebugFormat("Found a total of {0} activesync policies. Adding / updating the database");

                            db = new CloudPanelContext(Settings.ConnectionString);
                            db.Database.Connection.Open();
                            foreach (var p in policies)
                            {
                                var sqlPolicy = (from d in db.Plans_ExchangeActiveSync
                                                 where d.DisplayName == p.DisplayName
                                                 select d).FirstOrDefault();

                                if (sqlPolicy == null)
                                    db.Plans_ExchangeActiveSync.Add(p);
                                else
                                {
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
                };
        }
    }
}
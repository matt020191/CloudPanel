﻿using CloudPanel.Base.Config;
using CloudPanel.Base.Models.Database;
using CloudPanel.Database.EntityFramework;
using log4net;
using Nancy;
using Nancy.ViewEngines.Razor;
using Nancy.ModelBinding;
using Nancy.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CloudPanel.Exchange;
using CloudPanel.Code;
using Nancy.Responses.Negotiation;

namespace CloudPanel.Modules.CompanyModules.Exchange
{
    public class ActiveSyncModule : NancyModule
    {
        private static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public ActiveSyncModule() : base("/company/{CompanyCode}/exchange/activesync")
        {
            Get["/new"] = _ =>
            {
                this.RequiresValidatedClaims(c => ValidateClaims.AllowCompanyAdmin(Context.CurrentUser, _.CompanyCode, "vExchangeActiveSyncPlans"));

                var plan = new Plans_ExchangeActiveSync();
                return View["Company/Exchange/activesync.cshtml", new { Plan = GetDefaultPlan() }];
            };

            Post["/new"] = _ =>
            {
                this.RequiresValidatedClaims(c => ValidateClaims.AllowCompanyAdmin(Context.CurrentUser, _.CompanyCode, "cExchangeActiveSyncPlans"));

                #region Creates a new Activesync policy
                CloudPanelContext db = null;
                dynamic powershell = null;
                try
                {
                    // Checking if they are under the limit
                    if (CloudPanel.CPStaticHelpers.IsUnderLimit(_.CompanyCode, "activesync") != true)
                        throw new Exception("You are already at the limit of Activesync plans you can have.");

                    // Bind to the forum
                    logger.DebugFormat("Binding Activesync plan to form...");
                    var plan = this.Bind<Plans_ExchangeActiveSync>();
                    plan.CompanyCode = _.CompanyCode;
                    plan.ExchangeName = string.Format("{0}_{1}", _.CompanyCode, plan.DisplayName);

                    // Update Exchange
                    logger.DebugFormat("Adding Activesync plan {0} in Exchange", plan.DisplayName);
                    powershell = ExchPowershell.GetClass();
                    powershell.New_ActiveSyncPolicy(plan);

                    // Update SQL
                    db = new CloudPanelContext(Settings.ConnectionString);
                    db.Plans_ExchangeActiveSync.Add(plan);
                    db.SaveChanges();

                    var blankPlan = new Plans_ExchangeActiveSync();
                    return Negotiate.WithModel(new { Plan = blankPlan, success = "Successfully created plan" })
                                    .WithView("Company/Exchange/activesync.cshtml");
                }
                catch (Exception ex)
                {
                    logger.ErrorFormat("Error updating Activesync policy {0}: {1}", _.ID, ex.ToString());
                    return Negotiate.WithModel(new { error = ex.Message })
                                    .WithView("Error/500.cshtml");
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
                this.RequiresValidatedClaims(c => ValidateClaims.AllowCompanyAdmin(Context.CurrentUser, _.CompanyCode, "vExchangeActiveSyncPlans"));

                #region Gets a specific Activesync policy
                CloudPanelContext db = null;
                try
                {
                    db = new CloudPanelContext(Settings.ConnectionString);

                    int id = _.ID;
                    var plan = (from d in db.Plans_ExchangeActiveSync
                                where d.ASID == id
                                select d).FirstOrDefault();

                    return Negotiate.WithModel(new { Plan = plan })
                                    .WithView("Company/Exchange/activesync.cshtml");
                }
                catch (Exception ex)
                {
                    logger.ErrorFormat("Error getting activesync plan {0}: {1}", _.ID, ex.ToString());
                    return Negotiate.WithModel(new { error = ex.Message })
                                    .WithView("Error/500.cshtml");
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
                this.RequiresValidatedClaims(c => ValidateClaims.AllowCompanyAdmin(Context.CurrentUser, _.CompanyCode, "eExchangeActiveSyncPlans"));

                #region Updates an existing Activesync policy
                CloudPanelContext db = null;
                dynamic powershell = null;
                try
                {
                    // Bind to the forum
                    logger.DebugFormat("Binding Activesync plan to form...");
                    var plan = this.Bind<Plans_ExchangeActiveSync>(new[] { "ASID", "IsEnterpriseCAL" });
                    plan.CompanyCode = _.CompanyCode;
                    plan.ExchangeName = string.Format("{0}_{1}", _.CompanyCode, plan.DisplayName);

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
                        if (!p.Name.Equals("ASID") && !p.Name.Equals("ExchangeName"))
                        {
                            object value = p.GetValue(plan, null);
                            sqlPlan.GetType()
                                   .GetProperty(p.Name)
                                   .SetValue(sqlPlan, value, null);
                        }
                    }

                    db.SaveChanges();
                    return Negotiate.WithModel(new { Plan = plan, success = "Successfully updated plan" })
                                    .WithView("Company/Exchange/activesync.cshtml");
                }
                catch (Exception ex)
                {
                    logger.ErrorFormat("Error updating Activesync policy {0}: {1}", _.ID, ex.ToString());
                    return Negotiate.WithModel(new { error = ex.Message })
                                    .WithView("Error/500.cshtml");
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

            Delete["/{ID:int}"] = _ =>
            {
                this.RequiresValidatedClaims(c => ValidateClaims.AllowCompanyAdmin(Context.CurrentUser, _.CompanyCode, "dExchangeActiveSyncPlans"));

                #region Updates an existing Activesync policy
                CloudPanelContext db = null;
                dynamic powershell = null;
                try
                {
                    logger.DebugFormat("Getting ActiveSync plan from the database for deleting");
                    db = new CloudPanelContext(Settings.ConnectionString);
                    db.Database.Connection.Open();

                    int id = _.ID;
                    string companyCode = _.CompanyCode;
                    var plan = (from d in db.Plans_ExchangeActiveSync
                                where d.ASID == id
                                where d.CompanyCode == companyCode
                                select d).First();


                    logger.DebugFormat("Checking the database to make sure the plan is not in use");
                    var inUseCount = (from d in db.Users
                                      where d.ActiveSyncPlan == id
                                      select d).Count();

                    if (inUseCount > 0)
                        throw new Exception("This ActiveSync plan cannot be deleted because it is in use by " + inUseCount.ToString() + " user(s)");
                    else
                    {
                        logger.DebugFormat("Deleting ActiveSync plan {0} from Exchange", plan.ExchangeName);
                        powershell = ExchPowershell.GetClass();
                        powershell.Remove_ActiveSyncPolicy(plan.ExchangeName);

                        logger.DebugFormat("Deleted ActiveSync plan from Exchange. Now removing from SQL");
                        db.Plans_ExchangeActiveSync.Remove(plan);
                        db.SaveChanges();

                        string returnUrl = string.Format("/company/{0}/exchange/activesync/new", companyCode);
                        return Negotiate.WithModel(new { success = "Successfully removed ActiveSync policy" })
                                        .WithMediaRangeResponse("text/html", this.Response.AsRedirect(returnUrl));
                    }
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
        }

        public static List<Plans_ExchangeActiveSync> GetPlans(string companyCode)
        {
            CloudPanelContext db = null;
            try
            {
                db = new CloudPanelContext(Settings.ConnectionString);

                var plans = (from d in db.Plans_ExchangeActiveSync
                             where d.CompanyCode == companyCode
                             orderby d.DisplayName
                             select d).ToList();

                return plans;
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Failed to get activesync plans for {0}: {1}", companyCode, ex.ToString());
                throw;
            }
            finally
            {
                if (db != null)
                    db.Dispose();
            }
        }

        public static IHtmlString GetPlansWithOptions(string companyCode, int? selectedValue)
        {
            var sb = new StringBuilder();

            CloudPanelContext db = null;
            try
            {
                db = new CloudPanelContext(Settings.ConnectionString);

                var plans = (from d in db.Plans_ExchangeActiveSync
                             where d.CompanyCode == companyCode
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
                logger.ErrorFormat("Failed to get activesync plans with options for {0}: {1}", companyCode, ex.ToString());
                throw;
            }
            finally
            {
                if (db != null)
                    db.Dispose();
            }
        }

        /// <summary>
        /// Gets a list of activesync plans including the global ones that the company has access to
        /// </summary>
        /// <param name="companyCode"></param>
        /// <param name="selectedValue"></param>
        /// <returns></returns>
        public static IHtmlString GetAllPlansWithOptions(string companyCode, int? selectedValue)
        {
            var sb = new StringBuilder();

            CloudPanelContext db = null;
            try
            {
                logger.DebugFormat("Geting all plans for {0}", companyCode);
                db = new CloudPanelContext(Settings.ConnectionString);
                db.Database.Connection.Open();

                var plans = (from d in db.Plans_ExchangeActiveSync
                             where string.IsNullOrEmpty(d.CompanyCode) || d.CompanyCode.Equals(companyCode)
                             orderby d.DisplayName
                             select d).ToList();

                foreach (var p in plans)
                {
                    string append = string.Format("<option value='{0}' {1}>{2}</option>",
                                           p.ASID,
                                           (selectedValue.HasValue && p.ASID == selectedValue.Value) ? "selected" : "",
                                           p.DisplayName);
                    sb.AppendLine(append);
                }

                return new NonEncodedHtmlString(sb.ToString());
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error getting all plans with options for {0}: {1}", companyCode, ex.ToString());
                return new NonEncodedHtmlString(sb.ToString());
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
using CloudPanel.Base.Config;
using CloudPanel.Base.Models.Database;
using CloudPanel.Base.Security;
using CloudPanel.Code;
using CloudPanel.Database.EntityFramework;
using CloudPanel.Exchange;
using log4net;
using Nancy;
using Nancy.Security;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CloudPanel.Modules.Admin
{
    public class SettingsModule : NancyModule
    {
        private static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public SettingsModule() : base("/admin/settings")
        {
            Get["/"] = _ =>
                {
                    if (this.Context.IsLocalOrSuperAdmin())
                        return View["Admin/settings.cshtml"];
                    else
                        return this.Response.AsRedirect("~/login");
                };

            Get["/exchange/databases", c => c.Request.Accept("application/json")] = _ =>
                {
                    if (!this.Context.IsLocalOrSuperAdmin())
                        return this.Response.AsRedirect("~/login");
                    else
                    {
                        #region Gets Exchange databases from Exchange
                        dynamic powershell = null;
                        try
                        {
                            powershell = ExchPowershell.GetClass();
                            var databases = powershell.Get_MailboxDatabases();

                            return Negotiate.WithModel(new { databases = databases })
                                            .WithStatusCode(HttpStatusCode.OK);
                        }
                        catch (Exception ex)
                        {
                            logger.ErrorFormat("Error getting Exchange databases: {0}", ex.ToString());
                            return Negotiate.WithModel(new { error = ex.Message })
                                            .WithStatusCode(HttpStatusCode.InternalServerError);
                        }
                        finally
                        {
                            if (powershell != null)
                                powershell.Dispose();
                        }
                        #endregion
                    }
                };

            Post["/"] = _ =>
                {
                    if (!this.Context.IsLocalOrSuperAdmin())
                        return this.Response.AsRedirect("~/login");
                    else
                    {
                        #region Updates settings
                        try
                        {
                            // Settings
                            logger.DebugFormat("Saving settings");
                            StaticSettings.SaveSetting("Settings", "CompanyName", Request.Form.CompanyName.Value);
                            StaticSettings.SaveSetting("Settings", "HostingOU", Request.Form.HostingOU.Value);
                            StaticSettings.SaveSetting("Settings", "UsersOU", Request.Form.UsersOU.Value);
                            StaticSettings.SaveSetting("Settings", "PrimaryDC", Request.Form.PrimaryDC.Value);
                            StaticSettings.SaveSetting("Settings", "Username", Request.Form.Username.Value);
                            StaticSettings.SaveSetting("Settings", "Password", DataProtection.Encrypt(Request.Form.Password.Value, Request.Form.SaltKey.Value));
                            StaticSettings.SaveSetting("Settings", "SuperAdmins", Request.Form.SuperAdmins.Value);
                            StaticSettings.SaveSetting("Settings", "BillingAdmins", Request.Form.BillingAdmins.Value);
                            StaticSettings.SaveSetting("Settings", "SaltKey", Request.Form.SaltKey.Value);
                            StaticSettings.SaveSetting("Settings", "ResellersEnabled", Request.Form.ResellersEnabled.HasValue ? (string)Request.Form.ResellersEnabled.Value : "false");

                            // Exchange Settings
                            logger.DebugFormat("Saving Exchange settings");
                            StaticSettings.SaveSetting("Exchange", "ExchangeRoleAssignment", Request.Form.ExchangeRoleAssignment.Value);
                            StaticSettings.SaveSetting("Exchange", "ExchangeServer", Request.Form.ExchangeServer.Value);
                            StaticSettings.SaveSetting("Exchange", "ExchangePFServer", Request.Form.ExchangePFServer.Value);
                            StaticSettings.SaveSetting("Exchange", "ExchangePFEnabled", Request.Form.ExchangePFEnabled.HasValue ? (string)Request.Form.ExchangePFEnabled.Value : "false");
                            StaticSettings.SaveSetting("Exchange", "ExchangeArchivingEnabled", Request.Form.ExchangeArchivingEnabled.HasValue ? (string)Request.Form.ExchangeArchivingEnabled.Value : "false");
                            StaticSettings.SaveSetting("Exchange", "ExchangeVersion", Request.Form.ExchangeVersion.Value);
                            StaticSettings.SaveSetting("Exchange", "ExchangeConnection", Request.Form.ExchangeConnection.Value);
                            StaticSettings.SaveSetting("Exchange", "ExchangeMaxAliases", Request.Form.ExchangeMaxAliases.Value);
                            StaticSettings.SaveSetting("Exchange", "ExchangeDatabases", Request.Form.ExchangeDatabases.Value);
                            StaticSettings.SaveSetting("Exchange", "ExchangeGALName", Request.Form.ExchangeGALName.Value);
                            StaticSettings.SaveSetting("Exchange", "ExchangeABPName", Request.Form.ExchangeABPName.Value);
                            StaticSettings.SaveSetting("Exchange", "ExchangeOALName", Request.Form.ExchangeOALName.Value);
                            StaticSettings.SaveSetting("Exchange", "ExchangeUSERSName", Request.Form.ExchangeUSERSName.Value);
                            StaticSettings.SaveSetting("Exchange", "ExchangeCONTACTSName", Request.Form.ExchangeCONTACTSName.Value);
                            StaticSettings.SaveSetting("Exchange", "ExchangeROOMSName", Request.Form.ExchangeROOMSName.Value);
                            StaticSettings.SaveSetting("Exchange", "ExchangeGROUPSName", Request.Form.ExchangeGROUPSName.Value);
                            StaticSettings.SaveSetting("Exchange", "ExchangeGroupsOU", Request.Form.ExchangeGroupsOU.Value);
                            StaticSettings.SaveSetting("Exchange", "ExchangeContactsOU", Request.Form.ExchangeContactsOU.Value);
                            StaticSettings.SaveSetting("Exchange", "ExchangeRoomsOU", Request.Form.ExchangeRoomsOU.Value);
                            StaticSettings.SaveSetting("Exchange", "ExchangeResourceOU", Request.Form.ExchangeResourceOU.Value);
                            StaticSettings.SaveSetting("Exchange", "ExchangeOUName", "Exchange");

                            // Citrix Settings
                            StaticSettings.SaveSetting("Citrix", "ApplicationsOUName", Request.Form.ApplicationsOU.Value);
                            StaticSettings.SaveSetting("Citrix", "DeliveryController", Request.Form.CitrixDeliveryController.Value);

                            // Feature settings
                            logger.DebugFormat("Saving feature settings");
                            StaticSettings.SaveSetting("Modules", "ExchangeModule", Request.Form.ExchangeModule.HasValue ? (string)Request.Form.ExchangeModule.Value : "false");
                            StaticSettings.SaveSetting("Modules", "CitrixModule", Request.Form.CitrixModule.HasValue ? (string)Request.Form.CitrixModule.Value : "false");

                            // Notification settings
                            logger.DebugFormat("Saving notification settings");
                            StaticSettings.SaveSetting("Notifications", "Enabled", Request.Form.SNEnabled.HasValue ? (string)Request.Form.SNEnabled.Value : "false");
                            StaticSettings.SaveSetting("Notifications", "FromAddress", Request.Form.SNFrom.HasValue ? Request.Form.SNFrom.Value : "");
                            StaticSettings.SaveSetting("Notifications", "ToAddress", Request.Form.SNTo.HasValue ? Request.Form.SNTo.Value : "");
                            StaticSettings.SaveSetting("Notifications", "MailServer", Request.Form.SNServer.HasValue ? Request.Form.SNServer.Value : "");
                            StaticSettings.SaveSetting("Notifications", "MailPort", Request.Form.SNPort.HasValue ? Request.Form.SNPort.Value : "25");
                            StaticSettings.SaveSetting("Notifications", "MailUsername", Request.Form.SNUsername.HasValue ? Request.Form.SNUsername.Value : "");
                            StaticSettings.SaveSetting("Notifications", "MailPassword", Request.Form.SNPassword.HasValue ? DataProtection.Encrypt(Request.Form.SNPassword.Value, Request.Form.SaltKey.Value) : "");

                            StaticSettings.LoadSettings();

                            return Negotiate.WithModel(new { success = "Successfully saved settings" })
                                            .WithStatusCode(HttpStatusCode.OK);
                        }
                        catch (Exception ex)
                        {
                            return Negotiate.WithModel(new { error = ex.Message })
                                            .WithStatusCode(HttpStatusCode.InternalServerError);
                        }
                        #endregion
                    }
                };

            Post["/fix/exchange/contacts"] = _ =>
                {
                    #region Queries Exchange and gets the ObjectGuid for the contacts
                    dynamic powershell = null;
                    CloudPanelContext db = null;
                    try
                    {
                        powershell = ExchPowershell.GetClass();

                        db = new CloudPanelContext(Settings.ConnectionString);
                        db.Database.Connection.Open();

                        // Get all contacts from the database
                        var contacts = (from d in db.Contacts
                                        select d).ToList();

                        // Get all contacts from Exchange
                        List<Contacts> allContacts = powershell.Get_AllContacts();
                        if (allContacts != null && allContacts.Count > 0)
                        {
                            allContacts.ForEach(x =>
                                {
                                    logger.DebugFormat("Found contact {0} from Exchange. Searching sql...", x.DistinguishedName);
                                    var sqlContact = contacts.Where(c => c.DistinguishedName == x.DistinguishedName)
                                                             .FirstOrDefault();

                                    if (sqlContact != null)
                                        sqlContact.ObjectGuid = x.ObjectGuid;
                                    else
                                        logger.DebugFormat("Unable to find {0} in the database", x.DistinguishedName);
                                });
                        }

                        db.SaveChanges();
                        return Negotiate.WithModel(new { success = "Successfully updated all contacts in the database" })
                                        .WithStatusCode(HttpStatusCode.OK);
                    }
                    catch (Exception ex)
                    {
                        logger.ErrorFormat("Error getting contacts from Exchange: {0}", ex.ToString());
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

            Post["/fix/exchange/groups"] = _ =>
                {
                    #region Queries Exchange and gets the ObjectGuid for the groups
                    dynamic powershell = null;
                    CloudPanelContext db = null;
                    try
                    {
                        powershell = ExchPowershell.GetClass();

                        db = new CloudPanelContext(Settings.ConnectionString);
                        db.Database.Connection.Open();

                        // Get all groups from the database
                        var groups = (from d in db.DistributionGroups
                                        select d).ToList();

                        // Get all groups from Exchange
                        List<DistributionGroups> allGroups = powershell.Get_AllDistributionGroups();
                        if (allGroups != null && allGroups.Count > 0)
                        {
                            allGroups.ForEach(x =>
                            {
                                logger.DebugFormat("Found distribution group {0} from Exchange. Searching sql...", x.DistinguishedName);
                                var sqlGroup = groups.Where(c => c.DistinguishedName == x.DistinguishedName).FirstOrDefault();

                                if (sqlGroup != null)
                                    sqlGroup.ObjectGuid = x.ObjectGuid;
                                else
                                    logger.DebugFormat("Unable to find {0} in the database", x.DistinguishedName);
                            });
                        }

                        db.SaveChanges();
                        return Negotiate.WithModel(new { success = "Successfully updated all distribution groups in the database" })
                                        .WithStatusCode(HttpStatusCode.OK);
                    }
                    catch (Exception ex)
                    {
                        logger.ErrorFormat("Error getting distribution groups from Exchange: {0}", ex.ToString());
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
﻿using CloudPanel.Base.Security;
using CloudPanel.Code;
using CloudPanel.Exchange;
using log4net;
using Nancy;
using Nancy.Security;
using System;

namespace CloudPanel.Modules.Admin
{
    public class SettingsModule : NancyModule
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(SettingsModule));

        public SettingsModule() : base("/admin")
        {
            this.RequiresAuthentication();
            this.RequiresAnyClaim(new[] { "SuperAdmin" });

            Get["/settings"] = _ =>
                {
                    return View["Admin/settings.cshtml"];
                };

            Get["/settings/exchange/databases", c => c.Request.Accept("application/json")] = _ =>
                {
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
                        return Negotiate.WithModel(new { error = ex.Message })
                                        .WithStatusCode(HttpStatusCode.InternalServerError);
                    }
                    finally
                    {
                        if (powershell != null)
                            powershell.Dispose();
                    }
                };

            Post["/settings"] = _ =>
                {
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
                        StaticSettings.SaveSetting("Settings", "ResellersEnabled", Request.Form.ResellersEnabled.HasValue ? Request.Form.ResellersEnabled.Value : "false");

                        // Exchange Settings
                        logger.DebugFormat("Saving Exchange settings");
                        StaticSettings.SaveSetting("Exchange", "ExchangeRoleAssignment", Request.Form.ExchangeRoleAssignment.Value);
                        StaticSettings.SaveSetting("Exchange", "ExchangeServer", Request.Form.ExchangeServer.Value);
                        StaticSettings.SaveSetting("Exchange", "ExchangePFServer", Request.Form.ExchangePFServer.Value);
                        StaticSettings.SaveSetting("Exchange", "ExchangePFEnabled", Request.Form.ExchangePFEnabled.HasValue ? Request.Form.ExchangePFEnabled.Value : "false");
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

                        // Feature settings
                        logger.DebugFormat("Saving feature settings");
                        StaticSettings.SaveSetting("Modules", "ExchangeModule", Request.Form.ExchangeModule.HasValue ? Request.Form.ExchangeModule.Value : "false");
                        StaticSettings.SaveSetting("Modules", "CitrixModule", Request.Form.CitrixModule.HasValue ? Request.Form.CitrixModule.Value : "false");

                        StaticSettings.LoadSettings();

                        return Negotiate.WithModel(new { success = "Successfully saved settings"})
                                        .WithStatusCode(HttpStatusCode.OK);
                    }
                    catch (Exception ex)
                    {
                        return Negotiate.WithModel(new { error = ex.Message })
                                        .WithStatusCode(HttpStatusCode.InternalServerError);
                    }
                };
        }
    }
}
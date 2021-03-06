﻿using CloudPanel.Base.Config;
using CloudPanel.Database.EntityFramework;
using CloudPanel.Exchange;
using CloudPanel.Rollback;
using log4net;
using Nancy;
using Nancy.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CloudPanel.Modules.CompanyModules.Exchange
{
    public class EnableModule : NancyModule
    {
        private static readonly ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public EnableModule() : base("/company/{CompanyCode}/exchange/enable")
        {
            this.RequiresAuthentication();

            Get["/"] = _ =>
            {
                return View["Company/Exchange/enable.cshtml"];
            };

            Post["/"] = _ =>
            {
                CloudPanelContext db = null;
                ReverseActions reverse = null;
                dynamic powershell = null;
                try
                {
                    reverse = new ReverseActions();
                    db = new CloudPanelContext(Settings.ConnectionString);

                    string companyCode = _.CompanyCode;
                    logger.DebugFormat("Getting company {0} from the database", companyCode);
                    var company = (from d in db.Companies
                                   where !d.IsReseller
                                   where d.CompanyCode == companyCode
                                   select d).First();

                    logger.DebugFormat("Retrieved company {0} from the database. Initializing Exchange powershell class...", company.CompanyName);
                    powershell = ExchPowershell.GetClass();

                    // Used to rollback data
                    string name = "";

                    logger.Debug("Creating global address list");
                    name = powershell.New_GlobalAddressList(companyCode, company.DistinguishedName);
                    reverse.AddAction(Actions.CreateGlobalAddressList, name);
                    
                    logger.Debug("Creating All Users address list");
                    name = powershell.New_AddressListForUsers(companyCode, company.DistinguishedName);
                    reverse.AddAction(Actions.CreateAddressList, name);

                    logger.Debug("Creating All Groups address list");
                    name = powershell.New_AddressListForGroups(companyCode, company.DistinguishedName);
                    reverse.AddAction(Actions.CreateAddressList, name);

                    logger.Debug("Creating All Contacts address list");
                    name = powershell.New_AddressListForContacts(companyCode, company.DistinguishedName);
                    reverse.AddAction(Actions.CreateAddressList, name);

                    logger.Debug("Creating All Rooms address list");
                    name = powershell.New_AddressListForRooms(companyCode, company.DistinguishedName);
                    reverse.AddAction(Actions.CreateAddressList, name);

                    logger.Debug("Creating offline address book");
                    name = powershell.New_OfflineAddressBook(companyCode);
                    reverse.AddAction(Actions.CreateOfflineAddressBook, name);

                    logger.Debug("Creating address book policy");
                    name = powershell.New_AddressBookPolicy(companyCode);
                    reverse.AddAction(Actions.CreateAddressBookPolicy, name);

                    logger.DebugFormat("Updating database that Exchange is now enabled for company {0}", companyCode);
                    company.ExchEnabled = true;
                    db.SaveChanges();

                    logger.InfoFormat("Successfully enabled company {0} for Exchange", _.CompanyCode);
                    return Negotiate.WithModel(new { success = "Exchange has been enabled" });
                }
                catch (Exception ex)
                {
                    logger.ErrorFormat("Error enabling company for Exchange: {0}",  ex.ToString());

                    reverse.RollbackNow();
                    return Negotiate.WithModel(new { error = ex.Message })
                                    .WithStatusCode(HttpStatusCode.InternalServerError);
                }
                finally
                {
                    if (powershell != null)
                        powershell.Dispose();

                    if (db != null)
                        db.Dispose();
                }
            };
        }
    }
}
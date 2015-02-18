using CloudPanel.Base.Config;
using CloudPanel.Base.Enums;
using CloudPanel.Database.EntityFramework;
using CloudPanel.Exchange;
using log4net;
using Nancy;
using Nancy.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CloudPanel.Modules.CompanyModules.Exchange
{
    public class DisableModule : NancyModule
    {
        private static readonly ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public DisableModule() : base("/company/{CompanyCode}/exchange/disable")
        {
            this.RequiresAuthentication();

            Get["/"] = _ =>
            {
                return View["Company/Exchange/disable.cshtml"];
            };

            Post["/"] = _ =>
            {
                CloudPanelContext db = null;
                dynamic powershell = null;
                try
                {
                    db = new CloudPanelContext(Settings.ConnectionString);

                    logger.Debug("Validating that security codes match");
                    if (!Request.Form.SecurityCode.HasValue)
                        throw new Exception("The security code was not provided.");
                    if (!Request.Form.SecurityCodeEntered.HasValue)
                        throw new Exception("The security code entered was not provided");

                    string securityCode = Request.Form.SecurityCode;
                    string securityCodeEntered = Request.Form.SecurityCodeEntered;
                    if (securityCode != securityCodeEntered)
                        throw new Exception(string.Format("The security code {0} did not match {1}", securityCode, securityCodeEntered));
                    else
                    {
                        string companyCode = _.CompanyCode;

                        logger.Debug("Initializing Exchange powershell class...");
                        powershell = ExchPowershell.GetClass();

                        logger.DebugFormat("Removing all mailboxes for {0}", companyCode);
                        powershell.Remove_AllMailboxes(companyCode);

                        logger.DebugFormat("Settings all users in database to not have mailboxes");
                        var users = from d in db.Users where d.CompanyCode == companyCode where d.MailboxPlan > 0 select d;
                        if (users != null)
                        {
                            foreach (var u in users)
                            {
                                u.MailboxPlan = 0;
                            }

                            db.SaveChanges();
                        }

                        logger.DebugFormat("Removing all distribution groups for {0}", companyCode);
                        powershell.Remove_AllGroups(companyCode);

                        logger.DebugFormat("Removing all distributino groups from the database");
                        var groups = from d in db.DistributionGroups where d.CompanyCode == companyCode select d;
                        if (groups != null)
                            db.DistributionGroups.RemoveRange(groups);

                        logger.DebugFormat("Removing all mail contacts for {0}", companyCode);
                        powershell.Remove_AllContacts(companyCode);

                        logger.DebugFormat("Removing all mail contacts from the database");
                        var contacts = from d in db.Contacts where d.CompanyCode == companyCode select d;
                        if (contacts != null)
                            db.Contacts.RemoveRange(contacts);

                        logger.DebugFormat("Removing address book policy for {0} with format ({1})", companyCode, Settings.ExchangeABPName);
                        powershell.Remove_AddressBookPolicy(string.Format(Settings.ExchangeABPName, companyCode));

                        logger.DebugFormat("Removing offline address book for {0} with format ({1})", companyCode, Settings.ExchangeOALName);
                        powershell.Remove_OfflineAddressBook(string.Format(Settings.ExchangeOALName, companyCode));

                        logger.DebugFormat("Removing all rooms address list for {0} with format ({1})", companyCode, Settings.ExchangeROOMSName);
                        powershell.Remove_AddressList(string.Format(Settings.ExchangeROOMSName, companyCode));

                        logger.DebugFormat("Removing all contacts address list for {0} with format ({1})", companyCode, Settings.ExchangeCONTACTSName);
                        powershell.Remove_AddressList(string.Format(Settings.ExchangeCONTACTSName, companyCode));

                        logger.DebugFormat("Removing all groups address list for {0} with format ({1})", companyCode, Settings.ExchangeGROUPSName);
                        powershell.Remove_AddressList(string.Format(Settings.ExchangeGROUPSName, companyCode));

                        logger.DebugFormat("Removing all users address list for {0} with format ({1})", companyCode, Settings.ExchangeUSERSName);
                        powershell.Remove_AddressList(string.Format(Settings.ExchangeUSERSName, companyCode));

                        logger.DebugFormat("Removing global address list for {0} with format ({1})", companyCode, Settings.ExchangeGALName);
                        powershell.Remove_GlobalAddressList(string.Format(Settings.ExchangeGALName, companyCode));

                        logger.DebugFormat("Removing all accepted domains from Exchange");
                        var domains = (from d in db.Domains where d.CompanyCode == companyCode && d.IsAcceptedDomain select d).ToList();
                        if (domains != null)
                        {
                            domains.ForEach(x =>
                            {
                                powershell.Remove_AcceptedDomain(x);
                                x.IsAcceptedDomain = false;
                                x.DomainType = DomainType.Default;
                            });
                        }

                        logger.DebugFormat("Updating database that Exchange is now disabled for company {0}", companyCode);
                        var company = (from d in db.Companies
                                       where !d.IsReseller
                                       where d.CompanyCode == companyCode
                                       select d).First();

                        company.ExchEnabled = false;
                        db.SaveChanges();

                        logger.InfoFormat("Successfully disasbled company {0} from Exchange", _.CompanyCode);
                        return Negotiate.WithModel(new { success = "Exchange has been disabled" })
                                        .WithMediaRangeResponse("text/html", this.Response.AsRedirect("enable"));
                    }
                }
                catch (Exception ex)
                {
                    logger.ErrorFormat("Error disabling company {0} Exchange: {1}", _.CompanyCode, ex.ToString());
                    return Negotiate.WithModel(new { error = ex.Message })
                                    .WithView("Company/Exchange/disable.cshtml")
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
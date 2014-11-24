using CloudPanel.Base.Config;
using CloudPanel.Database.EntityFramework;
using log4net;
using Nancy;
using Nancy.Security;
using Nancy.ModelBinding;
using System;
using System.Linq;
using System.Reflection;
using CloudPanel.Base.Database.Models;
using CloudPanel.Exchange;
using CloudPanel.Rollback;
using System.Collections.Generic;
using CloudPanel.Code;

namespace CloudPanel.Modules
{
    public class ContactsModule : NancyModule
    {
        private static readonly ILog logger = log4net.LogManager.GetLogger(typeof(ContactsModule));

        public ContactsModule() : base("/company/{CompanyCode}/exchange/contacts")
        {
            this.RequiresAuthentication();

            Get["/"] = _ =>
            {
                this.RequiresValidatedClaims(c => ValidateClaims.AllowCompanyAdmin(Context.CurrentUser, _.CompanyCode, "vExchangeContacts"));

                #region Returns the contacts view with model or json data based on the request
                CloudPanelContext db = null;
                try
                {
                    db = new CloudPanelContext(Settings.ConnectionString);
                    db.Database.Connection.Open();

                    string companyCode = _.CompanyCode;
                    var contacts = (from d in db.Contacts
                                    where d.CompanyCode == companyCode
                                    orderby d.DisplayName
                                    select d).ToList();

                    int draw = 0, start = 0, length = 0, recordsTotal = contacts.Count, recordsFiltered = contacts.Count, orderColumn = 0;
                    string searchValue = "", orderColumnName = "";
                    bool isAscendingOrder = true;

                    // Check for dataTables and process the values
                    if (Request.Query.draw.HasValue)
                    {
                        draw = Request.Query.draw;
                        start = Request.Query.start;
                        length = Request.Query.length;
                        orderColumn = Request.Query["order[0][column]"];
                        searchValue = Request.Query["search[value]"].HasValue ? Request.Query["search[value]"] : string.Empty;
                        isAscendingOrder = Request.Query["order[0][dir]"] == "asc" ? true : false;
                        orderColumnName = Request.Query["columns[" + orderColumn + "][data]"];

                        // See if we are using dataTables to search
                        if (!string.IsNullOrEmpty(searchValue))
                        {
                            contacts = (from d in contacts
                                       where d.DisplayName.IndexOf(searchValue, StringComparison.InvariantCultureIgnoreCase) != -1 || 
                                             d.Email.IndexOf(searchValue, StringComparison.InvariantCultureIgnoreCase) != -1
                                       select d).ToList();
                            recordsFiltered = contacts.Count;
                        }

                        if (isAscendingOrder)
                            contacts = contacts.OrderBy(x => x.GetType()
                                                    .GetProperty(orderColumnName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance).GetValue(x, null))
                                                    .Skip(start)
                                                    .Take(length)
                                                    .ToList();
                        else
                            contacts = contacts.OrderByDescending(x => x.GetType()
                                                    .GetProperty(orderColumnName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance).GetValue(x, null))
                                                    .Skip(start)
                                                    .Take(length)
                                                    .ToList();
                    }

                    return Negotiate.WithModel(new { contacts = contacts } )
                                    .WithMediaRangeModel("application/json", new
                                    {
                                        draw = draw,
                                        recordsTotal = recordsTotal,
                                        recordsFiltered = recordsFiltered,
                                        data = contacts
                                    })
                                    .WithView("Company/Exchange/contacts.cshtml");
                }
                catch (Exception ex)
                {
                    return Negotiate.WithMediaRangeModel("application/json", new { error = ex.Message })
                                    .WithView("Company/Exchange/contacts.cshtml");
                }
                finally
                {
                    if (db != null)
                        db.Dispose();
                }
                #endregion
            };

            Post["/"] = _ =>
            {
                this.RequiresValidatedClaims(c => ValidateClaims.AllowCompanyAdmin(Context.CurrentUser, _.CompanyCode, "cExchangeContacts"));

                #region Creates a new contact

                CloudPanelContext db = null;
                ReverseActions reverse = null;
                dynamic powershell = null;
                try
                {
                    string companyCode = _.CompanyCode;

                    logger.DebugFormat("Creating a new contact for {0}", companyCode);
                    var newContact = this.Bind<Contacts>();
                    newContact.CompanyCode = companyCode;

                    // Validate
                    if (string.IsNullOrEmpty(newContact.DisplayName))
                        throw new Exception("You must provide the display name for the new contact");

                    if (string.IsNullOrEmpty(newContact.Email))
                        throw new Exception("You must provide the email for the new contact");

                    logger.DebugFormat("Make sure there isn't a contact with email {0} that already exists for this company", newContact.Email);
                    db = new CloudPanelContext(Settings.ConnectionString);
                    var alreadyExists = (from d in db.Contacts
                                         where d.CompanyCode == companyCode
                                         where d.Email == newContact.Email
                                         select d).Count();
                    if (alreadyExists > 0)
                        throw new Exception("Your company already has a contact created with this email address.");
                    else
                    {
                        logger.DebugFormat("Getting company {0} from the database", companyCode);
                        var company = (from d in db.Companies
                                       where !d.IsReseller
                                       where d.CompanyCode == companyCode
                                       select d).First();

                        logger.DebugFormat("Getting domains for company {0}", companyCode);
                        var domains = (from d in db.Domains
                                       where d.CompanyCode == companyCode
                                       select d).ToList();

                        logger.DebugFormat("Finding default domain...");
                        string defaultDomain = domains.Where(x => x.IsDefault) == null ? 
                                                       domains.First().Domain : domains.Where(x => x.IsDefault).First().Domain;

                        logger.DebugFormat("Creating contact {0} in Exchange", newContact.Email);
                        powershell = ExchPowershell.GetClass();

                        reverse = new ReverseActions();
                        var createdContact = powershell.New_MailContact(newContact, defaultDomain, Settings.OUPath(Settings.ExchangeContactsOU, company.DistinguishedName));
                        reverse.AddAction(Actions.CreateMailContact, createdContact.DistinguishedName);

                        logger.DebugFormat("Contact was created. Adding to database");
                        db.Contacts.Add(createdContact);
                        db.SaveChanges();

                        return Negotiate.WithModel(new { success = "Contact was created successfully" })
                                        .WithView("Company/Exchange/contacts.cshtml");
                    }
                }
                catch (Exception ex)
                {
                    logger.ErrorFormat("Error creating new contact for {0}: {1}", _.CompanyCode, ex.ToString());
                    reverse.RollbackNow();
                    return Negotiate.WithModel(new { error = ex.Message })
                                        .WithView("Company/Exchange/contacts.cshtml");
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

            Delete["/"] = _ =>
            {
                this.RequiresValidatedClaims(c => ValidateClaims.AllowCompanyAdmin(Context.CurrentUser, _.CompanyCode, "dExchangeContacts"));

                #region Deletes a contact

                CloudPanelContext db = null;
                dynamic powershell = null;
                try
                {
                    string companyCode = _.CompanyCode;

                    logger.DebugFormat("Preparing to delete contact... Getting ID..");
                    if (!Request.Form.ContactID.HasValue)
                        throw new Exception("Contact ID was not provided.");
                    else
                    {
                        int contactId = Request.Form.ContactID;

                        logger.DebugFormat("Getting contact {0} from the database", contactId);
                        db = new CloudPanelContext(Settings.ConnectionString);
                        var existingContact = (from d in db.Contacts
                                               where d.CompanyCode == companyCode
                                               where d.ID == contactId
                                               select d).First();

                        logger.DebugFormat("Found contact {0}. Removing from Exchange", existingContact.DistinguishedName);
                        powershell = ExchPowershell.GetClass();
                        powershell.Remove_MailContact(existingContact.DistinguishedName);

                        logger.DebugFormat("Contact {0} was removed from Exchange.. removing from database", existingContact.DistinguishedName);
                        db.Contacts.Remove(existingContact);
                        db.SaveChanges();

                        return Negotiate.WithModel(new { success = "Contact was deleted successfully" })
                                        .WithView("Company/Exchange/contacts.cshtml");
                    }
                }
                catch (Exception ex)
                {
                    logger.ErrorFormat("Error deleting contact for {0}: {1}", _.CompanyCode, ex.ToString());
                    return Negotiate.WithModel(new { error = ex.Message })
                                    .WithView("Company/Exchange/contacts.cshtml");
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

            Get["/{ContactID:int}"] = _ =>
            {
                this.RequiresValidatedClaims(c => ValidateClaims.AllowCompanyAdmin(Context.CurrentUser, _.CompanyCode, "vExchangeContacts"));

                #region Gets a specific contact from the database
                CloudPanelContext db = null;
                try
                {
                    logger.DebugFormat("Getting contact id {0}", _.ContactID);
                    db = new CloudPanelContext(Settings.ConnectionString);

                    string companyCode = _.CompanyCode;
                    int contactId = _.ContactID;
                    var contact = (from d in db.Contacts
                                   where d.CompanyCode == companyCode
                                   where d.ID == contactId
                                   select d).FirstOrDefault();

                    if (contact == null)
                        throw new Exception("Unable to find contact id {0} in the database.", _.ContactID);
                    else
                    {
                        return Negotiate.WithModel(new { contact = contact })
                                        .WithView("Company/Exchange/contacts_edit.cshtml");
                    }
                }
                catch (Exception ex)
                {
                    logger.ErrorFormat("Error getting contact {0}: {1}", _.ContactID, ex.ToString());
                    return Negotiate.WithModel(new { error = ex.Message })
                                    .WithView("Company/Exchange/contacts.cshtml");
                }
                finally
                {
                    if (db != null)
                        db.Dispose();
                }
                #endregion
            };

            Put["/{ContactID:int}"] = _ =>
            {
                this.RequiresValidatedClaims(c => ValidateClaims.AllowCompanyAdmin(Context.CurrentUser, _.CompanyCode, "eExchangeContacts"));

                #region Updates an existing contact in Exchange and the database

                CloudPanelContext db = null;
                dynamic powershell = null;
                try
                {
                    int contactId = _.ContactID;
                    string companyCode = _.CompanyCode;

                    logger.DebugFormat("Preparing to update contact {0} for company {1}... Validating required data", contactId);

                    if (!Request.Form.DisplayName.HasValue)
                        throw new Exception("Missing required field DisplayName");

                    logger.DebugFormat("Passed validation.. Getting contact from database");
                    db = new CloudPanelContext(Settings.ConnectionString);
                    var existingContact = (from d in db.Contacts
                                           where d.CompanyCode == companyCode
                                           where d.ID == contactId
                                           select d).FirstOrDefault();

                    if (existingContact == null)
                        throw new Exception("Unable to find contact in the database");
                    else
                    {
                        logger.DebugFormat("Found contact {0} in the database.", existingContact.DistinguishedName);
                        existingContact.DisplayName = Request.Form.DisplayName;
                        existingContact.Hidden = Request.Form.Hidden;

                        logger.DebugFormat("Updating contact {0} in Exchange", existingContact.DistinguishedName);
                        powershell = ExchPowershell.GetClass();
                        powershell.Update_MailContact(existingContact);

                        logger.DebugFormat("Saving changes to database");
                        db.SaveChanges();

                        string redirectUrl = string.Format("~/company/{0}/exchange/contacts", companyCode);
                        return Negotiate.WithModel(new { success = "Contact was updated successfully" })
                                        .WithMediaRangeResponse("text/html", this.Response.AsRedirect(redirectUrl));
                        
                    }
                }
                catch (Exception ex)
                {
                    logger.ErrorFormat("Error updating contact for {0}: {1}", _.CompanyCode, ex.ToString());

                    ViewBag.error = ex.ToString();
                    return Negotiate.WithModel(new { error = ex.Message })
                                    .WithStatusCode(HttpStatusCode.InternalServerError)
                                    .WithView("error.cshtml");
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

        public static List<Contacts> GetContacts(string companyCode)
        {
            CloudPanelContext db = null;
            try
            {
                logger.DebugFormat("Retrieving Exchange contacts for {0}", companyCode);
                db = new CloudPanelContext(Settings.ConnectionString);

                var contacts = (from d in db.Contacts
                                where d.CompanyCode == companyCode
                                orderby d.DisplayName
                                select d).ToList();

                return contacts;
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error retrieving Exchange contacts for {0}: {1}", companyCode, ex.ToString());
                throw;
            }
            finally
            {
                if (db != null)
                    db.Dispose();
            }
        }
    }
}
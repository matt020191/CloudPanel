using CloudPanel.Base.Config;
using CloudPanel.Base.Database.Models;
using CloudPanel.Code;
using CloudPanel.Database.EntityFramework;
using CloudPanel.Exchange;
using CloudPanel.Rollback;
using log4net;
using Nancy;
using Nancy.ModelBinding;
using Nancy.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CloudPanel.Modules
{
    public class ResourceModule : NancyModule
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(ResourceModule));

        public ResourceModule() : base("/company/{CompanyCode}/exchange/resourcemailboxes")
        {
            this.RequiresAuthentication();

            Get["/", c => c.Request.Accept("text/html")] = _ =>
            {
                this.RequiresValidatedClaims(c => ValidateClaims.AllowCompanyAdmin(Context.CurrentUser, _.CompanyCode, "vExchangeResources"));

                return View["Company/Exchange/resourcemailboxes.cshtml"];
            };

            Get["/", c => c.Request.Accept("application/json")] = _ =>
            {
                this.RequiresValidatedClaims(c => ValidateClaims.AllowCompanyAdmin(Context.CurrentUser, _.CompanyCode, "vExchangeResources"));

                #region Returns the groups view with model or json data based on the request
                CloudPanelContext db = null;
                try
                {
                    db = new CloudPanelContext(Settings.ConnectionString);
                    db.Database.Connection.Open();

                    string companyCode = _.CompanyCode;
                    var resourceMailboxes = (from d in db.ResourceMailboxes
                                             join p in db.Plans_ExchangeMailbox on d.MailboxPlan equals p.MailboxPlanID into p1
                                             from data1 in p1.DefaultIfEmpty()
                                             where d.CompanyCode == companyCode
                                             orderby d.DisplayName
                                             orderby d.ResourceType
                                             select new
                                             {
                                                 ResourceID = d.ResourceID,
                                                 DisplayName = d.DisplayName,
                                                 CompanyCode = d.CompanyCode,
                                                 UserPrincipalName = d.UserPrincipalName,
                                                 PrimarySmtpAddress = d.PrimarySmtpAddress,
                                                 ResourceType = d.ResourceType,
                                                 MailboxPlanID = d.MailboxPlan,
                                                 MailboxPlanName = data1.MailboxPlanName,
                                                 AdditionalMB = d.AdditionalMB
                                             }).ToList();

                    int draw = 0, start = 0, length = 0, recordsTotal = resourceMailboxes.Count, recordsFiltered = resourceMailboxes.Count, orderColumn = 0;
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
                            resourceMailboxes = (from d in resourceMailboxes
                                                 where d.DisplayName.IndexOf(searchValue, StringComparison.InvariantCultureIgnoreCase) != -1 || 
                                                       d.UserPrincipalName.IndexOf(searchValue, StringComparison.InvariantCultureIgnoreCase) != -1 || 
                                                       d.PrimarySmtpAddress.IndexOf(searchValue, StringComparison.InvariantCultureIgnoreCase) != -1 || 
                                                       d.ResourceType.IndexOf(searchValue, StringComparison.InvariantCultureIgnoreCase) != -1 || 
                                                       d.MailboxPlanName.IndexOf(searchValue, StringComparison.InvariantCultureIgnoreCase) != -1
                                                select d).ToList();
                            recordsFiltered = resourceMailboxes.Count;
                        }

                        if (isAscendingOrder)
                            resourceMailboxes = resourceMailboxes.OrderBy(x => x.GetType()
                                                    .GetProperty(orderColumnName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance).GetValue(x, null))
                                                    .Skip(start)
                                                    .Take(length)
                                                    .ToList();
                        else
                            resourceMailboxes = resourceMailboxes.OrderByDescending(x => x.GetType()
                                                    .GetProperty(orderColumnName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance).GetValue(x, null))
                                                    .Skip(start)
                                                    .Take(length)
                                                    .ToList();
                    }

                    return Negotiate.WithModel(resourceMailboxes)
                                    .WithMediaRangeModel("application/json", new
                                    {
                                        draw = draw,
                                        recordsTotal = recordsTotal,
                                        recordsFiltered = recordsFiltered,
                                        data = resourceMailboxes
                                    })
                                    .WithView("Company/Exchange/resourcemailboxes.cshtml");
                }
                catch (Exception ex)
                {
                    return Negotiate.WithMediaRangeModel("application/json", new { error = ex.Message })
                                    .WithView("Company/Exchange/resourcemailboxes.cshtml");
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
                this.RequiresValidatedClaims(c => ValidateClaims.AllowCompanyAdmin(Context.CurrentUser, _.CompanyCode, "cExchangeResources"));

                #region Creates a new resource mailbox
                string companyCode = _.CompanyCode;

                dynamic powershell = null;
                CloudPanelContext db = null;
                ReverseActions reverse = new ReverseActions();
                try
                {
                    #region Required Values
                    if (!Request.Form.EmailFirst.HasValue)
                        throw new MissingFieldException("", "EmailFirst");

                    if (!Request.Form.EmailDomain.HasValue)
                        throw new MissingFieldException("", "EmailDomain");

                    if (!Request.Form.SizeInMB.HasValue)
                        throw new MissingFieldException("", "SizeInMB");

                    if (!Request.Form.ResourceType.HasValue)
                        throw new MissingFieldException("", "ResourceType");

                    if (!Request.Form.MailboxPlan.HasValue)
                        throw new MissingFieldException("", "MailboxPlan");
                    #endregion

                    var resourceMailbox = this.Bind<ResourceMailboxes>();
                    resourceMailbox.CompanyCode = companyCode;

                    string emailFirst = Request.Form.EmailFirst;
                    string emailDomain = Request.Form.EmailDomain;

                    db = new CloudPanelContext(Settings.ConnectionString);
                    db.Database.Connection.Open();

                    // Find the domain
                    var domain = (from d in db.Domains
                                  where d.IsAcceptedDomain
                                  where d.CompanyCode == companyCode
                                  where d.Domain == emailDomain
                                  select d).First();

                    // Find the company
                    var company = (from d in db.Companies
                                   where d.CompanyCode == companyCode
                                   select d).First();

                    // Find the mailbox plan
                    var plan = (from d in db.Plans_ExchangeMailbox
                                where d.MailboxPlanID == resourceMailbox.MailboxPlan
                                select d).First();

                    if (domain == null)
                        throw new Exception("Unable to find domain in database: " + emailDomain);

                    if (company == null)
                        throw new Exception("Unable to find the company in database: " + companyCode);

                    if (plan == null)
                        throw new Exception("Unable to find the plan in database: " + resourceMailbox.MailboxPlan);

                    resourceMailbox.PrimarySmtpAddress = string.Format("{0}@{1}", emailFirst.Replace(" ", string.Empty), domain.Domain);
                    resourceMailbox.UserPrincipalName = resourceMailbox.PrimarySmtpAddress;

                    // Initialize powershell
                    powershell = ExchPowershell.GetClass();
                    
                    // Determine what kind of resource we are creating
                    switch (resourceMailbox.ResourceType.ToLower())
                    {
                        case "room":
                            powershell.New_RoomMailbox(resourceMailbox, company.DistinguishedName);
                            reverse.AddAction(Actions.CreateRoomMailbox, resourceMailbox.UserPrincipalName);

                            powershell.Set_RoomMailbox(resourceMailbox, plan, new string[] { "SMTP:" + resourceMailbox.PrimarySmtpAddress });
                            break;
                        case "equipment":
                            powershell.New_EquipmentMailbox(resourceMailbox, company.DistinguishedName);
                            reverse.AddAction(Actions.CreateEquipmentMailbox, resourceMailbox.UserPrincipalName);

                            powershell.Set_EquipmentMailbox(resourceMailbox, plan, new string[] { "SMTP:" + resourceMailbox.PrimarySmtpAddress });
                            break;
                        case "shared":
                            powershell.New_SharedMailbox(resourceMailbox, company.DistinguishedName);
                            reverse.AddAction(Actions.CreateSharedMailbox, resourceMailbox.UserPrincipalName);

                            powershell.Set_SharedMailbox(resourceMailbox, plan, new string[] { "SMTP:" + resourceMailbox.PrimarySmtpAddress });
                            break;
                        default:
                            throw new Exception("Unable to determine if we are creating a room, equipment, or shared mailbox");
                    }

                    db.ResourceMailboxes.Add(resourceMailbox);
                    db.SaveChanges();

                    return Negotiate.WithModel(new { success = "Successfully created new resource mailbox " })
                                    .WithView("Company/Exchange/resourcemailboxes.cshtml");
                }
                catch (Exception ex)
                {
                    logger.ErrorFormat("Unable to create new resource mailbox: {0}", ex.ToString());

                    reverse.RollbackNow();
                    return Negotiate.WithModel(new { error = ex.Message })
                                    .WithStatusCode(HttpStatusCode.InternalServerError)
                                    .WithView("Company/Exchange/resourcemailboxes.cshtml");
                }
                finally
                {
                    if (db != null)
                        db.Dispose();
                }
                #endregion
            };

            Delete["/"] = _ =>
            {
                this.RequiresValidatedClaims(c => ValidateClaims.AllowCompanyAdmin(Context.CurrentUser, _.CompanyCode, "dExchangeResources"));

                #region Deletes a resource

                CloudPanelContext db = null;
                dynamic powershell = null;
                try
                {
                    if (!Request.Form.UserPrincipalName.HasValue)
                        throw new MissingFieldException("", "UserPrincipalName");

                    string companyCode = _.CompanyCode;
                    string userPrincipalName = Request.Form.UserPrincipalName;

                    logger.DebugFormat("Attempting to delete resource {0}", userPrincipalName);
                    db = new CloudPanelContext(Settings.ConnectionString);
                    db.Database.Connection.Open();

                    var resource = (from d in db.ResourceMailboxes
                                    where d.CompanyCode == companyCode
                                    where d.UserPrincipalName == userPrincipalName
                                    select d).FirstOrDefault();
                    if (resource == null)
                        throw new Exception("Unable to find resource mailbox in the database: " + userPrincipalName);
                    else
                    {
                        powershell = ExchPowershell.GetClass();

                        logger.DebugFormat("Trying to determine the type of resource...");
                        switch (resource.ResourceType.ToLower())
                        {
                            case "room":
                                powershell.Remove_RoomMailbox(userPrincipalName);
                                break;
                            case "equipment":
                                powershell.Remove_EquipmentMailbox(userPrincipalName);
                                break;
                            case "shared":
                                powershell.Remove_SharedMailbox(userPrincipalName);
                                break;                                
                            default:
                                throw new Exception("Unable to determine the type of resource: " + resource.ResourceType);
                        }

                        logger.DebugFormat("Removing resource from the database");
                        db.ResourceMailboxes.Remove(resource);
                        db.SaveChanges();

                        return Negotiate.WithModel(new { success = "Successfully deleted resource " + userPrincipalName })
                                        .WithStatusCode(HttpStatusCode.OK);
                    }
                }
                catch (Exception ex)
                {
                    logger.ErrorFormat("Error removing resource: {0}", ex.ToString());
                    return Negotiate.WithModel(new { error = ex.Message })
                                    .WithStatusCode(HttpStatusCode.InternalServerError);
                }
                finally
                {

                }

                #endregion
            };

            Get["/{UserPrincipalName}", c => c.Request.Accept("text/html")] = _ =>
            {
                this.RequiresValidatedClaims(c => ValidateClaims.AllowCompanyAdmin(Context.CurrentUser, _.CompanyCode, "vExchangeResources"));

                string upn = _.UserPrincipalName;
                string companyCode = _.CompanyCode;
                return View["Company/Exchange/resourcemailboxes_edit.cshtml", new { 
                    upn = upn, 
                    MailboxUsers = CloudPanel.Modules.UsersModule.GetMailboxUsers(companyCode) 
                }];
            };

            Get["/{UserPrincipalName}", c => !c.Request.Accept("text/html")] = _ =>
            {
                this.RequiresValidatedClaims(c => ValidateClaims.AllowCompanyAdmin(Context.CurrentUser, _.CompanyCode, "vExchangeResources"));

                #region Gets the resource mailbox information
                CloudPanelContext db = null;
                dynamic powershell = null;
                try
                {
                    db = new CloudPanelContext(Settings.ConnectionString);
                    db.Database.Connection.Open();
                    
                    string companyCode = _.CompanyCode;
                    string upn = _.UserPrincipalName;

                    logger.DebugFormat("Getting resource mailbox {0} in company {1}", upn, companyCode);
                    var resourceMailbox = (from d in db.ResourceMailboxes
                                           where d.CompanyCode == companyCode
                                           where d.UserPrincipalName == upn
                                           select d).FirstOrDefault();
                    if (resourceMailbox == null)
                        throw new Exception("Unable to find resource mailbox in the database: " + upn);
                    else
                    {
                        powershell = ExchPowershell.GetClass();

                        ResourceMailboxes getMailbox = powershell.Get_ResourceMailbox(resourceMailbox.UserPrincipalName);
                        getMailbox.MailboxPlan = resourceMailbox.MailboxPlan;
                        getMailbox.ResourceType = resourceMailbox.ResourceType;
                        getMailbox.CompanyCode = companyCode;
                        getMailbox.DisplayName = resourceMailbox.DisplayName;
                        getMailbox.AdditionalMB = resourceMailbox.AdditionalMB;

                        // Update DistinguishedName in database
                        resourceMailbox.DistinguishedName = getMailbox.DistinguishedName;
                        db.SaveChanges();

                        if (getMailbox == null)
                            throw new Exception("Unable to find mailbox in Exchange " + resourceMailbox.UserPrincipalName);
                        else
                        {
                            return Negotiate.WithModel(new { Mailbox = getMailbox, MailboxUsers = CloudPanel.Modules.UsersModule.GetMailboxUsers(companyCode) })
                                            .WithView("Company/Exchange/resourcemailboxes_edit.cshtml");
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.ErrorFormat("Unable to get resource mailbox: {0}", ex.ToString());
                    return Negotiate.WithModel(new { error = ex.Message })
                                    .WithStatusCode(HttpStatusCode.InternalServerError)
                                    .WithView("error.cshtml");
                }
                finally
                {
                    if (db != null)
                        db.Dispose();
                }
                #endregion
            };

            Post["/{UserPrincipalName}"] = _ =>
            {
                this.RequiresValidatedClaims(c => ValidateClaims.AllowCompanyAdmin(Context.CurrentUser, _.CompanyCode, "eExchangeResources"));

                #region Creates a new resource mailbox
                CloudPanelContext db = null;
                dynamic powershell = null;
                try
                {
                    db = new CloudPanelContext(Settings.ConnectionString);

                    string companyCode = _.CompanyCode;
                    string upn = _.UserPrincipalName;

                    if (!Request.Form.EmailFirst.HasValue)
                        throw new MissingFieldException("", "EmailFirst");

                    if (!Request.Form.EmailDomain.HasValue)
                        throw new MissingFieldException("", "EmailDomain");

                    logger.DebugFormat("Getting resource mailbox {0} in company {1}", upn, companyCode);
                    var resourceMailbox = (from d in db.ResourceMailboxes
                                           where d.CompanyCode == companyCode
                                           where d.UserPrincipalName == upn
                                           select d).FirstOrDefault();

                    var acceptedDomains = (from d in db.Domains
                                        where d.IsAcceptedDomain
                                        where d.CompanyCode == companyCode
                                        select d).ToList();

                    if (resourceMailbox == null)
                        throw new Exception("Unable to find resource mailbox in the database: " + upn);

                    if (acceptedDomains == null)
                        throw new Exception("Unable to find any accepted domains for company " + companyCode);

                    string selectedId = Request.Form.EmailDomain;
                    var selectedDomain = (from d in acceptedDomains
                                          where d.DomainID.ToString() == selectedId
                                          select d).FirstOrDefault();

                    if (selectedDomain == null)
                        throw new Exception("Unable to find a matching domain for " + selectedId);

                    var boundMailbox = this.Bind<ResourceMailboxes>();
                    boundMailbox.DistinguishedName = resourceMailbox.DistinguishedName;
                    boundMailbox.UserPrincipalName = upn;
                    boundMailbox.CompanyCode = companyCode;
                    boundMailbox.PrimarySmtpAddress = string.Format("{0}@{1}", Request.Form.EmailFirst, selectedDomain.Domain);

                    logger.DebugFormat("Retrieving the mailbox plan {0}", boundMailbox.MailboxPlan);
                    var plan = (from d in db.Plans_ExchangeMailbox
                                    where d.MailboxPlanID == boundMailbox.MailboxPlan
                                    select d).FirstOrDefault();

                    if (plan == null)
                        throw new Exception("Unable to find mailbox plan " + boundMailbox.MailboxPlan);
                    else
                    {
                        logger.DebugFormat("Processing email aliases...");
                        var emailAddresses = ValidateEmails(ref boundMailbox, acceptedDomains);

                        logger.DebugFormat("Updating Exchange");
                        powershell = ExchPowershell.GetClass();
                        switch (resourceMailbox.ResourceType.ToLower())
                        {
                            case "room":
                                powershell.Set_RoomMailbox(boundMailbox, plan, emailAddresses.ToArray());
                                break;
                            case "equipment":
                                powershell.Set_EquipmentMailbox(boundMailbox, plan, emailAddresses.ToArray());
                                break;
                            case "shared":
                                powershell.Set_SharedMailbox(boundMailbox, plan, emailAddresses.ToArray());
                                break;
                            default:
                                throw new Exception("Unable to determine the resource type: " + resourceMailbox.ResourceType);
                        }
                    

                        logger.DebugFormat("Processing full access permissions for {0}", boundMailbox.UserPrincipalName);
                        ProcessFullAccess(boundMailbox.UserPrincipalName, ref powershell, boundMailbox.EmailFullAccessOriginal, boundMailbox.EmailFullAccess, boundMailbox.AutoMapping);

                        logger.DebugFormat("Processing send as permissions for {0}", boundMailbox.UserPrincipalName);
                        ProcessSendAs(boundMailbox.DistinguishedName, ref powershell, boundMailbox.EmailSendAsOriginal, boundMailbox.EmailSendAs);

                        string redirectUrl = string.Format("~/company/{0}/exchange/resourcemailboxes", companyCode);
                        return Negotiate.WithModel(new { success = "Successfully updated resource mailbox" })
                                        .WithMediaRangeResponse("text/html", this.Response.AsRedirect(redirectUrl));
                    }
                }
                catch (Exception ex)
                {
                    logger.ErrorFormat("Unable to update resource mailbox: {0}", ex.ToString());
                    return Negotiate.WithModel(new { error = ex.Message })
                                    .WithStatusCode(HttpStatusCode.InternalServerError)
                                    .WithView("error.cshtml");
                }
                finally
                {
                    if (db != null)
                        db.Dispose();
                }
                #endregion
            };
        }

        private List<string> ValidateEmails(ref ResourceMailboxes boundMailbox, List<Domains> validDomains)
        {
            var validatedList = new List<string>() { "SMTP:" + boundMailbox.PrimarySmtpAddress };

            if (boundMailbox.EmailAliases != null)
            {
                foreach (var email in boundMailbox.EmailAliases)
                {
                    if (!string.IsNullOrEmpty(email))
                    {
                        logger.DebugFormat("Validating alias {0} but removing spaces first", email);
                        var e = email.Replace(" ", string.Empty);

                        string[] split = e.Split('@');
                        var findDomain = (from d in validDomains
                                          where d.Domain.Equals(split[1], StringComparison.CurrentCultureIgnoreCase)
                                          select d).FirstOrDefault();

                        if (findDomain == null)
                            throw new Exception("Domain " + split[1] + " is not a valid domain for this company");
                        else
                        {
                            if (!e.StartsWith("sip:") && !e.StartsWith("X500") && !e.StartsWith("X400"))
                                validatedList.Add("smtp:" + e);
                            else
                                validatedList.Add(e);
                        }
                    }
                }
            }

            logger.DebugFormat("Formatted email aliases are: {0}", String.Join(",", validatedList));
            return validatedList;
        }

        /// <summary>
        /// Adds or removes full access permissions
        /// </summary>
        /// <param name="upn"></param>
        /// <param name="powershell"></param>
        /// <param name="original"></param>
        /// <param name="current"></param>
        /// <param name="autoMapping"></param>
        private void ProcessFullAccess(string upn, ref dynamic powershell, string[] original, string[] current, bool autoMapping = true)
        {
            var toAdd = new List<string>();
            var toRemove = new List<string>();

            logger.DebugFormat("Processing full access permissions");
            if (original != null && current != null)
            {
                logger.DebugFormat("Both original and current were not null");
                toAdd = (from c in current
                         where !original.Contains(c)
                         select c).ToList();

                toRemove = (from c in original
                            where !current.Contains(c)
                            select c).ToList();
            }

            logger.DebugFormat("Checking if all values were removed");
            if (original != null && current == null)
                toRemove.AddRange(original);

            logger.DebugFormat("Checking is original was null");
            if (original == null && current != null)
                toAdd.AddRange(current);

            logger.DebugFormat("Continuing...");
            if (toAdd != null && toAdd.Count() > 0)
                powershell.Add_FullAccessPermissions(upn, toAdd.ToArray(), autoMapping);

            if (toRemove != null && toRemove.Count() > 0)
                powershell.Remove_FullAccessPermissions(upn, toRemove.ToArray());
        }

        /// <summary>
        /// Adds or removes send as permissions
        /// </summary>
        /// <param name="distinguishedName"></param>
        /// <param name="powershell"></param>
        /// <param name="original"></param>
        /// <param name="current"></param>
        private void ProcessSendAs(string distinguishedName, ref dynamic powershell, string[] original, string[] current)
        {
            var toAdd = new List<string>();
            var toRemove = new List<string>();

            logger.DebugFormat("Processing send as permissions");
            if (original != null && current != null)
            {
                logger.DebugFormat("Both original and current were not null");
                toAdd = (from c in current
                         where !original.Contains(c)
                         select c).ToList();

                toRemove = (from c in original
                            where !current.Contains(c)
                            select c).ToList();
            }

            logger.DebugFormat("Checking if all values were removed");
            if (original != null && current == null)
                toRemove.AddRange(original);

            logger.DebugFormat("Checking is original was null");
            if (original == null && current != null)
                toAdd.AddRange(current);

            logger.DebugFormat("Continuing...");
            if (toAdd != null && toAdd.Count() > 0)
                powershell.Add_SendAsPermissions(distinguishedName, toAdd.ToArray());

            if (toRemove != null && toRemove.Count() > 0)
                powershell.Remove_SendAsPermissions(distinguishedName, toRemove.ToArray());
        }
    }
}
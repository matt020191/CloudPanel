using CloudPanel.Base.AD;
using CloudPanel.Base.Config;
using CloudPanel.Base.Models.Database;
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
        private static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

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
                        if (!Request.Form.ResourceGuid.HasValue)
                            throw new MissingFieldException("", "ResourceGuid");

                        string companyCode = _.CompanyCode;
                        string resourceGuid = Request.Form.ResourceGuid;
                        Guid guid = Guid.Parse(resourceGuid);

                        logger.DebugFormat("Attempting to delete resource {0}", resourceGuid);
                        db = new CloudPanelContext(Settings.ConnectionString);
                        db.Database.Connection.Open();

                        var resource = (from d in db.ResourceMailboxes
                                        where d.CompanyCode == companyCode
                                        where d.ResourceGuid ==  guid
                                        select d).FirstOrDefault();
                        if (resource == null)
                            throw new Exception("Unable to find resource mailbox in the database: " + resourceGuid);
                        else
                        {
                            powershell = ExchPowershell.GetClass();

                            logger.DebugFormat("Trying to determine the type of resource...");
                            switch (resource.ResourceType.ToLower())
                            {
                                case "room":
                                    powershell.Remove_RoomMailbox(resourceGuid);
                                    break;
                                case "equipment":
                                    powershell.Remove_EquipmentMailbox(resourceGuid);
                                    break;
                                case "shared":
                                    powershell.Remove_SharedMailbox(resourceGuid);
                                    break;                                
                                default:
                                    throw new Exception("Unable to determine the type of resource: " + resource.ResourceType);
                            }

                            logger.DebugFormat("Removing resource from the database");
                            db.ResourceMailboxes.Remove(resource);
                            db.SaveChanges();

                            return Negotiate.WithModel(new { success = "Successfully deleted resource mailbox" })
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
                        if (powershell != null)
                            powershell.Dispose();

                        if (db != null)
                            db.Dispose();
                    }

                    #endregion
                };

            Get["/{Guid:guid}", c => c.Request.Accept("text/html")] = _ =>
                {
                    this.RequiresValidatedClaims(c => ValidateClaims.AllowCompanyAdmin(Context.CurrentUser, _.CompanyCode, "vExchangeResources"));
                    Guid guid = _.Guid;
                    try
                    {
                        return Negotiate.WithModel(new { mailboxusers = UsersModule.GetMailboxUsers(_.CompanyCode) })
                                        .WithView("Company/Exchange/resourcemailboxes_edit.cshtml");
                    }
                    catch (Exception ex)
                    {
                        return Negotiate.WithModel(new { error = ex.Message })
                                        .WithStatusCode(HttpStatusCode.InternalServerError)
                                        .WithView("Error/500.cshtml");
                    }
                };

            Get["/{Guid:guid}", c => !c.Request.Accept("text/html")] = _ =>
                {
                    this.RequiresValidatedClaims(c => ValidateClaims.AllowCompanyAdmin(Context.CurrentUser, _.CompanyCode, "vExchangeResources"));
                    string companyCode = _.CompanyCode;
                    Guid guid = _.Guid;

                    #region Gets the resource mailbox information
                    CloudPanelContext db = null;
                    dynamic powershell = null;
                    try
                    {
                        db = new CloudPanelContext(Settings.ConnectionString);
                        db.Database.Connection.Open();

                        logger.DebugFormat("Getting resource mailbox {0} in company {1}", guid, companyCode);
                        var resourceMailbox = (from d in db.ResourceMailboxes
                                               where d.CompanyCode == companyCode
                                               where d.ResourceGuid == guid
                                               select d).FirstOrDefault();
                        if (resourceMailbox == null)
                            throw new Exception("Unable to find resource mailbox in the database: " + guid);
                        else
                        {
                            powershell = ExchPowershell.GetClass();

                            ResourceMailboxes getMailbox = powershell.Get_ResourceMailbox(guid.ToString());
                            getMailbox.MailboxPlan = resourceMailbox.MailboxPlan;
                            getMailbox.ResourceType = resourceMailbox.ResourceType;
                            getMailbox.CompanyCode = companyCode;
                            getMailbox.DisplayName = resourceMailbox.DisplayName;
                            getMailbox.AdditionalMB = resourceMailbox.AdditionalMB;

                            if (getMailbox == null)
                                throw new Exception("Unable to find mailbox in Exchange " + resourceMailbox.ResourceGuid);
                            else
                            {
                                return Negotiate.WithModel(new { mailbox = getMailbox });
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.ErrorFormat("Unable to get resource mailbox: {0}", ex.ToString());
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

            Post["/{Guid:guid}"] = _ =>
                {
                    this.RequiresValidatedClaims(c => ValidateClaims.AllowCompanyAdmin(Context.CurrentUser, _.CompanyCode, "eExchangeResources"));
                    string companyCode = _.CompanyCode;
                    Guid guid = _.Guid;

                    #region Creates a new resource mailbox
                    CloudPanelContext db = null;
                    dynamic powershell = null;
                    try
                    {
                        if (!Request.Form.EmailFirst.HasValue)
                            throw new MissingFieldException("", "EmailFirst");

                        if (!Request.Form.EmailDomain.HasValue)
                            throw new MissingFieldException("", "EmailDomain");

                        db = new CloudPanelContext(Settings.ConnectionString);
                        db.Database.Connection.Open();

                        var resourceMailbox = (from d in db.ResourceMailboxes
                                               where d.CompanyCode == companyCode
                                               where d.ResourceGuid == guid
                                               select d).Single();

                        var boundMailbox = this.Bind<ResourceMailboxes>();
                        boundMailbox.ResourceGuid = resourceMailbox.ResourceGuid;
                        boundMailbox.DistinguishedName = resourceMailbox.DistinguishedName;
                        boundMailbox.UserPrincipalName = resourceMailbox.UserPrincipalName;
                        boundMailbox.CompanyCode = companyCode;
                        boundMailbox.PrimarySmtpAddress = string.Format("{0}@{1}", Request.Form.EmailFirst, Request.Form.EmailDomain);

                        logger.DebugFormat("Retrieving the mailbox plan {0}", boundMailbox.MailboxPlan);
                        var plan = (from d in db.Plans_ExchangeMailbox
                                    where d.MailboxPlanID == boundMailbox.MailboxPlan
                                    select d).FirstOrDefault();

                        if (plan == null)
                            throw new Exception("Unable to find mailbox plan " + boundMailbox.MailboxPlan);
                        else
                        {
                            logger.DebugFormat("Processing email aliases...");
                            var domains = (from d in db.Domains
                                           where d.CompanyCode == companyCode
                                           where d.IsAcceptedDomain
                                           select d).ToList();

                            var emailAddresses = ValidateEmails(ref boundMailbox, domains);

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
                            if (boundMailbox.EmailFullAccess != null) // If it is null then we can't be adding anything
                            {
                                var addFullAccess = boundMailbox.EmailFullAccess.Except(
                                                    boundMailbox.EmailFullAccessOriginal == null ?
                                                    new string[] { "" } : boundMailbox.EmailFullAccessOriginal);
                                if (addFullAccess.Count() > 0)
                                {
                                    logger.DebugFormat("Adding full access permission to {0} for {1}", boundMailbox.ResourceGuid, String.Join(",", addFullAccess));
                                    powershell.Add_FullAccessPermissions(boundMailbox.ResourceGuid, addFullAccess.ToArray(), boundMailbox.AutoMapping);
                                }
                            }

                            if (boundMailbox.EmailFullAccessOriginal != null) // If the original is null then we cannot be removing anything
                            {
                                var removeFullAccess = boundMailbox.EmailFullAccessOriginal.Except(
                                                       boundMailbox.EmailFullAccess == null ?
                                                       new string[] { "" } : boundMailbox.EmailFullAccess);
                                if (removeFullAccess.Count() > 0)
                                {
                                    logger.DebugFormat("Removing full access permission from {0} for {1}", String.Join(",", removeFullAccess), boundMailbox.ResourceGuid);
                                    powershell.Remove_FullAccessPermissions(boundMailbox.ResourceGuid, removeFullAccess.ToArray());
                                }
                            }

                            
                            logger.DebugFormat("Processing send as permissions for {0}", boundMailbox.UserPrincipalName);
                            if (boundMailbox.EmailSendAs != null) // If its null then we can't be adding anything
                            {
                                var addSendAsAccess = boundMailbox.EmailSendAs.Except(
                                                      boundMailbox.EmailSendAsOriginal == null ?
                                                      new string[] { "" } : boundMailbox.EmailSendAsOriginal);
                                if (addSendAsAccess.Count() > 0)
                                {
                                    logger.DebugFormat("Adding send-as permission to {0} for {1}", boundMailbox.ResourceGuid, String.Join(",", addSendAsAccess));
                                    powershell.Add_SendAsPermissions(boundMailbox.ResourceGuid, addSendAsAccess.ToArray());
                                }
                            }

                            if (boundMailbox.EmailSendAsOriginal != null) // If the original is null then we cannot be removing anything
                            {
                                var removeSendAsAccess = boundMailbox.EmailSendAsOriginal.Except(
                                                         boundMailbox.EmailSendAs == null ?
                                                         new string[] { "" } : boundMailbox.EmailSendAs);
                                if (removeSendAsAccess.Count() > 0)
                                {
                                    logger.DebugFormat("Removing send-as permission from {0} for {1}", String.Join(",", removeSendAsAccess), boundMailbox.ResourceGuid);
                                    powershell.Remove_SendAsPermissions(boundMailbox.ResourceGuid, removeSendAsAccess.ToArray());
                                }
                            }

                            string redirectUrl = string.Format("~/company/{0}/exchange/resourcemailboxes", companyCode);
                            return Negotiate.WithModel(new { success = "Successfully updated resource mailbox" })
                                            .WithMediaRangeResponse("text/html", this.Response.AsRedirect(redirectUrl));
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.ErrorFormat("Unable to update resource mailbox: {0}", ex.ToString());
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
        }

        public static List<ResourceMailboxes> GetResources(string companyCode)
        {
            CloudPanelContext db = null;
            try
            {
                logger.DebugFormat("Retrieving Exchange resources for {0}", companyCode);
                db = new CloudPanelContext(Settings.ConnectionString);

                var resources = (from d in db.ResourceMailboxes
                              where d.CompanyCode == companyCode
                              orderby d.DisplayName
                              select d).ToList();

                if (resources != null)
                {
                    resources.ForEach(x =>
                        {
                            if (!string.IsNullOrEmpty(x.DistinguishedName))
                                x.CanonicalName = LdapConverters.ToCanonicalName(x.DistinguishedName);
                        });
                }

                return resources;
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error retrieving Exchange resources for {0}: {1}", companyCode, ex.ToString());
                throw;
            }
            finally
            {
                if (db != null)
                    db.Dispose();
            }
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
    }
}
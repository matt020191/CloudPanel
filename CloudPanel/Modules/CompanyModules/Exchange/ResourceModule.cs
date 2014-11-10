using Nancy;
using Nancy.Security;
using Nancy.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CloudPanel.Database.EntityFramework;
using CloudPanel.Base.Config;
using System.Reflection;
using CloudPanel.Base.Database.Models;
using CloudPanel.Exchange;
using log4net;
using CloudPanel.Rollback;

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
                return View["Company/Exchange/resourcemailboxes.cshtml"];
            };

            Get["/", c => c.Request.Accept("application/json")] = _ =>
            {
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
                                  select d).FirstOrDefault();

                    // Find the company
                    var company = (from d in db.Companies
                                   where d.CompanyCode == companyCode
                                   select d).FirstOrDefault();

                    // Find the mailbox plan
                    var plan = (from d in db.Plans_ExchangeMailbox
                                where d.MailboxPlanID == resourceMailbox.MailboxPlan
                                select d).FirstOrDefault();

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
                            powershell.Set_RoomMailbox(resourceMailbox, plan);
                            break;
                        case "equipment":
                            powershell.New_EquipmentMailbox(resourceMailbox, company.DistinguishedName);
                            reverse.AddAction(Actions.CreateEquipmentMailbox, resourceMailbox.UserPrincipalName);
                            powershell.Set_EquipmentMailbox(resourceMailbox, plan);
                            break;
                        case "shared":
                            powershell.New_SharedMailbox(resourceMailbox, company.DistinguishedName);
                            reverse.AddAction(Actions.CreateSharedMailbox, resourceMailbox.UserPrincipalName);
                            powershell.Set_SharedMailbox(resourceMailbox, plan);
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

            Put["/"] = _ =>
            {
                return HttpStatusCode.InternalServerError;
            };

            Delete["/"] = _ =>
            {
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
                string upn = _.UserPrincipalName;
                string companyCode = _.CompanyCode;
                return View["Company/Exchange/resourcemailboxes_edit.cshtml", new { 
                    upn = upn, 
                    MailboxUsers = CloudPanel.Modules.UsersModule.GetMailboxUsers(companyCode) 
                }];
            };

            Get["/{UserPrincipalName}", c => c.Request.Accept("application/json")] = _ =>
            {
                CloudPanelContext db = null;
                dynamic powershell = null;
                try
                {
                    db = new CloudPanelContext(Settings.ConnectionString);
                    
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
                return View["Company/Exchange/resourcemailboxes_edit.cshtml"];
            };
        }
    }
}
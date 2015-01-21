using CloudPanel.ActiveDirectory;
using CloudPanel.Base.AD;
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
using Nancy.ViewEngines.Razor;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudPanel.Modules.CompanyModules
{
    public class UsersEditModule : NancyModule
    {
        private static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public UsersEditModule() : base("/company/{CompanyCode}/users/{UserGuid:guid}")
        {
            this.RequiresAuthentication();

            Get["/"] = _ =>
                {
                    this.RequiresValidatedClaims(c => ValidateClaims.AllowCompanyAdmin(Context.CurrentUser, _.CompanyCode, "vUsers"));

                    #region Gets a specific user
                    CloudPanelContext db = null;
                    try
                    {
                        db = new CloudPanelContext(Settings.ConnectionString);
                        db.Database.Connection.Open();

                        logger.DebugFormat("Getting user {0} from the system", _.UserGuid);
                        string companyCode = _.CompanyCode;
                        Guid userGuid = _.UserGuid;

                        logger.DebugFormat("Querying the database for {0}", userGuid);
                        var user = (from d in db.Users.Include(x => x.Role)
                                    join c in db.Companies on d.CompanyCode equals c.CompanyCode into c1
                                    from company in c1.DefaultIfEmpty()
                                    where d.CompanyCode == companyCode
                                    where d.UserGuid == userGuid
                                    select new
                                    {
                                        UserGuid = d.UserGuid,
                                        CompanyCode = d.CompanyCode,
                                        sAMAccountName = d.sAMAccountName,
                                        UserPrincipalName = d.UserPrincipalName,
                                        DistinguishedName = d.DistinguishedName,
                                        DisplayName = d.DisplayName,
                                        Firstname = d.Firstname,
                                        Middlename = d.Middlename,
                                        Lastname = d.Lastname,
                                        Email = d.Email,
                                        Company = d.Company,
                                        TelephoneNumber = d.TelephoneNumber,
                                        JobTitle = d.JobTitle,
                                        Department = d.Department,
                                        HomePhone = d.HomePhone,
                                        MobilePhone = d.MobilePhone,
                                        Street = d.Street,
                                        City = d.City,
                                        State = d.State,
                                        PostalCode = d.PostalCode,
                                        Country = d.Country,
                                        Fax = d.Fax,
                                        IsEnabled = d.IsEnabled,
                                        Skype = d.Skype,
                                        Facebook = d.Facebook,
                                        Twitter = d.Twitter,
                                        Dribbble = d.Dribbble,
                                        Tumblr = d.Tumblr,
                                        LinkedIn = d.LinkedIn,
                                        MailboxPlan = d.MailboxPlan,
                                        ArchivePlan = d.ArchivePlan,
                                        Notes = d.Notes,
                                        AdditionalMB = d.AdditionalMB == null ? 0 : (int)d.AdditionalMB,
                                        Role = d.Role,
                                        CompanyName = company.CompanyName,
                                        ExchEnabled = company.ExchEnabled
                                    }).FirstOrDefault();

                        if (user == null)
                            throw new Exception("Unable to find user in database");
                        else
                        {
                            if (user.Role != null)
                                logger.DebugFormat("User's permission role id is {0}", user.Role.RoleID);

                            // Update samAccountName if needed
                            FixSamAccountName(user.UserGuid, user.sAMAccountName);

                            return Negotiate.WithModel(user)
                                            .WithStatusCode(HttpStatusCode.OK)
                                            .WithView("Company/user_edit.cshtml");
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.ErrorFormat("Error getting user {0}: {1}", _.UserGuid, ex.ToString());
                        return Negotiate.WithModel(new { error = ex.Message })
                                        .WithStatusCode(HttpStatusCode.InternalServerError)
                                        .WithView("Error/500.cshtml");
                    }
                    finally
                    {
                        if (db != null)
                            db.Dispose();
                    }
                    #endregion
                };

            Get["/mailbox"] = _ =>
                {
                    this.RequiresValidatedClaims(c => ValidateClaims.AllowCompanyAdmin(Context.CurrentUser, _.CompanyCode, "vUsers"));

                    #region Gets a specific user's mailbox
                    string companyCode = _.CompanyCode;
                    Guid userGuid = _.UserGuid;

                    dynamic powershell = null;
                    try
                    {
                        logger.DebugFormat("Getting mailbox {0} from Exchange", _.UserGuid);
                        powershell = ExchPowershell.GetClass();

                        var mailbox = powershell.Get_Mailbox(new Users() { UserGuid = userGuid });
                        return Negotiate.WithModel(new { mailbox = mailbox })
                                        .WithStatusCode(HttpStatusCode.OK);
                    }
                    catch (Exception ex)
                    {
                        logger.ErrorFormat("Error getting mailbox {0}: {1}", _.UserGuid, ex.ToString());
                        return Negotiate.WithModel(new { error = ex.Message })
                                        .WithStatusCode(HttpStatusCode.InternalServerError);
                    }
                    finally
                    {
                        if (powershell != null)
                            powershell.Dispose();
                    }
                    #endregion
                };

            Put["/"] = _ =>
                {
                    this.RequiresValidatedClaims(c => ValidateClaims.AllowCompanyAdmin(Context.CurrentUser, _.CompanyCode, "eUsers"));
                    string companyCode = _.CompanyCode;
                    Guid userGuid = _.UserGuid;

                    #region Updates a specific user's information
                    CloudPanelContext db = null;
                    ADUsers adUsers = null;
                    try
                    {
                        logger.DebugFormat("Retrieving user {0} in company {1} from database", userGuid, companyCode);
                        db = new CloudPanelContext(Settings.ConnectionString);
                        db.Database.Connection.Open();

                        var user = (from d in db.Users
                                    where d.UserGuid == userGuid
                                    where d.CompanyCode == companyCode
                                    select d).FirstOrDefault();

                        if (user == null)
                            throw new ArgumentNullException("User");

                        logger.DebugFormat("Binding to form and updating data");
                        var boundUser = this.Bind<Users>();

                        logger.DebugFormat("Setting SQL values");
                        //user.sAMAccountName = boundUser.sAMAccountName;
                        user.DistinguishedName = boundUser.DistinguishedName;
                        user.DisplayName = boundUser.DisplayName;
                        user.Firstname = boundUser.Firstname;
                        user.Middlename = boundUser.Middlename;
                        user.Lastname = boundUser.Lastname;
                        user.Department = boundUser.Department;
                        user.Skype = boundUser.Skype;
                        user.Facebook = boundUser.Facebook;
                        user.Twitter = boundUser.Twitter;
                        user.Dribbble = boundUser.Dribbble;
                        user.Tumblr = boundUser.Tumblr;
                        user.LinkedIn = boundUser.LinkedIn;
                        user.Street = boundUser.Street;
                        user.City = boundUser.City;
                        user.State = boundUser.State;
                        user.PostalCode = boundUser.PostalCode;
                        user.Country = boundUser.Country;
                        user.Company = boundUser.Company;
                        user.JobTitle = boundUser.JobTitle;
                        user.TelephoneNumber = boundUser.TelephoneNumber;
                        user.Fax = boundUser.Fax;
                        user.HomePhone = boundUser.HomePhone;
                        user.MobilePhone = boundUser.MobilePhone;
                        user.Notes = boundUser.Notes;
                        
                        if (this.Context.IsSuperOrResellerAdmin())
                        {
                            logger.DebugFormat("Checking changes to the user role since the logged in user is a super admin or reseller admin");
                            if (boundUser.RoleID > 0)
                            {
                                logger.DebugFormat("Role ID is {0}", boundUser.RoleID);
                                user.IsCompanyAdmin = true;
                                user.RoleID = boundUser.RoleID;
                            }
                            else
                            {
                                user.IsCompanyAdmin = false;
                                user.RoleID = null;
                            }
                        }

                        logger.DebugFormat("Setting Active Directory values");
                        adUsers = new ADUsers(Settings.Username, Settings.DecryptedPassword, Settings.PrimaryDC);
                        adUsers.UpdateUser(user);

                        db.SaveChanges();

                        return Negotiate.WithModel(new { success = "Successfully updated user values" })
                                        .WithStatusCode(HttpStatusCode.OK);
                    }
                    catch (Exception ex)
                    {
                        logger.ErrorFormat("Error updating user values: {0}", ex.ToString());
                        return Negotiate.WithModel(new { error = ex.Message })
                                        .WithStatusCode(HttpStatusCode.InternalServerError);
                    }
                    finally
                    {
                        if (adUsers != null)
                            adUsers.Dispose();

                        if (db != null)
                            db.Dispose();
                    }
                    #endregion
                };

            Post["/resetpassword"] = _ =>
                {
                    this.RequiresValidatedClaims(c => ValidateClaims.AllowCompanyAdmin(Context.CurrentUser, _.CompanyCode, "eUsers"));

                    #region resets a user's password
                    string companyCode = _.CompanyCode;
                    Guid userGuid = _.UserGuid;

                    ADUsers adUser = null;
                    CloudPanelContext db = null;
                    try
                    {
                        if (!Request.Form.Password.HasValue)
                            throw new MissingFieldException("", "Password");

                        db = new CloudPanelContext(Settings.ConnectionString);
                        var user = (from d in db.Users
                                    where d.CompanyCode == companyCode
                                    where d.UserGuid == userGuid
                                    select d).FirstOrDefault();
                        if (user == null)
                            throw new Exception("User was not found in database.");
                        else
                        {
                            adUser = new ADUsers(Settings.Username, Settings.DecryptedPassword, Settings.PrimaryDC);
                            adUser.ResetPassword(userGuid, Request.Form.Password);
                        }

                        return Negotiate.WithModel(new { success = "Successfully reset password for " + user.UserPrincipalName });
                    }
                    catch (Exception ex)
                    {
                        logger.ErrorFormat("Error resetting user {0}'s password", userGuid);
                        return Negotiate.WithModel(new { error = ex.Message })
                                        .WithStatusCode(HttpStatusCode.InternalServerError);
                    }
                    finally
                    {
                        if (db != null)
                            db.Dispose();

                        if (adUser != null)
                            adUser.Dispose();
                    }
                    #endregion
                };

            Post["/changelogin"] = _ =>
                {
                    this.RequiresValidatedClaims(c => ValidateClaims.AllowCompanyAdmin(Context.CurrentUser, _.CompanyCode, "eUsers"));
                    Guid userGuid = _.UserGuid;
                    string companyCode = _.CompanyCode;

                    #region change a users login name
                    ADUsers adUser = null;
                    CloudPanelContext db = null;
                    try
                    {
                        if (!Request.Form.NewUsername.HasValue)
                            throw new MissingFieldException("", "NewUsername");

                        if (!Request.Form.NewDomain.HasValue)
                            throw new MissingFieldException("", "NewDomain");

                        db = new CloudPanelContext(Settings.ConnectionString);
                        var user = (from d in db.Users
                                    where d.CompanyCode == companyCode
                                    where d.UserGuid == userGuid
                                    select d).FirstOrDefault();
                        if (user == null)
                            throw new Exception("User " + userGuid + " was not found in database.");
                        else
                        {
                            int domainId = Request.Form.NewDomain;

                            var domain = (from d in db.Domains
                                          where d.CompanyCode == companyCode
                                          where d.DomainID == domainId
                                          select d).FirstOrDefault();
                            if (domain == null)
                                throw new Exception("Unable to find domain in the database");

                            logger.DebugFormat("Connecting to Active Directory..");
                            adUser = new ADUsers(Settings.Username, Settings.DecryptedPassword, Settings.PrimaryDC);

                            logger.DebugFormat("Resetting username...");
                            string oldUpn = user.UserPrincipalName;
                            string newUpn = string.Format("{0}@{1}", Request.Form.NewUsername.Value, domain.Domain);
                            Users updatedUser = adUser.ChangeLogin(userGuid, newUpn, null, user.DisplayName, Settings.SamAccountNameFormat);

                            logger.DebugFormat("Updating database with new username");
                            user.UserPrincipalName = updatedUser.UserPrincipalName;
                            user.sAMAccountName = updatedUser.sAMAccountName;
                            user.DistinguishedName = updatedUser.DistinguishedName;

                            db.SaveChanges();

                            return Negotiate.WithModel(new { success = string.Format("Username {0} has been successfully changed to {1}", oldUpn, user.UserPrincipalName) });
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.ErrorFormat("Error updating user {0}'s username", userGuid);
                        return Negotiate.WithModel(new { error = ex.Message })
                                        .WithStatusCode(HttpStatusCode.InternalServerError);
                    }
                    finally
                    {
                        if (db != null)
                            db.Dispose();

                        if (adUser != null)
                            adUser.Dispose();
                    }
                    #endregion
                };

            Put["/mailbox"] = _ =>
                {
                    this.RequiresValidatedClaims(c => ValidateClaims.AllowCompanyAdmin(Context.CurrentUser, _.CompanyCode, "eUsers"));
                    string companyCode = _.CompanyCode;
                    Guid userGuid = _.UserGuid;

                    #region Updates a specific user's mailbox
                    CloudPanelContext db = null;
                    dynamic powershell = null;

                    ReverseActions reverse = new ReverseActions();
                    try
                    {
                        logger.DebugFormat("Getting user {0} from database", userGuid);
                        db = new CloudPanelContext(Settings.ConnectionString);
                        db.Database.Connection.Open();

                        var user = (from d in db.Users
                                    where d.CompanyCode == companyCode
                                    where d.UserGuid == userGuid
                                    select d).FirstOrDefault();

                        logger.DebugFormat("Binding the user to the form");
                        var boundUser = this.Bind<Users>();
                        boundUser.UserGuid = userGuid;

                        // Only make email changes if it was changed
                        #region Mailbox Changes
                        logger.DebugFormat("Determining the status of the mailbox");
                        powershell = ExchPowershell.GetClass();
                        if (user.MailboxPlan > 0 && !boundUser.IsMailboxEnabled)
                        {
                            #region Disable Mailbox
                            logger.InfoFormat("Disabling mailbox for {0}", userGuid);
                            powershell.Disable_Mailbox(userGuid);

                            user.MailboxPlan = 0;
                            user.AdditionalMB = 0;
                            user.ArchivePlan = 0;
                            user.LitigationHoldEnabled = false;
                            user.ActiveSyncDevices = null;

                            db.SaveChanges();
                            #endregion
                        }
                        else
                        {
                            #region Additional form validation

                            if (!Request.Form.EmailFirst.HasValue)
                                throw new MissingFieldException("", "EmailFirst");

                            if (!Request.Form.EmailDomain.HasValue)
                                throw new MissingFieldException("", "EmailDomain");

                            if (!Request.Form.MailboxPlan.HasValue)
                                throw new MissingFieldException("", "MailboxPlan");

                            if (!Request.Form.SizeInMB.HasValue)
                                throw new MissingFieldException("", "SizeInMB");

                            #endregion

                            // Get the primary email address
                            boundUser.Email = string.Format("{0}@{1}", Request.Form.EmailFirst.Value, Request.Form.EmailDomain.Value);

                            // Validate that the domains are for this company
                            var validatedEmails = ValidateEmails(ref boundUser, (from d in db.Domains
                                                                                 where d.CompanyCode == companyCode
                                                                                 where d.IsAcceptedDomain
                                                                                 select d).ToList());
                            // Get the plan chosen
                            var plan = (from d in db.Plans_ExchangeMailbox
                                        where d.MailboxPlanID == boundUser.MailboxPlan
                                        where (d.CompanyCode == companyCode || string.IsNullOrEmpty(d.CompanyCode))
                                        select d).First();

                            if (user.MailboxPlan > 0 && boundUser.IsMailboxEnabled)
                            {
                                #region Update Mailbox
                                // Update mailbox
                                logger.InfoFormat("Updating mailbox for {0}", userGuid);

                                powershell.Set_Mailbox(boundUser, plan, validatedEmails.ToArray());
                                powershell.Set_CASMailbox(boundUser.UserGuid, plan, null);

                                user.MailboxPlan = plan.MailboxPlanID;
                                user.Email = boundUser.Email;
                                user.AdditionalMB = (plan.MailboxSizeMB > 0 ? (boundUser.SizeInMB - plan.MailboxSizeMB) : 0);
                                db.SaveChanges();
                                #endregion
                            }
                            else if (user.MailboxPlan < 1 && boundUser.IsMailboxEnabled)
                            {
                                #region Enable Mailbox
                                // Enable mailbox
                                logger.InfoFormat("Enabling mailbox for {0}", userGuid);

                                powershell.Enable_Mailbox(boundUser);
                                reverse.AddAction(Actions.CreateMailbox, userGuid);

                                powershell.Set_Mailbox(boundUser, plan, validatedEmails.ToArray());
                                powershell.Set_CASMailbox(boundUser.UserGuid, plan, null);

                                user.MailboxPlan = plan.MailboxPlanID;
                                user.Email = boundUser.Email;
                                user.AdditionalMB = (plan.MailboxSizeMB > 0 ? (boundUser.SizeInMB - plan.MailboxSizeMB) : 0);
                                db.SaveChanges();
                                #endregion
                            }

                            #region Full Access and SendAs Permissions
                            // Update full access
                            logger.DebugFormat("Updating full access permissions");
                            if (boundUser.EmailFullAccess != null) // If it is null then we can't be adding anything
                            {
                                var addFullAccess = boundUser.EmailFullAccess.Except(
                                                    boundUser.EmailFullAccessOriginal == null ?
                                                    new string[] { "" } : boundUser.EmailFullAccessOriginal);
                                if (addFullAccess.Count() > 0)
                                {
                                    logger.DebugFormat("Adding full access permission to {0} for {1}", userGuid, String.Join(",", addFullAccess));
                                    powershell.Add_FullAccessPermissions(userGuid, addFullAccess.ToArray(), boundUser.AutoMapping);
                                }
                            }

                            if (boundUser.EmailFullAccessOriginal != null) // If the original is null then we cannot be removing anything
                            {
                                var removeFullAccess = boundUser.EmailFullAccessOriginal.Except(
                                                       boundUser.EmailFullAccess == null ?
                                                       new string[] { "" } : boundUser.EmailFullAccess);
                                if (removeFullAccess.Count() > 0)
                                {
                                    logger.DebugFormat("Removing full access permission from {0} for {1}", String.Join(",", removeFullAccess), userGuid);
                                    powershell.Remove_FullAccessPermissions(userGuid, removeFullAccess.ToArray());
                                }
                            }

                            // Update send as access
                            logger.DebugFormat("Updating send as permissions");
                            if (boundUser.EmailSendAs != null) // If its null then we can't be adding anything
                            {
                                var addSendAsAccess = boundUser.EmailSendAs.Except(
                                                      boundUser.EmailSendAsOriginal == null ?
                                                      new string[] { "" } : boundUser.EmailSendAsOriginal);
                                if (addSendAsAccess.Count() > 0)
                                {
                                    logger.DebugFormat("Adding send-as permission to {0} for {1}", userGuid, String.Join(",", addSendAsAccess));
                                    powershell.Add_SendAsPermissions(userGuid, addSendAsAccess.ToArray());
                                }
                            }

                            if (boundUser.EmailSendAsOriginal != null) // If the original is null then we cannot be removing anything
                            {
                                var removeSendAsAccess = boundUser.EmailSendAsOriginal.Except(
                                                         boundUser.EmailSendAs == null ?
                                                         new string[] { "" } : boundUser.EmailSendAs);
                                if (removeSendAsAccess.Count() > 0)
                                {
                                    logger.DebugFormat("Removing send-as permission from {0} for {1}", String.Join(",", removeSendAsAccess), userGuid);
                                    powershell.Remove_SendAsPermissions(userGuid, removeSendAsAccess.ToArray());
                                }
                            }
                            #endregion
                        }
                        #endregion

                        logger.InfoFormat("Successfully updated {0} mailbox settings", userGuid);
                        return Negotiate.WithModel(new { success = "Successfully updated mailbox settings" })
                                        .WithStatusCode(HttpStatusCode.OK);
                    }
                    catch (Exception ex)
                    {
                        logger.ErrorFormat("Error updating user {0} mailbox information: {1}", userGuid, ex.ToString());

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
                    #endregion
                };

            Put["/mailbox/litigationhold"] = _ =>
                {
                    this.RequiresValidatedClaims(c => ValidateClaims.AllowCompanyAdmin(Context.CurrentUser, _.CompanyCode, "eUsers"));
                    string companyCode = _.CompanyCode;
                    Guid userGuid = _.UserGuid;

                    #region Updates a specific user's mailbox litigation hold settings
                    CloudPanelContext db = null;
                    dynamic powershell = null;
                    try
                    {
                        logger.DebugFormat("Getting user {0} from database for litigation hold", userGuid);
                        db = new CloudPanelContext(Settings.ConnectionString);
                        db.Database.Connection.Open();

                        var user = (from d in db.Users
                                    where d.CompanyCode == companyCode
                                    where d.UserGuid == userGuid
                                    select d).FirstOrDefault();

                        logger.DebugFormat("Binding the user to the form for litigation hold");
                        var boundUser = this.Bind<Users>();
                        boundUser.UserGuid = userGuid;

                        // Check litigation hold
                        #region Litigation Hold
                        if (user.MailboxPlan > 0)
                        {
                            powershell = ExchPowershell.GetClass();

                            int? litigationHoldDuration = null;
                            if (Request.Form.LitigationHoldDuration.HasValue)
                            {
                                DateTime duration = DateTime.Parse(Request.Form.LitigationHoldDuration.Value);
                                litigationHoldDuration = (int)duration.Subtract(DateTime.Now).TotalDays;
                            }

                            powershell.Set_LitigationHold(userGuid, boundUser.LitigationHoldEnabled, boundUser.RetentionUrl, boundUser.RetentionComment, Context.CurrentUser.UserName, litigationHoldDuration);
                            user.LitigationHoldEnabled = boundUser.LitigationHoldEnabled;
                            db.SaveChanges();
                        }
                        #endregion

                        logger.InfoFormat("Successfully updated {0} litigation hold settings", userGuid);
                        return Negotiate.WithModel(new { success = "Successfully updated litigation settings" })
                                        .WithStatusCode(HttpStatusCode.OK);
                    }
                    catch (Exception ex)
                    {
                        logger.ErrorFormat("Error updating user {0} litigation hold information: {1}", userGuid, ex.ToString());
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

            Put["/mailbox/archive"] = _ =>
                {
                    this.RequiresValidatedClaims(c => ValidateClaims.AllowCompanyAdmin(Context.CurrentUser, _.CompanyCode, "eUsers"));
                    string companyCode = _.CompanyCode;
                    Guid userGuid = _.UserGuid;

                    #region Updates a specific user's mailbox
                    CloudPanelContext db = null;
                    dynamic powershell = null;

                    ReverseActions reverse = new ReverseActions();
                    try
                    {
                        logger.DebugFormat("Getting user {0} from database for archive", userGuid);
                        db = new CloudPanelContext(Settings.ConnectionString);
                        db.Database.Connection.Open();

                        var user = (from d in db.Users
                                    where d.CompanyCode == companyCode
                                    where d.UserGuid == userGuid
                                    select d).FirstOrDefault();

                        logger.DebugFormat("Binding the user to the form for litigation hold");
                        var boundUser = this.Bind<Users>();
                        boundUser.UserGuid = userGuid;

                        if (user.ArchivePlan > 0 && boundUser.ArchivePlan == 0)
                        {
                            logger.DebugFormat("Disabling archive mailbox for {0}", userGuid);
                            powershell = ExchPowershell.GetClass();
                            powershell.Disable_ArchiveMailbox(userGuid);

                            user.ArchivePlan = 0;
                            db.SaveChanges();
                        }
                        else if (user.ArchivePlan > 0 && boundUser.ArchivePlan > 0)
                        {
                            logger.DebugFormat("Updating archive mailbox for {0}", userGuid);
                            var plan = (from d in db.Plans_ExchangeArchiving
                                        where d.ArchivingID == boundUser.ArchivePlan
                                        where (d.CompanyCode == companyCode || string.IsNullOrEmpty(d.CompanyCode))
                                        select d).FirstOrDefault();

                            if (plan == null)
                                throw new ArgumentNullException("Plans_ExchangeArchiving");
                            else
                            {
                                powershell = ExchPowershell.GetClass();
                                powershell.Set_ArchiveMailbox(userGuid, null, plan.ArchiveSizeMB);

                                user.ArchivePlan = plan.ArchivingID;
                                db.SaveChanges();
                            }
                        }
                        else if (user.ArchivePlan < 1 && boundUser.ArchivePlan > 0)
                        {
                            logger.DebugFormat("Enabling archive mailbox for {0}", userGuid);
                            var plan = (from d in db.Plans_ExchangeArchiving
                                        where d.ArchivingID == boundUser.ArchivePlan
                                        where (d.CompanyCode == companyCode || string.IsNullOrEmpty(d.CompanyCode))
                                        select d).FirstOrDefault();

                            if (plan == null)
                                throw new ArgumentNullException("Plans_ExchangeArchiving");
                            else
                            {
                                powershell = ExchPowershell.GetClass();
                                powershell.Enable_ArchiveMailbox(userGuid, null, null);
                                reverse.AddAction(Actions.CreateArchiveMailbox, userGuid);

                                powershell.Set_ArchiveMailbox(userGuid, plan.ArchiveSizeMB);

                                user.ArchivePlan = plan.ArchivingID;
                                db.SaveChanges();
                            }
                        }


                        logger.InfoFormat("Successfully updated {0} archiving settings", userGuid);
                        return Negotiate.WithModel(new { success = "Successfully updated archive settings" })
                                        .WithStatusCode(HttpStatusCode.OK);
                    }
                    catch (Exception ex)
                    {
                        logger.ErrorFormat("Error updating user {0} archive settings: {1}", userGuid, ex.ToString());
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
        }

        /// <summary>
        /// Fixes the samAccountName missing from the database
        /// </summary>
        /// <param name="userPrincipalName"></param>
        /// <param name="samAccountName"></param>
        /// <returns></returns>
        private async void FixSamAccountName(Guid userGuid, string samAccountName)
        {
            await Task.Run(() =>
            {
                if (!string.IsNullOrEmpty(samAccountName))
                    logger.DebugFormat("User {0} samAccountName is already set to {1}", userGuid, samAccountName);
                else
                {
                    using (CloudPanelContext db = new CloudPanelContext(Settings.ConnectionString))
                    {
                        var user = (from d in db.Users
                                    where d.UserGuid == userGuid
                                    select d).Single();

                        ADUsers usr = null;
                        try
                        {
                            usr = new ADUsers(Settings.Username, Settings.DecryptedPassword, Settings.PrimaryDC);
                            string sam = usr.GetUserSamAccountName(userGuid);

                            user.sAMAccountName = sam;
                            db.SaveChanges();
                        }
                        catch (Exception ex)
                        {
                            logger.ErrorFormat("Error updating user's samAccountName: {0}", ex.ToString());
                        }
                        finally
                        {
                            if (usr != null)
                                usr.Dispose();
                        }
                    }
                }
            });
        }

        #region Exchange Methods

        /// <summary>
        /// Validates that the emails end with a domain the company has access to
        /// </summary>
        /// <param name="boundUser"></param>
        /// <param name="validDomains"></param>
        /// <returns></returns>
        private List<string> ValidateEmails(ref Users boundUser, List<Domains> validDomains)
        {
            var validatedList = new List<string>();
            string primaryDomain = boundUser.Email.Split('@')[1];

            if (validDomains.Any(x => x.Domain.Equals(primaryDomain, StringComparison.InvariantCultureIgnoreCase)))
            {
                validatedList.Add("SMTP:" + boundUser.Email);
                if (boundUser.EmailAliases != null)
                {
                    foreach (var alias in boundUser.EmailAliases)
                    {
                        // Remove whitespace characters (not allowed)
                        string adjustedAlias = alias.Replace(" ", string.Empty);

                        var aliasDomain = adjustedAlias.Split('@')[1];
                        if (validDomains.Any(x => x.Domain.Equals(aliasDomain, StringComparison.InvariantCultureIgnoreCase)))
                        {
                            if (!alias.StartsWith("sip:", StringComparison.InvariantCultureIgnoreCase) && 
                                !alias.StartsWith("X500", StringComparison.InvariantCultureIgnoreCase) && 
                                !alias.StartsWith("X400", StringComparison.InvariantCultureIgnoreCase))
                                validatedList.Add("smtp:" + adjustedAlias);
                            else
                                validatedList.Add(adjustedAlias);
                        }
                        else
                            throw new Exception("Alias " + alias + " is not a valid domain for this company");
                    }
                }
                else
                    logger.DebugFormat("No email aliases to validate.");
            }
            else
                throw new Exception("Domain " + primaryDomain + " is not a valid domain for this company");

            return validatedList;
        }

        /// <summary>
        /// Gets a list of accepted domains for the company
        /// </summary>
        /// <param name="companyCode"></param>
        /// <param name="db"></param>
        /// <returns></returns>
        public static List<Domains> GetAcceptedDomains(string companyCode, ref CloudPanelContext db)
        {
            logger.DebugFormat("Retrieving accepted domains for {0}", companyCode);
            var domains = (from d in db.Domains
                           where d.CompanyCode == companyCode
                           where d.IsAcceptedDomain
                           select d).ToList();

            return domains;
        }

        /// <summary>
        /// Gets a list of mailboxes for ForwardTo 
        /// </summary>
        /// <param name="companyCode"></param>
        /// <returns></returns>
        public static IHtmlString GetMailboxesWithOptions(string companyCode)
        {
            var sb = new StringBuilder();

            CloudPanelContext db = null;
            try
            {
                db = new CloudPanelContext(Settings.ConnectionString);

                var mailboxes = (from d in db.Users
                                 where d.CompanyCode == companyCode
                                 orderby d.DisplayName
                                 select d).ToList();

                mailboxes.ForEach(x =>
                    {
                        x.CanoncialName = LdapConverters.ToCanonicalName(x.DistinguishedName);
                        string append = string.Format("<option value='{0}'>{1}</option>",
                                                       x.CanoncialName,
                                                       x.DisplayName);
                        sb.AppendLine(append);
                    });

                return new NonEncodedHtmlString(sb.ToString());
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error getting mailboxes with options for {0}: {1}", companyCode, ex.ToString());
                return new NonEncodedHtmlString("");
            }
            finally
            {
                if (db != null)
                    db.Dispose();
            }
        }

        /// <summary>
        /// Gets a list of groups for ForwardTo
        /// </summary>
        /// <param name="companyCode"></param>
        /// <returns></returns>
        public static IHtmlString GetGroupsWithOptions(string companyCode)
        {
            var sb = new StringBuilder();

            CloudPanelContext db = null;
            try
            {
                db = new CloudPanelContext(Settings.ConnectionString);

                var groups = (from d in db.DistributionGroups
                                 where d.CompanyCode == companyCode
                                 orderby d.DisplayName
                                 select d).ToList();

                groups.ForEach(x =>
                {
                    x.CanonicalName = LdapConverters.ToCanonicalName(x.DistinguishedName);
                    string append = string.Format("<option value='{0}'>{1}</option>",
                                                   x.CanonicalName,
                                                   x.DisplayName);
                    sb.AppendLine(append);
                });

                return new NonEncodedHtmlString(sb.ToString());
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error getting distribution groups with options for {0}: {1}", companyCode, ex.ToString());
                return new NonEncodedHtmlString("");
            }
            finally
            {
                if (db != null)
                    db.Dispose();
            }
        }

        /// <summary>
        /// Gets a list of contacts for ForwardTo
        /// </summary>
        /// <param name="companyCode"></param>
        /// <returns></returns>
        public static IHtmlString GetContactsWithOptions(string companyCode)
        {
            var sb = new StringBuilder();

            CloudPanelContext db = null;
            try
            {
                db = new CloudPanelContext(Settings.ConnectionString);

                var contacts = (from d in db.Contacts
                              where d.CompanyCode == companyCode
                              orderby d.DisplayName
                              select d).ToList();

                contacts.ForEach(x =>
                {
                    x.CanoncialName = LdapConverters.ToCanonicalName(x.DistinguishedName);
                    string append = string.Format("<option value='{0}'>{1}</option>",
                                                   x.CanoncialName,
                                                   x.DisplayName);
                    sb.AppendLine(append);
                });

                return new NonEncodedHtmlString(sb.ToString());
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error getting distribution groups with options for {0}: {1}", companyCode, ex.ToString());
                return new NonEncodedHtmlString("");
            }
            finally
            {
                if (db != null)
                    db.Dispose();
            }
        }

        #endregion

    }
}
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
using System.Linq;
using System.Text;

namespace CloudPanel.Modules.CompanyModules
{
    public class UsersEditModule : NancyModule
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(UsersEditModule));

        public UsersEditModule() : base("/company/{CompanyCode}/users/{UserGuid:guid}")
        {
            this.RequiresAuthentication();

            Get["/", c => c.Request.Accept("text/html")] = _ =>
            {
                this.RequiresValidatedClaims(c => ValidateClaims.AllowCompanyAdmin(Context.CurrentUser, _.CompanyCode, "vUsers"));

                logger.DebugFormat("Loading user edit page for company {0} and user {1}", _.CompanyCode, _.UserGuid);
                string companyCode = _.CompanyCode;
                string userGuid = _.UserPrincipalName;
                return View["Company/users_edit.cshtml", new { 
                    CompanyCode = companyCode, 
                    UserGuid = userGuid,
                    MailboxUsers = UsersModule.GetMailboxUsers(companyCode) 
                }];
            };

            Get["/", c => c.Request.Accept("application/json")] = _ =>
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
                    var user = (from d in db.Users
                                where d.CompanyCode == companyCode
                                where d.UserGuid == userGuid
                                select d).FirstOrDefault();

                    if (user == null)
                        throw new Exception("Unable to find user in database");
                    else
                    {
                        return Negotiate.WithModel(new { user = user })
                                        .WithStatusCode(HttpStatusCode.OK);
                    }
                }
                catch (Exception ex)
                {
                    logger.ErrorFormat("Error getting user {0}: {1}", _.UserGuid, ex.ToString());
                    return Negotiate.WithModel(new { error = ex.Message })
                                    .WithStatusCode(HttpStatusCode.InternalServerError);
                }
                finally
                {
                    if (db != null)
                        db.Dispose();
                }
                #endregion
            };

            Get["/mailbox", c => c.Request.Accept("application/json")] = _ =>
            {
                this.RequiresValidatedClaims(c => ValidateClaims.AllowCompanyAdmin(Context.CurrentUser, _.CompanyCode, "vUsers"));

                #region Gets a specific user's mailbox
                dynamic powershell = null;
                try
                {
                    string companyCode = _.CompanyCode;
                    Guid userGuid = _.UserGuid;

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

                #region Updates a user

                CloudPanelContext db = null;
                ADUsers adUser = null;
                dynamic powershell = null;
                try
                {
                    string companyCode = _.CompanyCode;
                    Guid userGuid = _.UserGuid;

                    logger.DebugFormat("Retrieving user from database");
                    db = new CloudPanelContext(Settings.ConnectionString);
                    db.Database.Connection.Open();

                    var sqlUser = (from d in db.Users
                                   where d.CompanyCode == companyCode
                                   where d.UserGuid == userGuid
                                   select d).FirstOrDefault();

                    if (sqlUser == null)
                        throw new Exception("Unable to find user in the database: " + userGuid);
                    else
                    {
                        // Model bind
                        logger.DebugFormat("Binding form to class...");

                        var boundUser = this.Bind<Users>();
                        boundUser.UserGuid = userGuid;
                        boundUser.CompanyCode = companyCode;

                        // Combine the old values in sql with the new values in sql
                        logger.DebugFormat("Updating database values");
                        sqlUser.Firstname = boundUser.Firstname;
                        sqlUser.Middlename = boundUser.Middlename;
                        sqlUser.Lastname = boundUser.Lastname;
                        sqlUser.DisplayName = boundUser.DisplayName;
                        sqlUser.Company = boundUser.Company;
                        sqlUser.Department = boundUser.Department;
                        sqlUser.JobTitle = boundUser.JobTitle;
                        sqlUser.TelephoneNumber = boundUser.TelephoneNumber;
                        sqlUser.Fax = boundUser.Fax;
                        sqlUser.HomePhone = boundUser.HomePhone;
                        sqlUser.MobilePhone = boundUser.MobilePhone;
                        sqlUser.Street = boundUser.Street;
                        sqlUser.City = boundUser.City;
                        sqlUser.State = boundUser.State;
                        sqlUser.PostalCode = boundUser.PostalCode;
                        sqlUser.Country = boundUser.Country;
                        sqlUser.RoleID = boundUser.RoleID;
                        sqlUser.ChangePasswordNextLogin = boundUser.ChangePasswordNextLogin;

                        // Check if the user is a super admin and change the IsResellerAdmin option
                        if (this.Context.IsSuperAdmin())
                        {
                            sqlUser.IsResellerAdmin = boundUser.IsResellerAdmin;
                        }

                        logger.DebugFormat("Checking if company admin only if they have rights to change this");
                        if (this.Context.IsSuperResellerOrCompanyAdmin())
                        {
                            if (boundUser.RoleID > 0)
                            {
                                sqlUser.RoleID = boundUser.RoleID;
                                sqlUser.IsCompanyAdmin = true;
                            }
                            else
                            {
                                sqlUser.RoleID = 0;
                                sqlUser.IsCompanyAdmin = false;
                            }
                        }

                        logger.DebugFormat("Updating Active Directory for user {0}", userGuid);
                        adUser = new ADUsers(Settings.Username, Settings.DecryptedPassword, Settings.PrimaryDC);
                        adUser.UpdateUser(sqlUser);

                        #region Mailbox Changes

                        bool isExchangeEnabled = CPStaticHelpers.IsExchangeEnabled(companyCode);

                        // Check and process any mailbox changes
                        logger.DebugFormat("Checking for mailbox changes");
                        if (boundUser.IsEmailModified && isExchangeEnabled)
                        {
                            logger.DebugFormat("It appears the user loaded the email settings for {0} or set it to change.", sqlUser.UserPrincipalName);
                            boundUser.Email = string.Format("{0}@{1}", Request.Form.EmailFirst.Value, Request.Form.EmailDomain.Value);

                            ProcessMailbox(ref sqlUser, ref boundUser, ref db);                            
                        }
                        else
                            logger.DebugFormat("Email was not changed or was not enabled in Exchange for {0}", userGuid);

                        // Check for litigation hold changes
                        logger.DebugFormat("Checking for litigation hold changes");
                        if (boundUser.IsLitigationHoldModified && isExchangeEnabled)
                        {
                            ProcessLitigationHold(ref boundUser);
                        }
                        else
                            logger.DebugFormat("Litigation hold was not changed or was not enabled in Exchange for {0}", sqlUser.UserPrincipalName);

                        // Check for archive changes
                        logger.DebugFormat("Checking for archive changes");
                        if (boundUser.IsArchivingModified && isExchangeEnabled)
                        {
                            ProcessArchive(ref sqlUser, ref boundUser, ref db);
                        }
                        else
                            logger.DebugFormat("Archiving was not change or was not enabled in Exchange for {0}", sqlUser.UserPrincipalName);

                        #endregion

                        logger.DebugFormat("Saving database changes...");
                        db.SaveChanges();

                        string redirectUrl = string.Format("~/company/{0}/users", companyCode);
                        return Negotiate.WithModel(new { success = "Successfully updated user " + sqlUser.UserPrincipalName })
                                        .WithMediaRangeResponse("text/html", this.Response.AsRedirect(redirectUrl));
                    }
                }
                catch (Exception ex)
                {
                    logger.ErrorFormat("Error updating user for company {0}: {1}", _.CompanyCode, ex.ToString());
                    return Negotiate.WithModel(new { error = ex.Message })
                                    .WithView("Error/500.cshtml")
                                    .WithStatusCode(HttpStatusCode.InternalServerError);
                }
                finally
                {
                    if (adUser != null)
                        adUser.Dispose();

                    if (powershell != null)
                        powershell.Dispose();

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

                #region change a users login name
                Guid userGuid = _.UserGuid;
                string companyCode = _.CompanyCode;

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
                        Users updatedUser = adUser.ChangeLogin(userGuid, newUpn, null, user.DisplayName);

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
        }

        #region Exchange Methods

        /// <summary>
        /// Processes the mailbox for the user
        /// </summary>
        /// <param name="sqlUser"></param>
        /// <param name="boundUser"></param>
        /// <param name="db"></param>
        private void ProcessMailbox(ref Users sqlUser, ref Users boundUser, ref CloudPanelContext db)
        {
            bool wasEnabled = sqlUser.MailboxPlan > 0 ? true : false;
            bool nowEnabled = boundUser.EmailEnabledCheck;

            ReverseActions reverse = new ReverseActions();
            dynamic powershell = null;
            try
            {
                powershell = ExchPowershell.GetClass();

                string userPrincipalName = boundUser.UserPrincipalName;
                if ((!wasEnabled && nowEnabled) || (wasEnabled && nowEnabled))
                {
                    logger.DebugFormat("We are either enabling or updating the mailbox for {0}", userPrincipalName);
                    int mailboxPlan = (int)boundUser.MailboxPlan;
                    var plan = (from d in db.Plans_ExchangeMailbox
                                where d.MailboxPlanID == mailboxPlan
                                select d).FirstOrDefault();

                    if (plan == null)
                        throw new Exception("Unable to find mailbox plan " + boundUser.MailboxPlan);

                    logger.DebugFormat("Processing email addresses...");
                    var acceptedDomains = GetAcceptedDomains(boundUser.CompanyCode, ref db);
                    var emailAddresses = ValidateEmails(ref boundUser, acceptedDomains);

                    if (!wasEnabled && nowEnabled)
                    {
                        if (!CloudPanel.CPStaticHelpers.IsUnderLimit(sqlUser.CompanyCode, "mailbox"))
                            throw new Exception("You have reached the mailbox limit.");

                        // Enables mailbox
                        powershell.Enable_Mailbox(boundUser);
                        reverse.AddAction(Actions.CreateMailbox, userPrincipalName);
                    }

                    // Update mailbox settings
                    powershell.Set_Mailbox(boundUser, plan, emailAddresses.ToArray());

                    // Update CAS mailbox
                    logger.DebugFormat("New Activesync plan is {0}", boundUser.ActiveSyncPlan);
                    if (boundUser.ActiveSyncPlan > 0)
                    {
                        logger.DebugFormat("Activesync plan is not setting the default for {0}. Settings to {1}", userPrincipalName, boundUser.ActiveSyncPlan);
                        int asPlanId = boundUser.ActiveSyncPlan.Value;
                        var asPlan = (from d in db.Plans_ExchangeActiveSync
                                      where d.ASID == asPlanId
                                      select d).First();
                        powershell.Set_CASMailbox(boundUser.UserGuid, plan, asPlan);
                    }
                    else
                        powershell.Set_CASMailbox(boundUser.UserGuid, plan);

                    logger.DebugFormat("Processing full access permissions for {0}", boundUser.UserPrincipalName);
                    ProcessFullAccess(boundUser.UserGuid, ref powershell, boundUser.EmailFullAccessOriginal, boundUser.EmailFullAccess, boundUser.AutoMapping);

                    logger.DebugFormat("Processing send as permissions for {0}", boundUser.UserPrincipalName);
                    ProcessSendAs(sqlUser.UserGuid, ref powershell, boundUser.EmailSendAsOriginal, boundUser.EmailSendAs);

                    logger.DebugFormat("Updating sql data");
                    sqlUser.Email = boundUser.Email;
                    sqlUser.MailboxPlan = plan.MailboxPlanID;
                    sqlUser.ActiveSyncPlan = boundUser.ActiveSyncPlan > 0 ? boundUser.ActiveSyncPlan : 0;

                    if (plan.MailboxSizeMB > 0)
                        sqlUser.AdditionalMB = (boundUser.SizeInMB - plan.MailboxSizeMB);
                    else
                        sqlUser.AdditionalMB = 0;
                }
                else if (wasEnabled && !nowEnabled)
                {
                    #region Disable Mailbox
                    // Disable mailbox
                    powershell.Disable_Mailbox(boundUser.UserGuid);

                    // Update sql values
                    logger.DebugFormat("Updating SQL values for disabling mailbox {0}", boundUser.UserPrincipalName);
                    sqlUser.MailboxPlan = 0;
                    sqlUser.AdditionalMB = 0;
                    sqlUser.ActiveSyncPlan = 0;
                    sqlUser.Email = string.Empty;
                    sqlUser.LitigationHoldDate = string.Empty;
                    sqlUser.LitigationHoldEnabled = false;
                    sqlUser.LitigationHoldOwner = string.Empty;
                    #endregion
                }
                else
                    logger.InfoFormat("Processing mailbox returned an unknown result.");
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error processing mailbox for {0}: {1}", sqlUser.UserPrincipalName, ex.ToString());
                reverse.RollbackNow();
                throw;
            }
            finally
            {
                if (powershell != null)
                    powershell.Dispose();
            }
        }

        /// <summary>
        /// Processes the litigation hold settings for the user
        /// </summary>
        /// <param name="boundUser"></param>
        private void ProcessLitigationHold(ref Users boundUser)
        {
            logger.DebugFormat("Processing litigation hold section...");
            dynamic powershell = null;
            try
            {
                logger.DebugFormat("Litigation hold page was loaded. Updating values");
                powershell = ExchPowershell.GetClass();
                powershell.Set_LitigationHold(boundUser.UserGuid, boundUser.LitigationHoldEnabled, boundUser.RetentionUrl, boundUser.RetentionComment);
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error processing litigation hold for {0}: {1}", boundUser.UserPrincipalName, ex.ToString());
                throw;
            }
            finally
            {
                if (powershell != null)
                    powershell.Dispose();
            }
        }

        /// <summary>
        /// Processes the archive mailbox settings for the user
        /// </summary>
        /// <param name="sqlUser"></param>
        /// <param name="boundUser"></param>
        /// <param name="db"></param>
        private void ProcessArchive(ref Users sqlUser, ref Users boundUser, ref CloudPanelContext db)
        {
            logger.DebugFormat("Processing Exchange archiving");
            dynamic powershell = null;
            ReverseActions reverse = new ReverseActions();
            try
            {
                bool wasEnabled = sqlUser.ArchivePlan > 0 ? true : false;
                bool nowEnabled = boundUser.ArchivingEnabledChecked;

                powershell = ExchPowershell.GetClass();
                if ((!wasEnabled && nowEnabled) || (wasEnabled && nowEnabled))
                {
                    logger.DebugFormat("We are either enabling or updating the archive mailbox for {0}", boundUser.UserPrincipalName);
                    int archivePlan = (int)boundUser.ArchivePlan;
                    var plan = (from d in db.Plans_ExchangeArchiving
                                where d.ArchivingID == archivePlan
                                select d).First();

                    if (plan == null)
                        throw new Exception("Unable to find archive mailbox plan " + archivePlan);

                    if (!wasEnabled && nowEnabled) // Enabling archive 
                    {
                        powershell.Enable_ArchiveMailbox(sqlUser.UserGuid, boundUser.ArchiveName, plan.Database);
                        reverse.AddAction(Actions.CreateArchiveMailbox, sqlUser.UserGuid);

                        powershell.Set_ArchiveMailbox(sqlUser.UserGuid, plan.ArchiveSizeMB);
                    }
                    else // Update archive
                        powershell.Set_ArchiveMailbox(boundUser.UserGuid, plan.ArchiveSizeMB);

                    logger.DebugFormat("Updating sql data");
                    sqlUser.ArchivePlan = plan.ArchivingID;
                }
                else if (wasEnabled && !nowEnabled)
                {
                    #region Disable Archive Mailbox

                    // Disable mailbox
                    powershell.Disable_ArchiveMailbox(boundUser.UserGuid);

                    // Update sql values
                    logger.DebugFormat("Updating SQL values for disabling archive mailbox {0}", boundUser.UserPrincipalName);
                    sqlUser.ArchivePlan = 0;
                    #endregion
                }
                else
                    logger.InfoFormat("Processing archive mailbox returned an unknown result.");
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error processing litigation hold for {0}: {1}", boundUser.UserPrincipalName, ex.ToString());
                reverse.RollbackNow();

                throw;
            }
            finally
            {
                if (powershell != null)
                    powershell.Dispose();
            }
        }

        /// <summary>
        /// Adds or removes full access permissions
        /// </summary>
        /// <param name="upn"></param>
        /// <param name="powershell"></param>
        /// <param name="original"></param>
        /// <param name="current"></param>
        /// <param name="autoMapping"></param>
        private void ProcessFullAccess(Guid userGuid, ref dynamic powershell, string[] original, string[] current, bool autoMapping = true)
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
                powershell.Add_FullAccessPermissions(userGuid, toAdd.ToArray(), autoMapping);

            if (toRemove != null && toRemove.Count() > 0)
                powershell.Remove_FullAccessPermissions(userGuid, toRemove.ToArray());
        }

        /// <summary>
        /// Adds or removes send as permissions
        /// </summary>
        /// <param name="distinguishedName"></param>
        /// <param name="powershell"></param>
        /// <param name="original"></param>
        /// <param name="current"></param>
        private void ProcessSendAs(Guid userGuid, ref dynamic powershell, string[] original, string[] current)
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
                powershell.Add_SendAsPermissions(userGuid, toAdd.ToArray());

            if (toRemove != null && toRemove.Count() > 0)
                powershell.Remove_SendAsPermissions(userGuid, toRemove.ToArray());
        }

        /// <summary>
        /// Validates that the emails end with a domain the company has access to
        /// </summary>
        /// <param name="boundUser"></param>
        /// <param name="validDomains"></param>
        /// <returns></returns>
        private List<string> ValidateEmails(ref Users boundUser, List<Domains> validDomains)
        {
            var validatedList = new List<string>() { "SMTP:" + boundUser.Email };

            if (boundUser.EmailAliases != null)
            {
                foreach (var email in boundUser.EmailAliases)
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
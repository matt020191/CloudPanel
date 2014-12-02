using Nancy;
using Nancy.Security;
using Nancy.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using log4net;
using CloudPanel.Database.EntityFramework;
using CloudPanel.Base.Config;
using CloudPanel.Exchange;
using System.Text;
using CloudPanel.Rollback;

namespace CloudPanel.Modules.CompanyModules.Exchange
{
    public class BulkModule : NancyModule
    {
        private static readonly ILog logger = log4net.LogManager.GetLogger(typeof(BulkModule));

        private enum ActionToTake
        {
            Enable = 0,
            Change = 2,
            Disable = 1
        }

        public enum EmailFormat
        {
            Username = 0,
            Firstname = 1,
            Lastname = 2,
            FirstNameDotLastname = 3,
            FirstnameLastname = 4,
            LastnameDotFirstname = 5,
            LastnameFirstname = 6,
            FirstInitialLastname = 7,
            LastnameFirstInitial = 8
        }

        public BulkModule() : base("/company/{CompanyCode}/exchange/bulk")
        {
            this.RequiresAuthentication();

            Get["/", c => c.Request.Accept("text/html")] = _ =>
            {
                return View["Company/Exchange/Bulk.cshtml"];
            };

            Post["/"] = _ =>
            {
                try
                {
                    string companyCode = _.CompanyCode;

                    if (!Request.Form.CheckedUsers.HasValue)
                        throw new Exception("No users were selected.");

                    if (!Request.Form.ActionToTake.HasValue)
                        throw new Exception("No action was selected.");

                    logger.DebugFormat("Unparsed users is {0}", Request.Form.CheckedUsers.Value);
                    string users = Request.Form.CheckedUsers.Value;
                    string[] userPrincipalNames = users.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                    // For our results
                    Dictionary<string, string> results = new Dictionary<string,string>();

                    ActionToTake action = Enum.Parse(typeof(ActionToTake), Request.Form.ActionToTake.Value);
                    switch (action)
                    {
                        case ActionToTake.Disable:
                            logger.DebugFormat("Disable action was taken");
                            results = DisableMailboxes(userPrincipalNames, companyCode);
                            break;
                        case ActionToTake.Enable:
                            logger.DebugFormat("Enable action was taken");
                            if (!Request.Form.MailboxPlan.HasValue)
                                throw new MissingFieldException("", "MailboxPlan");

                            if (!Request.Form.EmailFormat.HasValue)
                                throw new MissingFieldException("", "EmailFormat");

                            if (!Request.Form.EmailDomain.HasValue)
                                throw new MissingFieldException("", "EmailDomain");

                            if (!Request.Form.SizeInMB.HasValue)
                                throw new MissingFieldException("", "SizeInMB");

                            EmailFormat format = Enum.Parse(typeof(EmailFormat), Request.Form.EmailFormat.Value);
                            results = EnableMailboxes(userPrincipalNames, companyCode, format, Request.Form.MailboxPlan, Request.Form.SizeInMB, Request.Form.EmailDomain);
                            break;
                        case ActionToTake.Change:
                            logger.DebugFormat("Change action was taken.. Checking Litigation Hold settings");
                            bool changeLitigationHold = Request.Form.cbChangeLitigationHold.HasValue ? (bool)Request.Form.cbChangeLitigationHold : false;
                            bool changeLitigationUrl = Request.Form.cbChangeLitigationHoldUrl.HasValue ? (bool)Request.Form.cbChangeLitigationHoldUrl : false;
                            bool changeLitigationComment = Request.Form.cbChangeLitigationHoldComment.HasValue ? (bool)Request.Form.cbChangeLitigationHoldComment : false;

                            if (changeLitigationHold || changeLitigationUrl || changeLitigationComment)
                            {
                                logger.DebugFormat("We are updating litigation hold information. Hold: {0}, Url: {1}, Comment: {2}", changeLitigationHold, changeLitigationUrl, changeLitigationComment);
                                results = ModifyLitigationHold(userPrincipalNames, companyCode, 
                                                               (changeLitigationHold == true ? (bool?)Request.Form.LitigationHoldEnabled : null),
                                                               (changeLitigationUrl == true ? Request.Form.cbChangeLitigationHoldUrl.Value : null),
                                                               (changeLitigationComment == true ? Request.Form.RetentionComment.Value : null));

                             
                            }

                            logger.DebugFormat("Checking for Archive Mailbox changes");
                            bool changeArchive = Request.Form.cbChangeArchiving.HasValue ? (bool)Request.Form.cbChangeArchiving : false;
                            bool changeArchiveName = Request.Form.cbChangeArchiveName.HasValue ? (bool)Request.Form.cbChangeArchiveName : false;
                            bool changeArchivePlan = Request.Form.cbChangeArchivePlan.HasValue ? (bool)Request.Form.cbChangeArchivePlan : false;

                            if (changeArchive || changeArchiveName || changeArchivePlan)
                            {
                                logger.DebugFormat("We are updating archive mailbox information. Archive: {0}, Name: {1}, Plan: {2}", changeArchive, changeArchiveName, changeArchivePlan);
                                
                                Dictionary<string, string> r2 = ModifyArchiveMailbox(userPrincipalNames, companyCode,
                                                                                    (changeArchive == true ? (bool?)Request.Form.ArchivingEnabledChecked : null),
                                                                                    (changeArchiveName == true ? Request.Form.ArchiveName.Value : null),
                                                                                    (changeArchivePlan == true || changeArchive == true) ? (int?)Request.Form.ArchivePlan : null);

                                results = results.Union(r2).ToDictionary(k => k.Key, v => v.Value);
                            }

                            break;
                        default:
                            throw new Exception("Unknown action was specified");
                    }


                    return Negotiate.WithModel(new { success = "Bulk action has successfully completed.", results = results })
                                    .WithView("success.cshtml");
                }
                catch (Exception ex)
                {
                    logger.ErrorFormat("Error bulk updating mailboxes: {0}", ex.ToString());

                    ViewBag.error = ex.ToString();
                    return Negotiate.WithModel(new { error = ex.Message })
                                    .WithView("error.cshtml");
                }
            };
        }

        /// <summary>
        /// Disables all the selected mailboxes
        /// </summary>
        /// <param name="userPrincipalNames"></param>
        /// <param name="companyCode"></param>
        private Dictionary<string, string> DisableMailboxes(string[] userPrincipalNames, string companyCode)
        {
            var results = new Dictionary<string, string>();

            CloudPanelContext db = null;
            dynamic powershell = null;
            try
            {
                db = new CloudPanelContext(Settings.ConnectionString);
                db.Database.Connection.Open();

                powershell = ExchPowershell.GetClass();
                foreach (var u in userPrincipalNames)
                {
                    try
                    {

                        logger.DebugFormat("Validating user {0} for {1} for disabling mailboxes", u, companyCode);
                        if (!string.IsNullOrEmpty(u))
                        {
                            logger.DebugFormat("User {0} passed validating... Checking SQL", u);
                            var sqlUser = (from d in db.Users
                                           where d.CompanyCode == companyCode
                                           where d.UserPrincipalName == u
                                           select d).FirstOrDefault();

                            if (sqlUser != null)
                            {
                                if (sqlUser.MailboxPlan > 0)
                                {
                                    logger.DebugFormat("Running diasble command for {0}", u);
                                    powershell.Disable_Mailbox(u);

                                    logger.DebugFormat("Updating SQL values for {0}", u);
                                    sqlUser.MailboxPlan = 0;
                                    sqlUser.AdditionalMB = 0;
                                    sqlUser.ActiveSyncPlan = 0;
                                    sqlUser.Email = string.Empty;
                                    sqlUser.LitigationHoldDate = string.Empty;
                                    sqlUser.LitigationHoldEnabled = false;
                                    sqlUser.LitigationHoldOwner = string.Empty;
                                    db.SaveChanges();

                                    results.Add(u, "SUCCESS");
                                }
                            }
                            else
                                throw new Exception("User was not found in database: " + u);
                        }
                    }
                    catch (Exception ex)
                    {
                        results.Add(u, "FAILED: " + ex.Message);
                    }
                }

                // Return results back to the calling method
                return results;
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error disabling mailboxes: {0}", ex.ToString());
                throw;
            }
            finally
            {
                if (powershell != null)
                    powershell.Dispose();

                if (db != null)
                    db.Dispose();

            }
        }

        /// <summary>
        /// Enables mailboxes
        /// </summary>
        /// <param name="userPrincipalNames"></param>
        /// <param name="companyCode"></param>
        /// <param name="format"></param>
        /// <param name="mailboxPlanId"></param>
        /// <param name="sizeInMB"></param>
        /// <param name="domainId"></param>
        /// <returns></returns>
        private Dictionary<string, string> EnableMailboxes(string[] userPrincipalNames, string companyCode, EmailFormat format, int mailboxPlanId, int sizeInMB, int domainId)
        {
            var results = new Dictionary<string, string>();

            CloudPanelContext db = null;
            dynamic powershell = null;
            try
            {
                db = new CloudPanelContext(Settings.ConnectionString);
                db.Database.Connection.Open();

                //
                // Get Exchange mailbox plan selected
                //
                logger.DebugFormat("Getting mailbox plan {0} from the database", mailboxPlanId);
                var mailboxPlan = (from d in db.Plans_ExchangeMailbox
                                   where d.MailboxPlanID == mailboxPlanId
                                   select d).FirstOrDefault();

                if (mailboxPlan == null)
                    throw new Exception("Unable to find mailbox plan in database " + mailboxPlanId);

                // 
                // Get the domain from the database
                //
                logger.DebugFormat("Getting domain {0} from the database", domainId);
                var domain = (from d in db.Domains
                              where d.DomainID == domainId
                              where d.IsAcceptedDomain == true
                              select d).FirstOrDefault();

                if (domain == null)
                    throw new Exception("Unable to find domain in the database " + domainId);

                powershell = ExchPowershell.GetClass();
                foreach (var u in userPrincipalNames)
                {
                    ReverseActions reverse = new ReverseActions();
                    try
                    {
                        //
                        // Get the user from the database
                        //
                        logger.DebugFormat("Getting user {0} from the database", u);
                        var sqlUser = (from d in db.Users
                                       where d.UserPrincipalName == u
                                       where d.CompanyCode == companyCode
                                       select d).FirstOrDefault();

                        if (sqlUser == null)
                            throw new Exception("User was not found in database");

                        if (sqlUser.MailboxPlan > 0)
                            throw new Exception("User already has a mailbox");

                        if (!CloudPanel.CPStaticHelpers.IsUnderLimit(companyCode, "mailbox"))
                            throw new Exception("You have reached the mailbox limit");

                        logger.DebugFormat("Getting user's email address based on options selected. Selected {0}", format.ToString());
                        switch (format)
                        {
                            #region Switch Formats for Email
                            case EmailFormat.Firstname:
                                if (string.IsNullOrEmpty(sqlUser.Firstname))
                                    throw new Exception("Cannot use first name format because user " + u + " does not contain a first name in the database");

                                sqlUser.Email = string.Format("{0}@{1}", sqlUser.Firstname.Replace(" ", string.Empty), domain.Domain);
                                break;
                            case EmailFormat.Lastname:
                                if (string.IsNullOrEmpty(sqlUser.Lastname))
                                    throw new Exception("Cannot use last name format because user " + u + " does not contain a last name in the database");

                                sqlUser.Email = string.Format("{0}@{1}", sqlUser.Lastname.Replace(" ", string.Empty), domain.Domain);
                                break;
                            case EmailFormat.FirstNameDotLastname:
                                if (string.IsNullOrEmpty(sqlUser.Firstname) || string.IsNullOrEmpty(sqlUser.Lastname))
                                    throw new Exception("Cannot use firstname.lastname format because user " + u + " does not contain a firstname or lastname in the database");

                                sqlUser.Email = string.Format("{0}.{1}@{2}", sqlUser.Firstname.Replace(" ", string.Empty), sqlUser.Lastname.Replace(" ", string.Empty), domain.Domain);
                                break;
                            case EmailFormat.FirstnameLastname:
                                if (string.IsNullOrEmpty(sqlUser.Firstname) || string.IsNullOrEmpty(sqlUser.Lastname))
                                    throw new Exception("Cannot use firstnamelastname format because user " + u + " does not contain a firstname or lastname in the database");

                                sqlUser.Email = string.Format("{0}{1}@{2}", sqlUser.Firstname.Replace(" ", string.Empty), sqlUser.Lastname.Replace(" ", string.Empty), domain.Domain);
                                break;
                            case EmailFormat.LastnameDotFirstname:
                                if (string.IsNullOrEmpty(sqlUser.Firstname) || string.IsNullOrEmpty(sqlUser.Lastname))
                                    throw new Exception("Cannot use lastname.firstname format because user " + u + " does not contain a firstname or lastname in the database");

                                sqlUser.Email = string.Format("{0}.{1}@{2}", sqlUser.Lastname.Replace(" ", string.Empty), sqlUser.Firstname.Replace(" ", string.Empty), domain.Domain);
                                break;
                            case EmailFormat.LastnameFirstname:
                                if (string.IsNullOrEmpty(sqlUser.Firstname) || string.IsNullOrEmpty(sqlUser.Lastname))
                                    throw new Exception("Cannot use lastnamefirstname format because user " + u + " does not contain a firstname or lastname in the database");

                                sqlUser.Email = string.Format("{0}{1}@{2}", sqlUser.Lastname.Replace(" ", string.Empty), sqlUser.Firstname.Replace(" ", string.Empty), domain.Domain);
                                break;
                            case EmailFormat.FirstInitialLastname:
                                if (string.IsNullOrEmpty(sqlUser.Firstname) || string.IsNullOrEmpty(sqlUser.Lastname))
                                    throw new Exception("Cannot use FirstInitialLastname format because user " + u + " does not contain a firstname or lastname in the database");

                                sqlUser.Email = string.Format("{0}{1}@{2}", sqlUser.Firstname.Replace(" ", string.Empty).Substring(0,1), sqlUser.Lastname.Replace(" ", string.Empty), domain.Domain);
                                break;
                            case EmailFormat.LastnameFirstInitial:
                                if (string.IsNullOrEmpty(sqlUser.Firstname) || string.IsNullOrEmpty(sqlUser.Lastname))
                                    throw new Exception("Cannot use LastnameFirstInitial format because user " + u + " does not contain a firstname or lastname in the database");

                                sqlUser.Email = string.Format("{0}{1}@{2}", sqlUser.Lastname.Replace(" ", string.Empty), sqlUser.Firstname.Replace(" ", string.Empty).Substring(0,1), domain.Domain);
                                break;
                            default:
                                sqlUser.Email = string.Format("{0}@{1}", sqlUser.UserPrincipalName.Split('@')[0], domain.Domain);
                                break;
                            #endregion
                        }

                        logger.DebugFormat("Setting user {0}'s mailbox size to {1}", u, sizeInMB);
                        sqlUser.SizeInMB = sizeInMB;

                        logger.DebugFormat("Enabling user's mailbox {0} with email {1}", u, sqlUser.Email);
                        powershell.Enable_Mailbox(sqlUser);
                        reverse.AddAction(Actions.CreateMailbox, u);

                        logger.DebugFormat("Setting mailbox properties for {0}", u);
                        powershell.Set_Mailbox(sqlUser, mailboxPlan, new string[] { "SMTP:" + sqlUser.Email });

                        logger.DebugFormat("Setting CAS mailbox properties for {0}", u);
                        powershell.Set_CASMailbox(u, mailboxPlan);

                        logger.DebugFormat("Successfully enabled {0} mailbox.. updating database", u);
                        sqlUser.MailboxPlan = mailboxPlanId;
                        sqlUser.AdditionalMB = sizeInMB > 0 ? (sizeInMB - mailboxPlan.MailboxSizeMB) : 0;
                        db.SaveChanges();

                        results.Add(u, "SUCCESS");
                    }
                    catch (Exception ex)
                    {
                        logger.ErrorFormat("Error enabling mailbox for {0}: {1}", u, ex.ToString());
                        results.Add(u, ex.Message);

                        reverse.RollbackNow();
                    }
                }

                // Return our results to the calling method to display to the user
                return results;
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error enabling mailboxes: {0}", ex.ToString());
                throw;
            }
            finally
            {
                if (powershell != null)
                    powershell.Dispose();

                if (db != null)
                    db.Dispose();
            }
        }

        /// <summary>
        /// Updates the litigation hold section
        /// </summary>
        /// <param name="userPrincipalNames"></param>
        /// <param name="companyCode"></param>
        /// <param name="isEnabled"></param>
        /// <param name="url"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        private Dictionary<string, string> ModifyLitigationHold(string[] userPrincipalNames, string companyCode, bool? isEnabled = null, string url = null, string message = null)
        {
            var results = new Dictionary<string, string>();

            CloudPanelContext db = null;
            dynamic powershell = null;
            try
            {
                db = new CloudPanelContext(Settings.ConnectionString);
                db.Database.Connection.Open();

                powershell = ExchPowershell.GetClass();
                foreach (var u in userPrincipalNames)
                {
                    string resultName = string.Format("{0} [Litigation Hold]", u);
                    try
                    {
                        //
                        // Get the user from the database
                        //
                        logger.DebugFormat("Getting user {0} from the database", u);
                        var sqlUser = (from d in db.Users
                                       where d.UserPrincipalName == u
                                       where d.CompanyCode == companyCode
                                       where d.MailboxPlan > 0
                                       select d).FirstOrDefault();

                        if (sqlUser == null)
                            results.Add(resultName, "User does not appear to have a mailbox");
                        else
                        {
                            powershell.Set_LitigationHold(u, litigationHoldEnabled: isEnabled, retentionUrl: url, retentionComment: message);
                            results.Add(resultName, "Successfully updated litigation hold settings");
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.ErrorFormat("Error updating litigation hold settings for {0}: {1}", u, ex.ToString());
                        results.Add(resultName, ex.Message);
                    }
                }

                // Return our results to the calling method to display to the user
                return results;
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error modifying litigation hold: {0}", ex.ToString());
                throw;
            }
            finally
            {
                if (powershell != null)
                    powershell.Dispose();

                if (db != null)
                    db.Dispose();
            }
        }

        /// <summary>
        /// Makes modifications to archive mailbox
        /// </summary>
        /// <param name="userPrincipalNames"></param>
        /// <param name="companyCode"></param>
        /// <param name="isEnabled"></param>
        /// <param name="archiveName"></param>
        /// <param name="archivePlan"></param>
        /// <returns></returns>
        private Dictionary<string, string> ModifyArchiveMailbox(string[] userPrincipalNames, string companyCode, bool? isEnabled = null, string archiveName = null, int? archivePlan = null)
        {
            var results = new Dictionary<string, string>();

            CloudPanelContext db = null;
            dynamic powershell = null;
            try
            {
                db = new CloudPanelContext(Settings.ConnectionString);
                db.Database.Connection.Open();

                int? archivePlanSize = null;
                if (archivePlan != null)
                    archivePlanSize = (from d in db.Plans_ExchangeArchiving
                                       where d.ArchivingID == archivePlan
                                       where string.IsNullOrEmpty(d.CompanyCode) || d.CompanyCode == companyCode
                                       select d.ArchiveSizeMB).First();

                powershell = ExchPowershell.GetClass();
                foreach (var u in userPrincipalNames)
                {
                    string resultName = string.Format("{0} [Archive]", u);
                    try
                    {
                        //
                        // Get the user from the database
                        //
                        logger.DebugFormat("Getting user {0} from the database", u);
                        var sqlUser = (from d in db.Users
                                       where d.UserPrincipalName == u
                                       where d.CompanyCode == companyCode
                                       where d.MailboxPlan > 0
                                       select d).FirstOrDefault();

                        if (sqlUser == null)
                            results.Add(resultName, "User does not appear to have a mailbox");
                        else
                        {
                            if (isEnabled == true) // Archiving is being enabled
                            {
                                logger.DebugFormat("Enabling archive mailbox for {0}", u);
                                powershell.Enable_ArchiveMailbox(u, archiveName, string.Empty);
                            }
                            else if (isEnabled == false) // Archiving is being disabled
                            {
                                logger.DebugFormat("Disabling archive mailbox for {0}", u);
                                powershell.Disable_ArchiveMailbox(u);
                                sqlUser.ArchivePlan = 0;
                                db.SaveChanges();
                            }

                            if (archiveName != null || archivePlanSize != null) // The name or plan has been changed.
                            {
                                logger.DebugFormat("Updating archive settings for {0}", u);
                                powershell.Set_ArchiveMailbox(u, archiveName, archivePlanSize);
                                sqlUser.ArchivePlan = (int)archivePlan;
                                db.SaveChanges();
                            }

                            results.Add(resultName, "Successfully updated archive settings");
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.ErrorFormat("Error updating archive for {0}: {1}", u, ex.ToString());
                        results.Add(resultName, ex.Message);
                    }
                }

                // Return our results to the calling method to display to the user
                return results;
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error enabling mailboxes: {0}", ex.ToString());
                throw;
            }
            finally
            {
                if (powershell != null)
                    powershell.Dispose();

                if (db != null)
                    db.Dispose();
            }
        }
    }
}
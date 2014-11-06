﻿using CloudPanel.Base.Config;
using CloudPanel.Base.Database.Models;
using CloudPanel.Database.EntityFramework;
using CloudPanel.Exchange;
using log4net;
using Nancy;
using Nancy.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CloudPanel.Modules.CompanyModules
{
    public class UsersEditModule : NancyModule
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(UsersEditModule));

        public UsersEditModule() : base("/company/{CompanyCode}/users/{UserPrincipalName}")
        {
            Get["/", c => c.Request.Accept("text/html")] = _ =>
            {
                logger.DebugFormat("Loading user edit page for company {0} and user {1}", _.CompanyCode, _.UserPrincipalName);

                string companyCode = _.CompanyCode;
                string userPrincipalName = _.UserPrincipalName;
                return View["Company/users_edit.cshtml", new { 
                    CompanyCode = companyCode, 
                    UserPrincipalName = userPrincipalName, 
                    MailboxUsers = UsersModule.GetMailboxUsers(companyCode) 
                }];
            };

            Get["/", c => c.Request.Accept("application/json")] = _ =>
            {
                #region Gets a specific user
                CloudPanelContext db = null;
                try
                {
                    db = new CloudPanelContext(Settings.ConnectionString);
                    db.Database.Connection.Open();

                    logger.DebugFormat("Getting user {0} from the system", _.UserPrincipalName);
                    string companyCode = _.CompanyCode;
                    string upn = _.UserPrincipalName;

                    logger.DebugFormat("Querying the database for {0}", upn);
                    var user = (from d in db.Users
                                where d.CompanyCode == companyCode
                                where d.UserPrincipalName == upn
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
                    logger.ErrorFormat("Error getting user {0}: {1}", _.UserPrincipalName, ex.ToString());
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
                #region Gets a specific user's mailbox
                dynamic powershell = null;
                try
                {
                    string companyCode = _.CompanyCode;
                    string upn = _.UserPrincipalName;

                    logger.DebugFormat("Getting mailbox {0} from Exchange", _.UserPrincipalName);
                    powershell = ExchPowershell.GetClass();

                    var mailbox = powershell.Get_Mailbox(new Users() { UserPrincipalName = upn });
                    return Negotiate.WithModel(new { mailbox = mailbox })
                                    .WithStatusCode(HttpStatusCode.OK);
                }
                catch (Exception ex)
                {
                    logger.ErrorFormat("Error getting mailbox {0}: {1}", _.UserPrincipalName, ex.ToString());
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
                #region Updates a user

                CloudPanelContext db = null;
                dynamic powershell = null;
                try
                {
                    string companyCode = _.CompanyCode;
                    string userPrincipalName = _.UserPrincipalName;

                    logger.DebugFormat("Retrieving user from database");
                    db = new CloudPanelContext(Settings.ConnectionString);
                    db.Database.Connection.Open();

                    var sqlUser = (from d in db.Users
                                   where d.CompanyCode == companyCode
                                   where d.UserPrincipalName == userPrincipalName
                                   select d).FirstOrDefault();

                    if (sqlUser == null)
                        throw new Exception("Unable to find user in the database: " + userPrincipalName);
                    else
                    {
                        // Model bind
                        logger.DebugFormat("Binding form to class...");
                        var boundUser = this.Bind<Users>();
                        boundUser.UserPrincipalName = userPrincipalName;
                        boundUser.CompanyCode = companyCode;

                        // Combine the old values in sql with the new values in sql
                        logger.DebugFormat("Updating database values");
                        Combine(ref sqlUser, ref boundUser);

                        #region Mailbox Changes

                        bool isExchangeEnabled = CPStaticHelpers.IsExchangeEnabled(companyCode);

                        // Check and process any mailbox changes
                        logger.DebugFormat("Checking for mailbox changes");
                        if (boundUser.IsEmailModified && isExchangeEnabled)
                        {
                            logger.DebugFormat("It appears the user loaded the email settings for {0} or set it to change.", userPrincipalName);
                            boundUser.Email = string.Format("{0}@{1}", Request.Form.EmailFirst.Value, Request.Form.EmailDomain.Value);

                            ProcessMailbox(ref sqlUser, ref boundUser, ref db);                            
                        }
                        else
                            logger.DebugFormat("Email was not changed or was not enabled in Exchange for {0}", userPrincipalName);

                        logger.DebugFormat("Checking for litigation hold changes");
                        if (boundUser.IsLitigationHoldModified && isExchangeEnabled && sqlUser.MailboxPlan > 0)
                        {
                            ProcessLitigationHold(ref boundUser);
                        }
                        else
                            logger.DebugFormat("Litigation hold was not changed or was not enabled in Exchange for {0}", userPrincipalName);

                        #endregion
                    }

                    logger.DebugFormat("Saving database changes...");
                    db.SaveChanges();

                    string redirectUrl = string.Format("~/company/{0}/users", companyCode);
                    return Negotiate.WithModel(new { success = "Successfully updated user " + userPrincipalName })
                                    .WithMediaRangeResponse("text/html", this.Response.AsRedirect(redirectUrl));
                }
                catch (Exception ex)
                {
                    logger.ErrorFormat("Error creating new user for company {0}: {1}", _.CompanyCode, ex.ToString());

                    ViewBag.error = ex.Message;
                    return Negotiate.WithModel(new { error = ex.Message })
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

            dynamic powershell = null;
            try
            {
                powershell = ExchPowershell.GetClass();

                if ((!wasEnabled && nowEnabled) || (wasEnabled && nowEnabled))
                {
                    logger.DebugFormat("We are either enabling or updating the mailbox for {0}", boundUser.UserPrincipalName);
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
                        powershell.Enable_Mailbox(boundUser, plan, emailAddresses.ToArray());
                    else
                        powershell.Set_Mailbox(boundUser, plan, emailAddresses.ToArray());

                    logger.DebugFormat("Processing full access permissions for {0}", boundUser.UserPrincipalName);
                    ProcessFullAccess(boundUser.UserPrincipalName, ref powershell, boundUser.EmailFullAccessOriginal, boundUser.EmailFullAccess, true);

                    logger.DebugFormat("Processing send as permissions for {0}", boundUser.UserPrincipalName);
                    ProcessSendAs(sqlUser.DistinguishedName, ref powershell, boundUser.EmailSendAsOriginal, boundUser.EmailSendAs);

                    logger.DebugFormat("Updating sql data");
                    sqlUser.Email = boundUser.Email;
                    sqlUser.MailboxPlan = plan.MailboxPlanID;

                    if (plan.MailboxSizeMB > 0)
                        sqlUser.AdditionalMB = (boundUser.SizeInMB - plan.MailboxSizeMB);
                    else
                        sqlUser.AdditionalMB = 0;
                }
                else if (wasEnabled && !nowEnabled)
                {
                    #region Disable Mailbox
                    // Disable mailbox
                    powershell.Disable_Mailbox(boundUser.UserPrincipalName);

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
                throw;
            }
            finally
            {
                if (powershell != null)
                    powershell.Dispose();
            }
        }

        private void ProcessLitigationHold(ref Users boundUser)
        {
            logger.DebugFormat("Processing litigation hold section...");
            dynamic powershell = null;
            try
            {
                powershell = ExchPowershell.GetClass();

                logger.DebugFormat("Now checking litigation hold");
                if (boundUser.IsLitigationHoldModified)
                {
                    logger.DebugFormat("Litigation hold page was loaded. Updating values");
                    powershell.Set_LitigationHold(boundUser.UserPrincipalName, boundUser.LitigationHoldEnabled, boundUser.RetentionUrl, boundUser.RetentionComment);
                }
                else
                    logger.DebugFormat("Litigation hold was not modified");
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

        #endregion

        #region Combine Values

        /// <summary>
        /// Combines the second parameter into the first parameter (Users object)
        /// </summary>
        /// <param name="sqlUser"></param>
        /// <param name="updatedUser"></param>
        private void Combine(ref Users sqlUser, ref Users updatedUser)
        {
            sqlUser.Firstname = updatedUser.Firstname;
            sqlUser.Middlename = updatedUser.Middlename;
            sqlUser.Lastname = updatedUser.Lastname;
            sqlUser.DisplayName = updatedUser.DisplayName;
            sqlUser.Company = updatedUser.Company;
            sqlUser.Department = updatedUser.Department;
            sqlUser.JobTitle = updatedUser.JobTitle;
            sqlUser.TelephoneNumber = updatedUser.TelephoneNumber;
            sqlUser.Fax = updatedUser.Fax;
            sqlUser.HomePhone = updatedUser.HomePhone;
            sqlUser.MobilePhone = updatedUser.MobilePhone;
            sqlUser.Street = updatedUser.Street;
            sqlUser.City = updatedUser.City;
            sqlUser.State = updatedUser.State;
            sqlUser.PostalCode = updatedUser.PostalCode;
            sqlUser.Country = updatedUser.Country;
        }

        #endregion
    }
}
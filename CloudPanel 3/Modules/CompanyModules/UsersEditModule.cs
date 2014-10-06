using CloudPanel.ActiveDirectory;
using CloudPanel.Base.Config;
using CloudPanel.Database.EntityFramework;
using CloudPanel.Exchange;
using log4net;
using Nancy;
using Nancy.ModelBinding;
using Nancy.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

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
                return View["Company/users_edit.cshtml", new { CompanyCode = companyCode, UserPrincipalName = userPrincipalName }];
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

            /*
            Get["/"] = _ =>
            {
                #region Gets a specific user
                CloudPanelContext db = null;
                dynamic powershell = null;
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
                        logger.DebugFormat("Querying Exchange information for {0}", upn);
                        if (user.MailboxPlan > 0)
                        {
                            logger.DebugFormat("User {0} has a mailbox. Getting the mailbox plan", upn);
                            var plan = (from d in db.Plans_ExchangeMailbox
                                        where d.MailboxPlanID == user.MailboxPlan
                                        select d).FirstOrDefault();

                            if (plan == null)
                                throw new Exception("Unable to find mailbox plan " + user.MailboxPlan + " in the database");
                            else
                            {
                                logger.DebugFormat("Setting user Exchange attributes");
                                user.SizeInMB = plan.MailboxSizeMB + (user.AdditionalMB == null ? 0 : (int)user.AdditionalMB); // Add the default plan size to the additional

                                logger.DebugFormat("Opening connection to Exchange");
                                powershell = ExchPowershell.GetClass();
                            }
                        }
                        else
                            logger.DebugFormat("User is not enabled for Exchange... skip connecting...");

                        return Negotiate.WithModel(new { user = user })
                                        .WithView("Company/users_edit.cshtml");
                    }
                }
                catch (Exception ex)
                {
                    logger.ErrorFormat("Error getting user {0}: {1}", _.UserPrincipalName, ex.ToString());
                    return Negotiate.WithModel(new { error = ex.Message })
                                    .WithView("error.cshtml");
                }
                finally
                {
                    if (db != null)
                        db.Dispose();
                }
                #endregion
            };

            Put["/{UserPrincipalName}"] = _ =>
            {
                #region Updates an existing user

                CloudPanelContext db = null;
                ADUsers adUsers = null;
                try
                {
                    db = new CloudPanelContext(Settings.ConnectionString);
                    db.Database.Connection.Open();

                    string upn = _.UserPrincipalName;
                    string companyCode = _.CompanyCode;

                    logger.DebugFormat("Getting user {0} from the database", upn);
                    var existingUser = (from d in db.Users
                                        where d.CompanyCode == companyCode
                                        where d.UserPrincipalName == upn
                                        select d).First();

                    if (existingUser == null)
                        throw new Exception("Unable to find user in the database: " + upn);
                    else
                    {
                        // Bind user to form
                        logger.DebugFormat("Binding form to object..");
                        var updatedUser = this.Bind<Users>();

                        #region Active Directory
                        UpdateAdUser(ref updatedUser, upn); // Updates user in AD
                        Combine(ref existingUser, ref updatedUser); // Combines the new values into the old values to save to database
                        db.SaveChanges();
                        #endregion

                        #region Mailbox
                        logger.DebugFormat("Checking if Exchange is enabled for {0}", companyCode);
                        bool isExchangeEnabled = CPStaticHelpers.IsExchangeEnabled(companyCode);

                        if (isExchangeEnabled)
                        {
                            logger.DebugFormat("Entered the Exchange section for {0}", upn);
                            var emailInfo = this.Bind<EmailInfo>();

                            if (!emailInfo.IsEmailEnabled) // We are disabling the mailbox or not changing
                                UpdateMailbox(ref existingUser, emailInfo, null);
                            else
                            {
                                logger.DebugFormat("Validating email settings...");
                                if (emailInfo.MailboxPlanID == 0)
                                    throw new Exception("Invalid mailbox plan was selected");

                                if (emailInfo.DomainID == 0)
                                    throw new Exception("Invalid domain was selected");

                                if (string.IsNullOrEmpty(emailInfo.EmailFirst))
                                    throw new Exception("Email was not provided");

                                logger.DebugFormat("Getting mailbox plan {0}", emailInfo.MailboxPlanID);
                                var mailboxPlan = (from d in db.Plans_ExchangeMailbox
                                                   where d.MailboxPlanID == emailInfo.MailboxPlanID
                                                   select d).FirstOrDefault();

                                logger.DebugFormat("Getting domain {0}", emailInfo.DomainID);
                                var domain = (from d in db.Domains
                                              where d.CompanyCode == companyCode
                                              where d.DomainID == emailInfo.DomainID
                                              select d).FirstOrDefault();

                                if (mailboxPlan != null && domain != null)
                                {
                                    emailInfo.EmailDomain = domain.Domain;
                                    UpdateMailbox(ref existingUser, emailInfo, mailboxPlan);
                                }
                                else
                                    logger.DebugFormat("Mailbox plan and/or domain for {0} was null. Skipping mailbox changes", upn);
                            }
                        }
                        else
                            logger.DebugFormat("Exchange is not enabled for {0} so we are not processing Exchange settings for this user", companyCode);
                        #endregion

                        db.SaveChanges();
                        logger.InfoFormat("Successfully updated user {0}", upn);
                        return Negotiate.WithModel(new { success = "Successfully updated user " + upn })
                                        .WithMediaRangeResponse("text/html", this.Response.AsRedirect("~/company/" + companyCode + "/users"));
                    }
                }
                catch (Exception ex)
                {
                    logger.ErrorFormat("Error updating user {0}: {1}", _.UserPrincipalName, ex.ToString());
                    return Negotiate.WithModel(new { error = ex.Message })
                                    .WithView("error.cshtml");
                }
                finally
                {
                    if (adUsers != null)
                        adUsers.Dispose();

                    if (db != null)
                        db.Dispose();
                }

                #endregion
            };*/
        }

    }
}
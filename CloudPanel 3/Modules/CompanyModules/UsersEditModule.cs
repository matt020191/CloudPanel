using CloudPanel.ActiveDirectory;
using CloudPanel.Base.Config;
using CloudPanel.Base.Database.Models;
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
                #region Creates a new user

                CloudPanelContext db = null;
                dynamic powershell = null;
                try
                {
                    string companyCode = _.CompanyCode;
                    string userPrincipalName = _.UserPrincipalName;

                    logger.DebugFormat("Retrieving user from database");
                    var sqlUser = (from d in db.Users
                                   where d.CompanyCode == companyCode
                                   where d.UserPrincipalName == userPrincipalName
                                   select d).FirstOrDefault();

                    if (sqlUser == null)
                        throw new Exception("Unable to find user in the database: " + userPrincipalName);
                    else
                    {
                        logger.DebugFormat("Binding form to class...");
                        var boundUser = this.Bind<Users>();
                        boundUser.UserPrincipalName = userPrincipalName;
                        boundUser.CompanyCode = companyCode;

                        logger.DebugFormat("Checking for mailbox changes");
                        bool newEmailStatus = Request.Form.IsEmailEnabled;

                        if (newEmailStatus && (sqlUser.MailboxPlan == null || sqlUser.MailboxPlan < 1)) // Old value disabled but new value enabled [Create]
                            EnableMailbox(ref powershell, ref boundUser);
                        else if (newEmailStatus && sqlUser.MailboxPlan > 0) // Old value enabled and new value enabled [Update]
                            UpdateMailbox(ref powershell, ref boundUser);
                        else if (!newEmailStatus && sqlUser.MailboxPlan > 0) // Old value enabled and new value disabled [Disable]
                            DisableMailbox(ref powershell, ref boundUser);


                    }                    
                }
                catch (Exception ex)
                {
                    logger.ErrorFormat("Error creating new user for company {0}: {1}", _.CompanyCode, ex.ToString());
                    return Negotiate.WithModel(new { error = ex.Message })
                                    .WithView("Company/company_users.cshtml");
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

        private void EnableMailbox(ref dynamic powershell, ref Users user)
        {
            powershell = ExchPowershell.GetClass();
        }

        private void DisableMailbox(ref dynamic powershell, ref Users user)
        {
            powershell = ExchPowershell.GetClass();
            powershell.Disable_Mailbox(user.UserPrincipalName);
        }

        private void UpdateMailbox(ref dynamic powershell, ref Users user)
        {
            powershell = ExchPowershell.GetClass();
        }

        #endregion
    }
}
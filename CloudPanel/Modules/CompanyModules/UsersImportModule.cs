using CloudPanel.ActiveDirectory;
using CloudPanel.Base.Config;
using CloudPanel.Base.Database.Models;
using CloudPanel.Database.EntityFramework;
using CloudPanel.Exchange;
using CloudPanel.Rollback;
using log4net;
using Nancy;
using Nancy.Security;
using System;
using System.Linq;

namespace CloudPanel.Modules
{
    public class UsersImportModule : NancyModule
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(UsersImportModule));

        public UsersImportModule() : base("/company/{CompanyCode}/users/import")
        {
            //this.RequiresAuthentication();

            Get["/csv"] = _ =>
            { 
                return View["Company/users_importcsv.cshtml"];
            };

            Post["/csv"] = _ =>
            {
                #region Creates a new user

                CloudPanelContext db = null;
                ADUsers adUsers = null;
                ADGroups adGroups = null;
                dynamic powershell = null;

                ReverseActions reverse = new ReverseActions();
                try
                {
                    string companyCode = _.CompanyCode;
                    #region Validate required fields

                    if (!Request.Form.DisplayName.HasValue)
                        throw new Exception("Display name is a required field");

                    if (!Request.Form.Username.HasValue)
                        throw new Exception("Username is a required field");

                    if (!Request.Form.Password.HasValue)
                        throw new Exception("Password is a required field");

                    if (!CloudPanel.CPStaticHelpers.IsUnderLimit(companyCode, "user"))
                        throw new Exception("You have reached the user limit");

                    #endregion

                    db = new CloudPanelContext(Settings.ConnectionString);
                    db.Database.Connection.Open();

                    logger.DebugFormat("Getting domains from the database");
                    var domains = (from d in db.Domains
                                   where d.CompanyCode == companyCode
                                   select d).ToList();

                    logger.DebugFormat("Getting mailbox plans from the database");
                    var mailboxPlans = (from d in db.Plans_ExchangeMailbox
                                        where (string.IsNullOrEmpty(d.CompanyCode) || d.CompanyCode == companyCode)
                                        select d).ToList();

                    logger.DebugFormat("Getting company from the database");
                    var company = (from d in db.Companies
                                   where !d.IsReseller
                                   where d.CompanyCode == companyCode
                                   select d).FirstOrDefault();

                    if (company == null)
                        throw new Exception("Unable to find company " + companyCode + " in the database");

                    if (domains == null)
                        throw new Exception("Unable to find the domains in the database for company");

                    #region Generate users object
                    logger.DebugFormat("Compiling data into Users object");

                    Users newUser = new Users();
                    newUser.CompanyCode = companyCode;
                    newUser.Name = Request.Form.DisplayName; // TODO: MAKE THIS USE EITHER DISPLAY NAME OR EMAIL BASED ON CONFIG FILE
                    newUser.DisplayName = Request.Form.DisplayName;
                    newUser.Firstname = Request.Form.Firstname.HasValue ? Request.Form.Firstname.Value : string.Empty;
                    newUser.Middlename = Request.Form.Middlename.HasValue ? Request.Form.Middlename.Value : string.Empty;
                    newUser.Lastname = Request.Form.Lastname.HasValue ? Request.Form.Lastname.Value : string.Empty;
                    newUser.Department = Request.Form.Department.HasValue ? Request.Form.Department.Value : string.Empty;
                    newUser.Company = Request.Form.Company.HasValue ? Request.Form.Company.Value : string.Empty;
                    newUser.JobTitle = Request.Form.JobTitle.HasValue ? Request.Form.JobTitle.Value : string.Empty;
                    newUser.TelephoneNumber = Request.Form.TelephoneNumber.HasValue ? Request.Form.TelephoneNumber.Value : string.Empty;
                    newUser.Fax = Request.Form.Fax.HasValue ? Request.Form.Fax.Value : string.Empty;
                    newUser.HomePhone = Request.Form.HomePhone.HasValue ? Request.Form.HomePhone.Value : string.Empty;
                    newUser.MobilePhone = Request.Form.MobilePhone.HasValue ? Request.Form.MobilePhone.Value : string.Empty;
                    newUser.Street = Request.Form.Street.HasValue ? Request.Form.Street.Value : string.Empty;
                    newUser.City = Request.Form.City.HasValue ? Request.Form.City.Value : string.Empty;
                    newUser.State = Request.Form.State.HasValue ? Request.Form.State.Value : string.Empty;
                    newUser.PostalCode = Request.Form.PostalCode.HasValue ? Request.Form.PostalCode.Value : string.Empty;
                    newUser.Country = Request.Form.Country.HasValue ? Request.Form.Country.Value : string.Empty;
                    newUser.IsResellerAdmin = false;
                    newUser.IsCompanyAdmin = false;
                    newUser.LyncPlan = 0;
                    newUser.AdditionalMB = 0;
                    newUser.IsEnabled = true;
                    newUser.Created = DateTime.Now;

                    // See if they set the change password next login value
                    if (Request.Form.ChangePasswordNextLogin.HasValue)
                    {
                        logger.DebugFormat("Flag to change password on next login was set");
                        string value = Request.Form.ChangePasswordNextLogin.Value;
                        bool change = false;
                        bool.TryParse(value, out change);

                        newUser.ChangePasswordNextLogin = change;
                    }
                    #endregion

                    #region UserPrincipalName validation

                    logger.DebugFormat("Formatting the UserPrincipalName");
                    string upn = Request.Form.Username.Value;
                    if (!upn.Contains("@"))
                        throw new Exception("Username does not contain an @ symbol. Username must be in the format of name@domain.tld");

                    logger.DebugFormat("Validating the domain is in the system for this company.");
                    string[] upnSplit = upn.Split(new[] { '@' }, StringSplitOptions.RemoveEmptyEntries);
                    var validDomain = (from d in domains where d.Domain == upnSplit[1] select d).FirstOrDefault();
                    if (validDomain == null)
                        throw new Exception("Unable to find the domain in the database for this company " + upnSplit[1]);

                    logger.DebugFormat("Making sure the userprincipalname {0} is not already taken", upn);
                    var takenUPN = (from d in db.Users
                                    where d.CompanyCode == companyCode
                                    where d.UserPrincipalName == upn
                                    select d).Count();

                    if (takenUPN > 0)
                        throw new Exception("The username " + upn + " is already taken");

                    logger.DebugFormat("Username {0} was not taken.. continue creating user... Getting company from database", upn);
                    newUser.UserPrincipalName = upn;

                    #endregion

                    logger.DebugFormat("Creating user in Active Directory now");
                    adUsers = new ADUsers(Settings.Username, Settings.DecryptedPassword, Settings.PrimaryDC);
                    newUser = adUsers.Create(Settings.UsersOuPath(company.DistinguishedName), Request.Form.Password, newUser);
                    reverse.AddAction(Actions.AddUsers, newUser.UserPrincipalName);

                    logger.DebugFormat("User {0} created in Active Directory. Adding to the AllUsers security group", upn);
                    adGroups = new ADGroups(Settings.Username, Settings.DecryptedPassword, Settings.PrimaryDC);
                    adGroups.AddUser("AllUsers@" + company.CompanyCode, newUser.UserPrincipalName);

                    logger.DebugFormat("User {0} was created successfully. Adding to database");
                    db.Users.Add(newUser);
                    db.SaveChanges();

                    logger.DebugFormat("Checking if user was enabled with a mailbox...");
                    string email = Request.Form.Email.HasValue ? Request.Form.Email.Value : string.Empty;
                    int planId = Request.Form.MailboxPlan.HasValue ? (from d in mailboxPlans
                                                                      where d.MailboxPlanName == (string)Request.Form.MailboxPlan.Value
                                                                      select d.MailboxPlanID).FirstOrDefault() : 0;

                    string message = "Successfully created user " + upn; // Our message to return back
                    if (!string.IsNullOrEmpty(email) && planId > 0)
                    {
                        logger.DebugFormat("Looks like user {0} is going to be imported with a mailbox with plan {1}", newUser.UserPrincipalName, planId);
                        string[] emailSplit = email.Split(new char[] { '@' }, StringSplitOptions.RemoveEmptyEntries);

                        var domainExist = (from d in domains
                                           where d.IsAcceptedDomain
                                           where d.Domain == emailSplit[1]
                                           select d).Count();

                        if (domainExist > 0)
                        {
                            // Set the email attribute to store in the database and pass to powershell
                            newUser.Email = email;

                            // Check the mailbox limit
                            if (!CloudPanel.CPStaticHelpers.IsUnderLimit(companyCode, "mailbox"))
                                throw new Exception("You have reached the mailbox limit.");

                            logger.DebugFormat("Initializing powershell to enable mailbox for {0}", newUser.UserPrincipalName);
                            powershell = ExchPowershell.GetClass();
                            powershell.Enable_Mailbox(newUser);
                            reverse.AddAction(Actions.CreateMailbox, newUser.UserPrincipalName);

                            logger.DebugFormat("Setting mailbox for {0}", newUser.UserPrincipalName);
                            var plan = (from d in mailboxPlans where d.MailboxPlanID == planId select d).First();
                            newUser.SizeInMB = plan.MailboxSizeMB; // Set the size to the minimum plan size
                            powershell.Set_Mailbox(newUser, plan, new string[] { "SMTP:" + newUser.Email });

                            logger.DebugFormat("Setting CAS mailbox for {0}", newUser.UserPrincipalName);
                            powershell.Set_CASMailbox(newUser.UserPrincipalName, plan);

                            logger.DebugFormat("Updating database with Exchange info for {0}", newUser.UserPrincipalName);
                            newUser.MailboxPlan = planId;
                            db.SaveChanges();
                        }
                        else
                        {
                            logger.InfoFormat("User {0} was supposed to have a mailbox on import but the domain {1} was invalid.", newUser.UserPrincipalName, email[1]);
                            message = string.Format("Successfully created user BUT the mailbox was not created because the domain {0} was invalid", email[1]);
                        }
                    }

                    return Negotiate.WithModel(new { message = message })
                                    .WithStatusCode(HttpStatusCode.OK);
                }
                catch (Exception ex)
                {
                    logger.ErrorFormat("Error creating new user for company {0}: {1}", _.CompanyCode, ex.ToString());

                    reverse.RollbackNow();
                    return Negotiate.WithModel(new { error = ex.Message })
                                    .WithStatusCode(HttpStatusCode.InternalServerError);
                }
                finally
                {
                    if (powershell != null)
                        powershell.Dispose();

                    if (adUsers != null)
                        adUsers.Dispose();

                    if (db != null)
                        db.Dispose();
                }
                #endregion
            };
        }
    }
}
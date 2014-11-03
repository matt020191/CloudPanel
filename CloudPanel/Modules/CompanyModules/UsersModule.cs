using CloudPanel.Base.Config;
using CloudPanel.Database.EntityFramework;
using log4net;
using Nancy;
using Nancy.Security;
using Nancy.ModelBinding;
using System;
using System.Linq;
using System.Reflection;
using CloudPanel.Base.Database.Models;
using CloudPanel.ActiveDirectory;
using CloudPanel.Rollback;
using CloudPanel.Base.Exchange;
using CloudPanel.Exchange;
using System.Collections.Generic;

namespace CloudPanel.Modules
{
    public class UsersModule : NancyModule
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(UsersModule));

        public UsersModule() : base("/company/{CompanyCode}/users")
        {
            this.RequiresAuthentication();

            Get["/", c => c.Request.Accept("text/html")] = _ =>
            {
                return View["Company/users.cshtml"];
            };

            Get["/", c => c.Request.Accept("application/json")] = _ =>
            {
                #region Returns the users view with model or json data based on the request
                CloudPanelContext db = null;
                try
                {
                    db = new CloudPanelContext(Settings.ConnectionString);
                    db.Database.Connection.Open();

                    string companyCode = _.CompanyCode;
                    var users = (from d in db.Users
                                 join m in db.Plans_ExchangeMailbox on d.MailboxPlan equals m.MailboxPlanID into d1
                                 from mailboxplan in d1.DefaultIfEmpty()
                                 join s in db.SvcMailboxSizes on d.UserPrincipalName equals s.UserPrincipalName into d2
                                 from mailboxinfo in d2.DefaultIfEmpty()
                                 where d.CompanyCode == companyCode
                                 select new
                                 {
                                     UserGuid = d.UserGuid,
                                     CompanyCode = d.CompanyCode,
                                     DisplayName = d.DisplayName,
                                     UserPrincipalName = d.UserPrincipalName,
                                     SamAccountName = d.sAMAccountName,
                                     DistinguishedName = d.DistinguishedName,
                                     Department = d.Department,
                                     AdditionalMB = d.AdditionalMB == null ? 0 : d.AdditionalMB,
                                     IsCompanyAdmin = d.IsCompanyAdmin == null ? false : (bool)d.IsCompanyAdmin,
                                     IsResellerAdmin = d.IsResellerAdmin == null ? false : (bool)d.IsResellerAdmin,
                                     IsEnabled = d.IsEnabled == null ? true : (bool)d.IsEnabled,
                                     MailboxPlan = d.MailboxPlan == null ? 0 : (int)d.MailboxPlan,
                                     MailboxPlanName = mailboxplan == null ? "" : mailboxplan.MailboxPlanName,
                                     MailboxPlanSize = mailboxplan == null ? 0 : mailboxplan.MailboxSizeMB,
                                     MailboxPlanMaxSize = mailboxplan == null ? 0 : mailboxplan.MaxMailboxSizeMB,
                                     MailboxPlanMaxSendKB = mailboxplan == null ? 0 : mailboxplan.MaxSendKB,
                                     MailboxPlanMaxReceiveKB = mailboxplan == null ? 0 : mailboxplan.MaxReceiveKB,
                                     MailboxPlanMaxRecipients = mailboxplan == null ? 0 : mailboxplan.MaxRecipients,
                                     MailboxPlanPOP3 = mailboxplan == null ? false : mailboxplan.EnablePOP3,
                                     MailboxPlanIMAP = mailboxplan == null ? false: mailboxplan.EnableIMAP,
                                     MailboxPlanOWA = mailboxplan == null ? false : mailboxplan.EnableOWA,
                                     MailboxPlanMAPI = mailboxplan == null ? false : mailboxplan.EnableMAPI,
                                     MailboxPlanAS = mailboxplan == null ? false : mailboxplan.EnableAS,
                                     MailboxPlanECP = mailboxplan == null ? false : mailboxplan.EnableECP,
                                     MailboxDatabase = mailboxinfo == null ? "" : mailboxinfo.MailboxDatabase,
                                     MailboxSizeInKB = mailboxinfo == null ? "" : mailboxinfo.TotalItemSizeInKB,
                                     MailboxDeletedSizeInKB = mailboxinfo == null ? "" : mailboxinfo.TotalDeletedItemSizeInKB,
                                     MailboxItemCount = mailboxinfo == null ? 0 : mailboxinfo.ItemCount,
                                     MailboxDeletedItemCount = mailboxinfo == null ? 0 : mailboxinfo.DeletedItemCount,
                                     MailboxInfoRetrieved = mailboxinfo == null ? "" : mailboxinfo.Retrieved.ToString(),
                                     Created = d.Created,
                                     Email = d.Email
                                 }).ToList();

                    int draw = 0, start = 0, length = 0, recordsTotal = users.Count, recordsFiltered = users.Count, orderColumn = 0;
                    string searchValue = "", orderColumnName = "";
                    bool isAscendingOrder = true;

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
                        users = (from d in users
                                 where d.DisplayName.IndexOf(searchValue, StringComparison.InvariantCultureIgnoreCase) != -1 ||
                                        d.UserPrincipalName.IndexOf(searchValue, StringComparison.InvariantCultureIgnoreCase) != -1 ||
                                        d.SamAccountName.IndexOf(searchValue, StringComparison.InvariantCultureIgnoreCase) != -1 ||
                                        d.Department.IndexOf(searchValue, StringComparison.InvariantCultureIgnoreCase) != -1 ||
                                        d.Email.IndexOf(searchValue, StringComparison.InvariantCultureIgnoreCase) != -1
                                 select d).ToList();
                        recordsFiltered = users.Count;
                    }

                    if (isAscendingOrder)
                        users = users.OrderBy(x => x.GetType()
                                                .GetProperty(orderColumnName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance).GetValue(x, null))
                                                .Skip(start)
                                                .Take(length)
                                                .ToList();
                    else
                        users = users.OrderByDescending(x => x.GetType()
                                                .GetProperty(orderColumnName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance).GetValue(x, null))
                                                .Skip(start)
                                                .Take(length)
                                                .ToList();


                    return Negotiate.WithModel(new
                                    {
                                        draw = draw,
                                        recordsTotal = recordsTotal,
                                        recordsFiltered = recordsFiltered,
                                        data = users
                                    })
                                    .WithStatusCode(HttpStatusCode.OK);
                }
                catch (Exception ex)
                {
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

            Post["/"] = _ =>
            {
                #region Creates a new user

                CloudPanelContext db = null;
                ADUsers adUsers = null;
                ADGroups adGroups = null;

                ReverseActions reverse = new ReverseActions();
                try
                {
                    logger.DebugFormat("Creating a new user... validating parameters");

                    if (!Request.Form.DisplayName.HasValue)
                        throw new Exception("Display name is a required field");

                    if (!Request.Form.Username.HasValue)
                        throw new Exception("Username is a required field");

                    if (!Request.Form.DomainID.HasValue)
                        throw new Exception("DomainID is a required field");

                    if (!Request.Form.Password.HasValue)
                        throw new Exception("Password is a required field");

                    int domainID = Request.Form.DomainID;
                    string companyCode = _.CompanyCode;

                    logger.DebugFormat("Getting selected domain from the database");
                    db = new CloudPanelContext(Settings.ConnectionString);
                    db.Database.Connection.Open();

                    var domain = (from d in db.Domains
                                  where d.CompanyCode == companyCode
                                  where d.DomainID == domainID
                                  select d).FirstOrDefault();

                    if (domain == null)
                        throw new Exception("Unable to find the domain in the database");
                    else
                    {
                        logger.DebugFormat("Compiling data into Users object");

                        Users newUser = new Users();
                        newUser.CompanyCode = companyCode;
                        newUser.Name = Request.Form.DisplayName; // TODO: MAKE THIS USE EITHER DISPLAY NAME OR EMAIL BASED ON CONFIG FILE
                        newUser.DisplayName = Request.Form.DisplayName;

                        newUser.Firstname = Request.Form.Firstname.HasValue ? Request.Form.Firstname.Value : string.Empty;
                        newUser.Middlename = Request.Form.Middlename.HasValue ? Request.Form.Middlename.Value : string.Empty;
                        newUser.Lastname = Request.Form.Lastname.HasValue ? Request.Form.Lastname.Value : string.Empty;
                        newUser.Department = Request.Form.Department.HasValue ? Request.Form.Department.Value : string.Empty;
                        newUser.Email = string.Empty;
                        newUser.IsResellerAdmin = false;
                        newUser.IsCompanyAdmin = false;
                        newUser.MailboxPlan = 0;
                        newUser.TSPlan = 0;
                        newUser.LyncPlan = 0;
                        newUser.Created = DateTime.Now;
                        newUser.AdditionalMB = 0;
                        newUser.ActiveSyncPlan = 0;
                        newUser.IsEnabled = true;

                        logger.DebugFormat("Formatting the UserPrincipalName");
                        string upn = string.Format("{0}@{1}", Request.Form.Username, domain.Domain);

                        logger.DebugFormat("Replacing whitespace characters in UPN: {0}", upn);
                        upn = upn.Replace(" ", string.Empty);

                        logger.DebugFormat("UPN after replacing whitepsace characters: {0}", upn);

                        logger.DebugFormat("Making sure the userprincipalname {0} is not already taken", upn);
                        var takenUPN = (from d in db.Users
                                        where d.CompanyCode == companyCode
                                        where d.UserPrincipalName == upn
                                        select d).Count();

                        if (takenUPN > 0)
                            throw new Exception("The username you entered is already taken");
                        else
                        {
                            logger.DebugFormat("Username {0} was not taken.. continue creating user... Getting company from database", upn);
                            newUser.UserPrincipalName = upn;

                            var company = (from d in db.Companies
                                           where !d.IsReseller
                                           where d.CompanyCode == companyCode
                                           select d).FirstOrDefault();

                            if (company == null)
                                throw new Exception("Unable to find company in database");
                            else
                            {
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

                                return Negotiate.WithModel(new { success = "Created new user " + upn })
                                                .WithStatusCode(HttpStatusCode.OK);
                            }
                        }
                    }
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
                    if (adUsers != null)
                        adUsers.Dispose();

                    if (db != null)
                        db.Dispose();
                }

                #endregion
            };

            Delete["/"] = _ =>
            {
                #region Deletes a user from Active Directory and the database
                CloudPanelContext db = null;
                ADUsers adUsers = null;
                try
                {
                    logger.DebugFormat("Opening connection to delete user for {0}", _.CompanyCode);
                    db = new CloudPanelContext(Settings.ConnectionString);
                    db.Database.Connection.Open();

                    logger.DebugFormat("Validating parameters");
                    if (!Request.Form.UserPrincipalName.HasValue)
                        throw new Exception("UserPrincipalName is a required field");
                    else
                    {
                        logger.DebugFormat("Getting company code and userprincipalname for {0}", _.CompanyCode);
                        string companyCode = _.CompanyCode;
                        string upn = Request.Form.UserPrincipalName;

                        logger.DebugFormat("Getting user {0} from database", upn);
                        var user = (from d in db.Users
                                    where d.CompanyCode == companyCode
                                    where d.UserPrincipalName == upn
                                    select d).FirstOrDefault();

                        if (user == null)
                            throw new Exception("Unable to find user in database");
                        else
                        {
                            logger.DebugFormat("Deleting {0} from Active Directory", upn);
                            adUsers = new ADUsers(Settings.Username, Settings.DecryptedPassword, Settings.PrimaryDC);
                            adUsers.Delete(upn);

                            logger.DebugFormat("Removing user from database");
                            int userId = user.ID; // To perform more cleanup
                            db.Users.Remove(user);
                            db.SaveChanges();

                            logger.DebugFormat("User has been removed. Now cleaning up the database to remove all traces of the user");

                            logger.DebugFormat("Clearing user from citrix plans");
                            var citrixPlans = from d in db.UserPlansCitrix where d.UserID == userId select d;
                            if (citrixPlans != null)
                                db.UserPlansCitrix.RemoveRange(citrixPlans);

                            logger.DebugFormat("Clearing user from permissions");
                            var permissions = from d in db.UserPermissions where d.UserID == userId select d;
                            if (permissions != null)
                                db.UserPermissions.RemoveRange(permissions);

                            logger.DebugFormat("Clearing user from queues");
                            var queues = from d in db.SvcQueue where d.UserPrincipalName == upn select d;
                            if (queues != null)
                                db.SvcQueue.RemoveRange(queues);

                            logger.DebugFormat("Clearing user from mailbox sizes");
                            var mailboxSizes = from d in db.SvcMailboxSizes where d.UserPrincipalName == upn select d;
                            if (mailboxSizes != null)
                                db.SvcMailboxSizes.RemoveRange(mailboxSizes);

                            logger.DebugFormat("Finished cleanup for {0}", upn);
                            db.SaveChanges();

                            return Negotiate.WithModel(new { success = "Deleted user " + upn })
                                            .WithView("Company/users.cshtml");
                        }
                    }
                }
                catch (Exception ex)
                {
                    return Negotiate.WithModel(new { error = ex.Message })
                                    .WithView("Company/users.cshtml");
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
        }

        /// <summary>
        /// Updates the old object with new information from the new object
        /// </summary>
        /// <param name="oldObject"></param>
        /// <param name="newObject"></param>
        private void Combine(ref Users oldObject, ref Users newObject)
        {
            logger.DebugFormat("Combining Users object for {0}", oldObject.UserPrincipalName);

            oldObject.Firstname = newObject.Firstname;
            oldObject.Middlename = newObject.Middlename;
            oldObject.Lastname = newObject.Lastname;
            oldObject.DisplayName = newObject.DisplayName;
            oldObject.Company = newObject.Company;
            oldObject.Department = newObject.Department;
            oldObject.JobTitle = newObject.JobTitle;
            oldObject.TelephoneNumber = newObject.TelephoneNumber;
            oldObject.Fax = newObject.Fax;
            oldObject.Street = newObject.Street;
            oldObject.City = newObject.City;
            oldObject.State = newObject.State;
            oldObject.PostalCode = newObject.PostalCode;
            oldObject.Country = newObject.Country;
        }

        /// <summary>
        /// Updates the Active Directory attributes of the user
        /// </summary>
        /// <param name="updatedUser"></param>
        /// <param name="userPrincipalName"></param>
        private void UpdateAdUser(ref Users updatedUser, string userPrincipalName)
        {
            ADUsers adUsers = null;
            try
            {
                updatedUser.UserPrincipalName = userPrincipalName;
                logger.DebugFormat("Updating user {0} in Active Directory", userPrincipalName);

                adUsers = new ADUsers(Settings.Username, Settings.DecryptedPassword, Settings.PrimaryDC);
                adUsers.UpdateUser(updatedUser);

                logger.DebugFormat("Successfully updated {0} in Active Directory", userPrincipalName);
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error updating user {0} in Active Directory: {1}", ex.ToString());
                throw;
            }
            finally
            {
                if (adUsers != null)
                    adUsers.Dispose();
            }
        }

        /// <summary>
        /// Updates a mailbox for a specific user (or doesn't update depending on settings)
        /// </summary>
        /// <param name="userObject"></param>
        /// <param name="boundInfo"></param>
        private void UpdateMailbox(ref Users userObject, EmailInfo boundInfo, Plans_ExchangeMailbox mailboxPlan)
        {
            dynamic powershell = null;
            try
            {
                if (userObject.MailboxPlan > 0 && !boundInfo.IsEmailEnabled)
                {
                    // We are disabling the mailbox
                    powershell = ExchPowershell.GetClass();
                    powershell.Disable_Mailbox(userObject.UserPrincipalName);

                    userObject.MailboxPlan = 0;
                    userObject.AdditionalMB = 0;
                }
                else if (userObject.MailboxPlan <= 0 && boundInfo.IsEmailEnabled)
                {
                    // We are enabling the mailbox
                    powershell = ExchPowershell.GetClass();
                    powershell.Enable_Mailbox(userObject.UserPrincipalName, userObject.CompanyCode, boundInfo, mailboxPlan);

                    userObject.MailboxPlan = mailboxPlan.MailboxPlanID;
                    userObject.AdditionalMB = 0;

                }
                else if (userObject.MailboxPlan > 0 && boundInfo.IsEmailEnabled)
                {
                    // We are updating the mailbox
                    powershell = ExchPowershell.GetClass();
                }
                else
                {
                    logger.DebugFormat("No changes made to mailbox");
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error updating mailbox for {0}: {1}", userObject.UserPrincipalName, ex.ToString());
                throw;
            }
            finally
            {
                if (powershell != null)
                    powershell.Dispose();
            }
        }
        
        /// <summary>
        /// Gets a list of mailbox users
        /// </summary>
        /// <param name="companyCode"></param>
        /// <returns></returns>
        public static List<Users> GetMailboxUsers(string companyCode)
        {
            CloudPanelContext db = null;
            try
            {
                db = new CloudPanelContext(Settings.ConnectionString);
                db.Database.Connection.Open();

                logger.DebugFormat("Getting mailbox users for {0}", companyCode);
                var users = (from u in db.Users
                             where u.CompanyCode == companyCode
                             where u.MailboxPlan > 0
                             orderby u.DisplayName
                             select u).ToList();

                logger.DebugFormat("Found a total of {0} mailbox users", users.Count());
                return users;
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error getting mailbox users: {0}", ex.ToString());
                throw;
            }
            finally
            {
                if (db != null)
                    db.Dispose();
            }
        }
    }
}
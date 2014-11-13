using CloudPanel.ActiveDirectory;
using CloudPanel.Base.Config;
using CloudPanel.Base.Database.Models;
using CloudPanel.Database.EntityFramework;
using CloudPanel.Rollback;
using log4net;
using Nancy;
using Nancy.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CloudPanel.Modules
{
    public class UsersImportModule : NancyModule
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(UsersImportModule));

        public UsersImportModule() : base("/company/{CompanyCode}/users/import")
        {
            this.RequiresAuthentication();

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

                ReverseActions reverse = new ReverseActions();
                try
                {
                    logger.DebugFormat("Creating a new user... validating parameters");

                    if (!Request.Form.DisplayName.HasValue)
                        throw new Exception("Display name is a required field");

                    if (!Request.Form.UserPrincipalName.HasValue)
                        throw new Exception("Username is a required field");

                    if (!Request.Form.Password.HasValue)
                        throw new Exception("Password is a required field");

                    string companyCode = _.CompanyCode;

                    logger.DebugFormat("Getting domains from the database");
                    db = new CloudPanelContext(Settings.ConnectionString);
                    db.Database.Connection.Open();

                    var domains = (from d in db.Domains
                                  where d.CompanyCode == companyCode
                                  select d).ToList();

                    if (domains == null)
                        throw new Exception("Unable to find the domains in the database for company");
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
                        string upn = Request.Form.UserPrincipalName;
                        string[] upnSplit = upn.Split(new[] { '@' }, StringSplitOptions.RemoveEmptyEntries);
                        string upnDomain = upnSplit[1];

                        logger.DebugFormat("Validating the domain is in the system for this company.");
                        var validDomain = (from d in domains where d.Domain == upnDomain select d).FirstOrDefault();
                        if (validDomain == null)
                            throw new Exception("Unable to find the domain in the database for this company " + upnDomain);


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
        }
    }
}
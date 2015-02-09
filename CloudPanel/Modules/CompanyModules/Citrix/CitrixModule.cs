using CloudPanel.Base.Citrix;
using CloudPanel.Base.Config;
using CloudPanel.Base.Database.Models;
using CloudPanel.Citrix;
using CloudPanel.Code;
using CloudPanel.ActiveDirectory;
using CloudPanel.Database.EntityFramework;
using log4net;
using Nancy;
using Nancy.ModelBinding;
using Nancy.Security;
using System.Data.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CloudPanel.Base.AD;
using CloudPanel.Rollback;

namespace CloudPanel.Modules.CompanyModules.Citrix
{
    public class CitrixModule : NancyModule
    {
        private static readonly ILog logger = LogManager.GetLogger("Citrix");

        public CitrixModule() : base("/company/{CompanyCode}/citrix")
        {
            Get["/sessions", c => c.Request.Accept("text/html")] = _ =>
                {
                    this.RequiresValidatedClaims(c => ValidateClaims.AllowCompanyAdmin(Context.CurrentUser, _.CompanyCode, "vCitrix"));
                    return View["Company/Citrix/sessions.cshtml"];
                };

            Get["/sessions", c => !c.Request.Accept("text/html")] = _ =>
                {
                    this.RequiresValidatedClaims(c => ValidateClaims.AllowCompanyAdmin(Context.CurrentUser, _.CompanyCode, "vCitrix"));
                    #region Gets sessions for all destop groups the user is assigned
                    string companyCode = _.CompanyCode;
                    CloudPanelContext db = null;
                    XenDesktop7 xd7 = null;
                    try
                    {
                        db = new CloudPanelContext(Settings.ConnectionString);
                        db.Database.Connection.Open();

                        var users = db.Users.Where(x => x.CompanyCode == companyCode).ToList();
                        var desktopGroups = db.CitrixDesktopGroup
                                              .Where(x =>
                                                     x.Companies.Any(c =>
                                                     c.CompanyCode == companyCode))
                                              .ToList();

                        var brokerSessions = new List<BrokerSession>();
                        xd7 = new XenDesktop7(Settings.CitrixUri, Settings.Username, Settings.DecryptedPassword);
                        desktopGroups.ForEach(x =>
                            {
                                var sessions = xd7.GetSessionsByDesktopGroup(x.Uid);
                                if (sessions != null)
                                {
                                    sessions.ForEach(s =>
                                        {
                                            if (users.Any(u => u.UserPrincipalName.Equals(s.UserUPN, StringComparison.CurrentCultureIgnoreCase)))
                                            {
                                                brokerSessions.Add(s);
                                            }
                                        });
                                }
                            });

                        return Negotiate.WithModel(new { sessions = brokerSessions })
                                        .WithStatusCode(HttpStatusCode.OK);
                    }
                    catch (Exception ex)
                    {
                        logger.ErrorFormat("Error getting sessions for {0}: {1}", companyCode, ex.ToString());
                        return Negotiate.WithModel(new { error = ex.Message })
                                        .WithStatusCode(HttpStatusCode.InternalServerError);
                    }
                    finally
                    {
                        if (xd7 != null)
                            xd7.Dispose();

                        if (db != null)
                            db.Dispose();
                    }
                    #endregion
                };

            Get["/desktopgroups", c => c.Request.Accept("text/html")] = _ =>
                {
                    this.RequiresValidatedClaims(c => ValidateClaims.AllowCompanyAdmin(Context.CurrentUser, _.CompanyCode, "vCitrix"));
                    return Negotiate.WithView("Company/Citrix/groups.cshtml");
                };

            Get["/desktopgroups", c => !c.Request.Accept("text/html")] = _ =>
                {
                    this.RequiresValidatedClaims(c => ValidateClaims.AllowCompanyAdmin(Context.CurrentUser, _.CompanyCode, "vCitrix"));
                    #region Return all desktop groups
                    string companyCode = _.CompanyCode;
                    try
                    {
                        return Negotiate.WithModel(new { groups = GetDesktopGroups(companyCode) });
                    }
                    catch (Exception ex)
                    {
                        logger.ErrorFormat("Error getting desktops group for company {0}: {1}", companyCode, ex.ToString());
                        return Negotiate.WithModel(new { error = ex.Message })
                                        .WithView("error/500.cshtml")
                                        .WithStatusCode(HttpStatusCode.InternalServerError);
                    }
                    #endregion
                };

            Get["/desktopgroups/{ID:int}", c => c.Request.Accept("text/html")] = _ =>
                {
                    this.RequiresValidatedClaims(c => ValidateClaims.AllowCompanyAdmin(Context.CurrentUser, _.CompanyCode, "vCitrix"));
                    return View["Company/Citrix/groups.cshtml"];
                };

            Get["/desktopgroups/{ID:int}", c => !c.Request.Accept("text/html")] = _ =>
                {
                    this.RequiresValidatedClaims(c => ValidateClaims.AllowCompanyAdmin(Context.CurrentUser, _.CompanyCode, "vCitrix"));
                    #region Get all users for the desktop group and if they are selected or not
                    int id = _.ID;
                    string companyCode = _.CompanyCode;

                    CloudPanelContext db = null;
                    try
                    {
                        db = new CloudPanelContext(Settings.ConnectionString);
                        db.Database.Connection.Open();

                        var allUsers = (from d in db.Users
                                                    .Include(x => x.CitrixDesktopGroups)
                                        where d.CompanyCode == companyCode
                                        orderby d.DisplayName
                                        select new {
                                            UserGuid = d.UserGuid,
                                            DisplayName = d.DisplayName,
                                            UserPrincipalName = d.UserPrincipalName,
                                            IsSelected = d.CitrixDesktopGroups.Any(a => a.DesktopGroupID == id)
                                        }).ToList();

                        logger.DebugFormat("Total users: {0}", allUsers.Count);
                        return Negotiate.WithModel(new { users = allUsers })
                                        .WithStatusCode(HttpStatusCode.OK);
                    }
                    catch (Exception ex)
                    {
                        logger.ErrorFormat("Error getting users for company desktop group {0}: {1}", companyCode, ex.ToString());
                        return Negotiate.WithModel(new { error = ex.Message })
                                        .WithView("error/500.cshtml")
                                        .WithStatusCode(HttpStatusCode.InternalServerError);
                    }
                    finally
                    {
                        if (db != null)
                            db.Dispose();
                    }
                    #endregion
                };

            Post["/desktopgroups/{ID:int}"] = _ =>
                {
                    this.RequiresValidatedClaims(c => ValidateClaims.AllowCompanyAdmin(Context.CurrentUser, _.CompanyCode, "eCitrix"));
                    #region Adds/removes users to desktop groups
                    int id = _.ID;
                    string companyCode = _.CompanyCode;

                    CloudPanelContext db = null;
                    ADGroups adGroup = null;
                    ReverseActions reverse = new ReverseActions();
                    try
                    {
                        db = new CloudPanelContext(Settings.ConnectionString);
                        db.Database.Connection.Open();

                        var userGuids = new List<string>();
                        if (Request.Form["UserGuid[]"].HasValue) {
                            string g = Request.Form["UserGuid[]"].Value;
                            userGuids = g.ToLower().Split(',').ToList();
                        }

                        // Get the desktop group from the database
                        logger.DebugFormat("Retrieving desktop group id {0} from the database", id);
                        var desktopGroup = db.CitrixDesktopGroup.Where(x => x.DesktopGroupID == id).Single();
                        if (string.IsNullOrEmpty(desktopGroup.SecurityGroup))
                            throw new ArgumentNullException("SecurityGroup");

                        // Get the company and the users that belong to the company
                        logger.DebugFormat("Retrieving company code {0} and users from the database", companyCode);
                        var company = db.Companies.Where(x => x.CompanyCode == companyCode).Single();
                        var users = db.Users.Include(x => x.CitrixDesktopGroups).Where(x => x.CompanyCode == companyCode).ToList();

                        // Validate that a security group exists for this desktop group for the company
                        logger.DebugFormat("Checking if the security group for the desktop group and company code {0} already exist", companyCode);
                        adGroup = new ADGroups(Settings.Username, Settings.DecryptedPassword, Settings.PrimaryDC);
                        var securityGroup = db.CitrixSecurityGroup.Where(x => x.DesktopGroupID == desktopGroup.DesktopGroupID && x.CompanyCode == companyCode).FirstOrDefault();
                        if (securityGroup == null)
                        {
                            // Security group does not exist for this desktop group & company. Create it!
                            var newSecurityGroup = new SecurityGroup();
                            newSecurityGroup.Name = string.Format("{0}@{1}", desktopGroup.SecurityGroup, companyCode);
                            newSecurityGroup.SamAccountName = newSecurityGroup.Name;
                            newSecurityGroup.Description = string.Format("Desktop Group {0}, Created by CloudPanel", desktopGroup.Name);

                            adGroup.Create(Settings.ApplicationOuPath(company.DistinguishedName), newSecurityGroup);
                            reverse.AddAction(Actions.CreateSecurityGroup, newSecurityGroup.Name);

                            // Add group to the parent desktop group and the AllTSUsers group
                            adGroup.AddGroup(desktopGroup.SecurityGroup, newSecurityGroup.Name);
                            adGroup.AddGroup("AllTSUsers@" + companyCode, newSecurityGroup.Name);

                            // Add the new security group to the citrix security group table
                            securityGroup = new CitrixSecurityGroups();
                            securityGroup.GroupName = newSecurityGroup.Name;
                            securityGroup.CompanyCode = companyCode;
                            securityGroup.Description = newSecurityGroup.Description;
                            securityGroup.DesktopGroupID = desktopGroup.DesktopGroupID;

                            db.CitrixSecurityGroup.Add(securityGroup);
                        }

                        //
                        // Loop through each user and see if we are adding or removing from the security group
                        //
                        var addedGuids = new List<string>();
                        var removedGuids = new List<string>();
                        users.ForEach(x =>
                        {
                            string guid = x.UserGuid.ToString().ToLower();
                            if (!x.CitrixDesktopGroups.Contains(desktopGroup) && userGuids.Contains(guid))
                            {
                                // We are adding the user
                                addedGuids.Add(guid);
                                x.CitrixDesktopGroups.Add(desktopGroup);
                            }
                            else if (x.CitrixDesktopGroups.Contains(desktopGroup) && !userGuids.Contains(guid))
                            {
                                // We are removing the user
                                removedGuids.Add(guid);
                                x.CitrixDesktopGroups.Remove(desktopGroup);
                            }
                        });

                        // Add users to the security group
                        if (addedGuids.Count > 0)
                            adGroup.AddUser(securityGroup.GroupName, addedGuids.ToArray());

                        // Remove users from the security group
                        if (removedGuids.Count > 0)
                            adGroup.RemoveUser(securityGroup.GroupName, removedGuids.ToArray());

                        db.SaveChanges();
                        return Negotiate.WithModel(new { success = "Successfully desktop group users" })
                                        .WithStatusCode(HttpStatusCode.OK);
                    }
                    catch (Exception ex)
                    {
                        logger.ErrorFormat("Error updating desktop group {0}: {1}", id, ex.ToString());

                        reverse.RollbackNow();
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

            Get["/applications"] = _ =>
                {
                    this.RequiresValidatedClaims(c => ValidateClaims.AllowCompanyAdmin(Context.CurrentUser, _.CompanyCode, "vCitrix"));
                    #region Gets applications
                    string companyCode = _.CompanyCode;
                    logger.DebugFormat("Querying applications for {0}", companyCode);
                    try
                    {
                        return Negotiate.WithModel(GetApplications(companyCode));
                    }
                    catch (Exception ex)
                    {
                        logger.ErrorFormat("Error getting applications for company {0}: {1}", companyCode, ex.ToString());
                        return Negotiate.WithModel(new { error = ex.Message })
                                        .WithStatusCode(HttpStatusCode.InternalServerError);
                    }
                    #endregion
                };
        }

        public static List<CitrixDesktopGroups> GetDesktopGroups(string companyCode)
        {
            CloudPanelContext db = null;
            try
            {
                db = new CloudPanelContext(Settings.ConnectionString);
                db.Database.Connection.Open();

                var desktopGroups = db.Companies
                                        .Include(x => x.CitrixDesktopGroups)
                                      .Where(x => x.CompanyCode == companyCode)
                                      .SelectMany(x => x.CitrixDesktopGroups)
                                      .ToList();

                return desktopGroups;
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error getting desktops group for company {0}: {1}", companyCode, ex.ToString());
                return null;
            }
            finally
            {
                if (db != null)
                    db.Dispose();
            }
        }

        public static List<CitrixApplications> GetApplications(string companyCode)
        {
            CloudPanelContext db = null;
            try
            {
                db = new CloudPanelContext(Settings.ConnectionString);
                db.Database.Connection.Open();

                var applications = new List<CitrixApplications>();
                var desktopGroups = db.Companies
                                        .Include(x => x.CitrixDesktopGroups.Select(a => a.Applications))
                                      .Where(x => x.CompanyCode == companyCode)
                                      .SelectMany(x => x.CitrixDesktopGroups)
                                      .ToList();

                desktopGroups.ForEach(x =>
                    {
                        if (x.Applications != null)
                        {
                            foreach (var app in x.Applications)
                            {
                                applications.Add(app);
                            }
                        }
                    });

                return applications;
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error getting desktops group for company {0}: {1}", companyCode, ex.ToString());
                return null;
            }
            finally
            {
                if (db != null)
                    db.Dispose();
            }
        }
    }
}
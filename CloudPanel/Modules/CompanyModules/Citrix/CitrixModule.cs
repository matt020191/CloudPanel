using CloudPanel.Base.Citrix;
using CloudPanel.Base.Config;
using CloudPanel.Base.Database.Models;
using CloudPanel.Citrix;
using CloudPanel.Code;
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

                    string companyCode = _.CompanyCode;
                    #region Gets sessions for all destop groups the user is assigned
                    CloudPanelContext db = null;
                    XenDesktop7 xd7 = null;
                    try
                    {
                        db = new CloudPanelContext(Settings.ConnectionString);
                        db.Database.Connection.Open();

                        var users = db.Users
                                      .Where(x => x.CompanyCode == companyCode)
                                      .ToList();

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
                    //this.RequiresValidatedClaims(c => ValidateClaims.AllowCompanyAdmin(Context.CurrentUser, _.CompanyCode, "vCitrix"));
                    string companyCode = _.CompanyCode;
                    return Negotiate.WithView("Company/Citrix/groups.cshtml");
                };

            Get["/desktopgroups", c => !c.Request.Accept("text/html")] = _ =>
                {
                    string companyCode = _.CompanyCode;
                    try
                    {
                        return Negotiate.WithModel(new { groups = GetDesktopGroups(companyCode) })
                                        .WithView("Company/Citrix/groups.cshtml");
                    }
                    catch (Exception ex)
                    {
                        logger.ErrorFormat("Error getting desktops group for company {0}: {1}", companyCode, ex.ToString());
                        return Negotiate.WithModel(new { error = ex.Message })
                                        .WithView("error/500.cshtml")
                                        .WithStatusCode(HttpStatusCode.InternalServerError);
                    }
                };

            Get["/desktopgroups/users"] = _ =>
                {
                    string companyCode = _.CompanyCode;

                    #region Gets a list of users in a desktop group
                    var db = new CloudPanelContext(Settings.ConnectionString);
                    var allUsers = (from d in db.Users
                                    where d.CompanyCode == companyCode
                                    orderby d.DisplayName
                                    select new
                                    {
                                        UserGuid = d.UserGuid,
                                        DisplayName = d.DisplayName
                                    }).ToList();

                    return Response.AsJson(allUsers);
                    #endregion
                };

            Get["/desktopgroups/{ID:int}", c => c.Request.Accept("text/html")] = _ =>
                {
                    return View["Company/Citrix/groups.cshtml"];
                };

            Get["/desktopgroups/{ID:int}", c => !c.Request.Accept("text/html")] = _ =>
                {
                    int id = _.ID;
                    string companyCode = _.CompanyCode;

                    #region Get all users for the desktop group and if they are selected or not
                    CloudPanelContext db = null;
                    try
                    {
                        db = new CloudPanelContext(Settings.ConnectionString);
                        db.Database.Connection.Open();

                        var allUsers = (from d in db.Users
                                                    .Include(x => x.CitrixDesktopGroups)
                                        where d.CompanyCode == companyCode
                                        select new {
                                            UserGuid = d.UserGuid,
                                            DisplayName = d.DisplayName,
                                            UserPrincipalName = d.UserPrincipalName,
                                            IsSelected = false
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
                    int id = _.ID;
                    string companyCode = _.CompanyCode;

                    CloudPanelContext db = null;
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
                        var desktopGroup = db.CitrixDesktopGroup.Where(x => x.DesktopGroupID == id).Single();

                        // Get a list of all users so we can tell who was removed or added
                        var addedGuids = new List<string>();
                        var removedGuids = new List<string>();
                        var users = db.Users.Include(x => x.CitrixDesktopGroups).Where(x => x.CompanyCode == companyCode).ToList();
                        users.ForEach(x =>
                        {
                            string guid = x.UserGuid.ToString().ToLower();
                            if (!x.CitrixDesktopGroups.Contains(desktopGroup) && userGuids.Contains(guid))
                            {
                                // We are adding the user
                                x.CitrixDesktopGroups.Add(desktopGroup);
                            }
                            else if (x.CitrixDesktopGroups.Contains(desktopGroup) && !userGuids.Contains(guid))
                            {
                                // We are removing the user
                                x.CitrixDesktopGroups.Remove(desktopGroup);
                            }
                        });

                        db.SaveChanges();

                        return Negotiate.WithModel(new { success = "Successfully desktop group users" })
                                        .WithStatusCode(HttpStatusCode.OK);
                    }
                    catch (Exception ex)
                    {
                        logger.ErrorFormat("Error updating desktop group {0}: {1}", id, ex.ToString());
                        return Negotiate.WithModel(new { error = ex.Message })
                                        .WithStatusCode(HttpStatusCode.InternalServerError);
                    }
                    finally
                    {
                        if (db != null)
                            db.Dispose();
                    }
                };

            Get["/applications"] = _ =>
            {
                this.RequiresValidatedClaims(c => ValidateClaims.AllowCompanyAdmin(Context.CurrentUser, _.CompanyCode, "vCitrix"));
                string companyCode = _.CompanyCode;

                #region Gets applications
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
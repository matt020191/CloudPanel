using CloudPanel.Base.Config;
using CloudPanel.Base.Database.Models;
using CloudPanel.Database.EntityFramework;
using log4net;
using Nancy;
using Nancy.ModelBinding;
using Nancy.ViewEngines.Razor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CloudPanel.Modules
{
    public class PermissionsModule : NancyModule
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(PermissionsModule));

        public PermissionsModule() : base("/company/{CompanyCode}/permissions")
        {
            Get["/new"] = _ =>
            {
                #region Get roles and show default screen
                string companyCode = _.CompanyCode;
                var roles = GetRoles(companyCode);

                ViewBag.Title = "New Permission";
                return View["Company/permissions.cshtml", new { Permissions = roles, Permission = new UserRoles() }];
                #endregion
            };

            Post["/new"] = _ =>
            {
                #region Create new permissions model
                CloudPanelContext db = null;
                try
                {
                    db = new CloudPanelContext(Settings.ConnectionString);

                    string companyCode = _.CompanyCode;
                    logger.DebugFormat("Creating new permissions model for {0}", companyCode);

                    var newModel = this.Bind<UserRoles>();
                    newModel.CompanyCode = companyCode;

                    db.UserRoles.Add(newModel);
                    db.SaveChanges();

                    string redirectUrl = string.Format("~/company/{0}/permissions/{1}", companyCode, newModel.RoleID);
                    return Negotiate.WithModel(new { success = "Successfully created new model" })
                                    .WithMediaRangeResponse("text/html", this.Response.AsRedirect(redirectUrl));
                }
                catch (Exception ex)
                {
                    logger.ErrorFormat("Error creating new permission model: {0}", ex.ToString());

                    ViewBag.error = ex.ToString();
                    return Negotiate.WithModel(new { error = ex.Message })
                                    .WithView("error.cshtml")
                                    .WithStatusCode(HttpStatusCode.InternalServerError);
                }
                finally
                {
                    if (db != null)
                        db.Dispose();
                }
                #endregion
            };

            Get["/{ID:int}"] = _ =>
            {
                #region Gets a specific permission model
                CloudPanelContext db = null;
                try
                {
                    db = new CloudPanelContext(Settings.ConnectionString);
                    db.Database.Connection.Open();

                    int id = _.ID;
                    string companyCode = _.CompanyCode;
                    var allPermissions = (from d in db.UserRoles
                                          where d.CompanyCode == companyCode
                                          select d).ToList();

                    var foundPermission = (from d in allPermissions
                                           where d.RoleID == id
                                           select d).FirstOrDefault();

                    ViewBag.Title = "Editing " + foundPermission.DisplayName;
                    return Negotiate.WithModel(new { Permissions = allPermissions, Permission = foundPermission })
                                    .WithView("Company/permissions.cshtml");
                }
                catch (Exception ex)
                {
                    logger.ErrorFormat("Error getting permission model {0}: {1}", _.ID, ex.ToString());

                    ViewBag.error = ex.ToString();
                    return Negotiate.WithModel(new { error = ex.Message })
                                    .WithView("error.cshtml")
                                    .WithStatusCode(HttpStatusCode.InternalServerError);
                }
                finally
                {
                    if (db != null)
                        db.Dispose();
                }
                #endregion
            };

            Post["/{ID:int}"] = _ =>
            {
                #region Updates a specific permission model
                CloudPanelContext db = null;
                try
                {
                    db = new CloudPanelContext(Settings.ConnectionString);
                    db.Database.Connection.Open();

                    int id = _.ID;
                    string companyCode = _.CompanyCode;
                    var allPermissions = (from d in db.UserRoles
                                          where d.CompanyCode == companyCode
                                          select d).ToList();

                    var foundPermission = (from d in db.UserRoles
                                           where d.RoleID == id
                                           where d.CompanyCode == companyCode
                                           select d).FirstOrDefault();

                    logger.DebugFormat("Found permission {0} to update... binding to form...", foundPermission.DisplayName);
                    var updatedPermission = this.Bind<UserRoles>();
                    foundPermission.DisplayName = updatedPermission.DisplayName;

                    logger.DebugFormat("Updating create permissions");
                    foreach (var found in updatedPermission.GetType().GetProperties())
                    {
                        if (found.PropertyType == typeof(bool))
                        {
                            logger.DebugFormat("Updating property {0} with value {1}", found.Name, found.GetValue(updatedPermission, null));
                            foundPermission.GetType()
                                           .GetProperties()
                                           .Where(x => x.Name.Equals(found.Name)).FirstOrDefault()
                                           .SetValue(foundPermission, found.GetValue(updatedPermission, null), null);
                        }
                    }
                    /*
                    logger.DebugFormat("Updating view section");
                    foundPermission.vCitrix = updatedPermission.vCitrix;
                    foundPermission.vDomains = updatedPermission.vDomains;
                    foundPermission.vExchangeContacts = updatedPermission.vExchangeContacts;
                    foundPermission.vExchangeGroups = updatedPermission.vExchangeGroups;
                    foundPermission.vExchangePublicFolders = updatedPermission.vExchangePublicFolders;
                    foundPermission.vExchangeResources = updatedPermission.vExchangeResources;
                    foundPermission.vLync = updatedPermission.vLync;
                    foundPermission.vUsers = updatedPermission.vUsers;
                    foundPermission.vUsersEdit = updatedPermission.vUsersEdit;

                    logger.DebugFormat("Updating edit section");
                    foundPermission.eCitrix = updatedPermission.eCitrix;
                    foundPermission.eDomains = updatedPermission.eDomains;
                    foundPermission.eExchangeContacts = updatedPermission.eExchangeContacts;
                    foundPermission.eExchangeGroups = updatedPermission.eExchangeGroups;
                    foundPermission.eExchangePublicFolders = updatedPermission.eExchangePublicFolders;
                    foundPermission.eExchangeResources = updatedPermission.eExchangeResources;
                    foundPermission.eLync = updatedPermission.eLync;
                    foundPermission.ePermissions = updatedPermission.ePermissions;
                    foundPermission.eUsers = updatedPermission.eUsers;

                    logger.DebugFormat("Updating delete section");
                    foundPermission.dCitrix = updatedPermission.dCitrix;
                    foundPermission.dDomains = updatedPermission.dDomains;
                    foundPermission.dExchangeContacts = updatedPermission.dExchangeContacts;
                    foundPermission.dExchangeGroups = updatedPermission.dExchangeGroups;
                    foundPermission.dExchangePublicFolders = updatedPermission.dExchangePublicFolders;
                    foundPermission.dExchangeResources = updatedPermission.dExchangeResources;
                    foundPermission.dLync = updatedPermission.dLync;
                    foundPermission.dPermissions = updatedPermission.dPermissions;
                    foundPermission.dUsers = updatedPermission.dUsers;*/

                    db.SaveChanges();

                    ViewBag.Title = "Editing " + foundPermission.DisplayName;
                    return Negotiate.WithModel(new { Permissions = allPermissions, Permission = foundPermission, success = "Successfully updated permissions" })
                                    .WithView("Company/permissions.cshtml");
                }
                catch (Exception ex)
                {
                    logger.ErrorFormat("Error getting permission model {0}: {1}", _.ID, ex.ToString());

                    ViewBag.error = ex.ToString();
                    return Negotiate.WithModel(new { error = ex.Message })
                                    .WithView("error.cshtml")
                                    .WithStatusCode(HttpStatusCode.InternalServerError);
                }
                finally
                {
                    if (db != null)
                        db.Dispose();
                }
                #endregion
            };

            Delete["/{ID:int}"] = _ =>
            {
                #region Deletes a specific permission model
                CloudPanelContext db = null;
                try
                {
                    db = new CloudPanelContext(Settings.ConnectionString);
                    db.Database.Connection.Open();

                    int id = _.ID;
                    string companyCode = _.CompanyCode;
                    var foundPermission = (from d in db.UserRoles
                                           where d.CompanyCode == companyCode
                                           where d.RoleID == id
                                           select d).FirstOrDefault();

                    logger.DebugFormat("Searching for users using the permissions group {0}", foundPermission.DisplayName);
                    var usedCount = (from d in db.UserPermission
                                     where d.RoleID == foundPermission.RoleID
                                     select d).Count();

                    if (usedCount > 0)
                        throw new Exception("Unable to remove permissions because it is in use by " + usedCount + " user(s)");
                    else
                    {
                        logger.DebugFormat("Deleting {0} for {1}", id, companyCode);
                        db.UserRoles.Remove(foundPermission);
                        db.SaveChanges();

                        string redirectUrl = string.Format("~/company/{0}/permissions/new", companyCode);
                        return Negotiate.WithModel(new { success = "Successfully removed permissions" })
                                        .WithMediaRangeResponse("text/html", this.Response.AsRedirect(redirectUrl));
                    }
                }
                catch (Exception ex)
                {
                    logger.ErrorFormat("Error deleting permission model {0}: {1}", _.ID, ex.ToString());

                    ViewBag.error = ex.ToString();
                    return Negotiate.WithModel(new { error = ex.Message })
                                    .WithView("error.cshtml")
                                    .WithStatusCode(HttpStatusCode.InternalServerError);
                }
                finally
                {
                    if (db != null)
                        db.Dispose();
                }
                #endregion
            };

        }

        public static List<UserRoles> GetRoles(string companyCode)
        {
            CloudPanelContext db = null;
            try
            {
                db = new CloudPanelContext(Settings.ConnectionString);

                logger.DebugFormat("Getting company permissions for {0}", companyCode);
                var permissions = (from d in db.UserRoles
                                   where d.CompanyCode == companyCode
                                   select d).ToList();

                return permissions;
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error getting company {0} permissions: {1}", companyCode, ex.ToString());
                return null;
            }
            finally
            {
                if (db != null)
                    db.Dispose();
            }
        }

        public static IHtmlString GetRolesWithOptions(string companyCode, int? selectedId)
        {
            var sb = new StringBuilder();

            CloudPanelContext db = null;
            try
            {
                db = new CloudPanelContext(Settings.ConnectionString);

                logger.DebugFormat("Getting company permissions for {0}", companyCode);
                var permissions = (from d in db.UserRoles
                                   where d.CompanyCode == companyCode
                                   select d).ToList();

                sb.AppendLine("<option value='0'>Not a Company Admin</option>");
                if (permissions != null)
                {
                    foreach (var p in permissions)
                    {
                        string format = string.Format("<option value='{0}' {1}>{2}</option>",
                            p.RoleID,
                            (selectedId != null && selectedId == p.RoleID) ? "value='true' selected" : "",
                            p.DisplayName);

                        sb.AppendLine(format);
                    }
                }

                return new NonEncodedHtmlString(sb.ToString());
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error getting company {0} permissions: {1}", companyCode, ex.ToString());
                return new NonEncodedHtmlString(ex.Message);
            }
            finally
            {
                if (db != null)
                    db.Dispose();
            }
        }
    }
}
using CloudPanel.ActiveDirectory;
using CloudPanel.Base.AD;
using CloudPanel.Base.Config;
using CloudPanel.Base.Database.Models;
using CloudPanel.Citrix;
using CloudPanel.Database.EntityFramework;
using CloudPanel.Rollback;
using log4net;
using Nancy;
using System;
using System.Data.Entity;
using System.Linq;
using System.Reflection;

namespace CloudPanel.Modules.PlansModules
{
    public class CitrixPlanModule : NancyModule
    {
        private static readonly ILog logger = LogManager.GetLogger("Citrix");

        public CitrixPlanModule() : base("/plans/citrix")
        {
            Get["/", c => c.Request.Accept("text/html")] = _ =>
                {
                    return View["Plans/Citrix/citrix.cshtml"];
                };

            Get["/", c => !c.Request.Accept("text/html")] = _ =>
                {
                    #region Get the desktop groups, its companies, and users from the database
                    CloudPanelContext db = null;
                    try
                    {
                        db = new CloudPanelContext(Settings.ConnectionString);
                        db.Database.Connection.Open();

                        logger.DebugFormat("Retrieving the desktop groups");
                        var desktopGroups = (from d in db.CitrixDesktopGroup
                                             orderby d.Name
                                             select new
                                             {
                                                 Uid = d.Uid,
                                                 UUID = d.UUID,
                                                 DesktopGroupID = d.DesktopGroupID,
                                                 Name = d.Name,
                                                 PublishedName = d.PublishedName,
                                                 SecurityGroup = d.SecurityGroup,
                                                 Description = d.Description,
                                                 LastRetrieved = d.LastRetrieved,
                                                 ApplicationCount = d.Applications.Count,
                                                 DesktopCount = d.Desktops.Count,
                                                 CompanyCount = d.Companies.Count,
                                                 Companies = (from d2 in d.Companies
                                                              select new
                                                              {
                                                                  CompanyCode = d2.CompanyCode,
                                                                  CompanyName = d2.CompanyName,
                                                                  TotalUsers = d.Users.Where(x => x.CompanyCode == d2.CompanyCode).Count()
                                                              }).ToList(),
                                                 TotalUsers = d.Users.Count
                                             }).ToList();

                        int draw = 0, start = 0, length = 0, recordsTotal = desktopGroups.Count, recordsFiltered = desktopGroups.Count, orderColumn = 0;
                        string searchValue = "", orderColumnName = "";
                        bool isAscendingOrder = true;

                        if (Request.Query.draw.HasValue)
                        {
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
                                desktopGroups = (from d in desktopGroups
                                                 where d.Name.IndexOf(searchValue, StringComparison.InvariantCultureIgnoreCase) != -1 ||
                                                       d.PublishedName.IndexOf(searchValue, StringComparison.InvariantCultureIgnoreCase) != -1 ||
                                                       d.Companies.Any(x =>
                                                                         x.CompanyCode.IndexOf(searchValue, StringComparison.InvariantCultureIgnoreCase) > 0 ||
                                                                         x.CompanyName.IndexOf(searchValue, StringComparison.InvariantCultureIgnoreCase) > 0
                                                                       )
                                                 select d).ToList();
                                recordsFiltered = desktopGroups.Count;
                            }

                            if (isAscendingOrder)
                                desktopGroups = desktopGroups.OrderBy(x => x.GetType()
                                                        .GetProperty(orderColumnName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance).GetValue(x, null))
                                                        .Skip(start)
                                                        .Take((length > 0 ? length : desktopGroups.Count))
                                                        .ToList();
                            else
                                desktopGroups = desktopGroups.OrderByDescending(x => x.GetType()
                                                        .GetProperty(orderColumnName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance).GetValue(x, null))
                                                        .Skip(start)
                                                        .Take((length > 0 ? length : desktopGroups.Count))
                                                        .ToList();
                        }

                        logger.DebugFormat("Completed getting Citrix data");
                        return Negotiate.WithModel(new
                                        {
                                            draw = draw,
                                            recordsTotal = recordsTotal,
                                            recordsFiltered = recordsFiltered,
                                            data = desktopGroups
                                        });
                    }
                    catch (Exception ex)
                    {
                        logger.ErrorFormat("Error getting citrix data: {0}", ex.ToString());
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

            Get["/group/{UUID:Guid}"] = _ =>
                {
                    #region Get the desktop groups, its companies, and users from the database
                    CloudPanelContext db = null;
                    try
                    {
                        db = new CloudPanelContext(Settings.ConnectionString);
                        db.Database.Connection.Open();

                        logger.DebugFormat("Retrieving the desktop groups");
                        Guid desktopUUID = _.UUID;
                        var desktopGroups = (from d in db.CitrixDesktopGroup
                                                         .Include(x => x.Companies)
                                                         .Include(x => x.Users)
                                                         .Include(x => x.Applications)
                                                         .Include(x => x.Desktops)
                                             where d.UUID == d.UUID
                                             select new
                                             {
                                                 Uid = d.Uid,
                                                 UUID = d.UUID,
                                                 DesktopGroupID = d.DesktopGroupID,
                                                 Name = d.Name,
                                                 PublishedName = d.PublishedName,
                                                 Description = d.Description,
                                                 LastRetrieved = d.LastRetrieved,
                                                 Applications = (from app in d.Applications
                                                                 select new
                                                                 {
                                                                     Uid = app.Uid,
                                                                     UUID = app.UUID,
                                                                     ApplicationName = app.ApplicationName,
                                                                     PublishedName = app.PublishedName,
                                                                     CommandLineExecutable = app.CommandLineExecutable,
                                                                     CommandLineArguments = app.CommandLineArguments,
                                                                     Description = app.Description,
                                                                     IsEnabled = app.IsEnabled,
                                                                     LastRetrieved = app.LastRetrieved
                                                                 }).ToList(),
                                                 Desktops = (from desktop in d.Desktops
                                                             select new
                                                             {
                                                                 Uid = desktop.Uid,
                                                                 SID = desktop.SID,
                                                                 DNSName = desktop.DNSName,
                                                                 IPAddress = desktop.IPAddress,
                                                                 InMaintenanceMode = desktop.InMaintenanceMode,
                                                                 MachineName = desktop.MachineName
                                                             }).ToList(),
                                                 Companies = (from d2 in d.Companies
                                                              select new
                                                              {
                                                                  CompanyCode = d2.CompanyCode,
                                                                  CompanyName = d2.CompanyName,
                                                                  TotalUsers = d.Users.Where(x => x.CompanyCode == d2.CompanyCode).Count()
                                                              }).ToList(),
                                                 TotalUsers = d.Users.Count
                                             }).First();

                        logger.DebugFormat("Completed getting Citrix data");
                        return Negotiate.WithView("Plans/Citrix/group.cshtml")
                                        .WithModel(desktopGroups)
                                        .WithStatusCode(HttpStatusCode.OK);
                    }
                    catch (Exception ex)
                    {
                        logger.ErrorFormat("Error getting citrix data: {0}", ex.ToString());
                        return Negotiate.WithModel(new { error = ex.Message })
                                        .WithStatusCode(HttpStatusCode.InternalServerError)
                                        .WithView("Error/500.cshtml");
                    }
                    finally
                    {
                        if (db != null)
                            db.Dispose();
                    }
                    #endregion
                };

            Get["/all"] = _ =>
                {
                    #region Queries Citrix and adds/updates all data
                    XenDesktop7 xd7 = null;
                    try
                    {
                        xd7 = new XenDesktop7(Settings.CitrixUri, Settings.Username, Settings.DecryptedPassword);

                        // Get desktop groups
                        var desktopGroups = xd7.GetDesktopGroups();
                        if (desktopGroups != null)
                        {
                            desktopGroups.ForEach(x =>
                                {
                                    var desktops = xd7.GetDesktops(x.Uid);
                                    if (desktops != null)
                                        x.Desktops = desktops;

                                    var apps = xd7.GetApplications(x.Uid);
                                    if (apps != null)
                                        x.Applications = apps;

                                    AddDesktopGroup(x, ref xd7);
                                });
                        }

                        return Negotiate.WithModel(new { success = "Successfully reloaded data from Citrix" })
                                        .WithStatusCode(HttpStatusCode.OK);
                    }
                    catch (Exception ex)
                    {
                        logger.ErrorFormat("Error getting all Citrix data: {0}", ex.ToString());
                        return Negotiate.WithModel(new { error = ex.Message })
                                        .WithStatusCode(HttpStatusCode.InternalServerError);
                    }
                    finally
                    {
                        if (xd7 != null)
                            xd7.Dispose();
                    }
                    #endregion
                };

            Post["/group/{UUID:Guid}/add"] = _ =>
                {
                    Guid uuid = _.UUID;

                    #region Add company to delivery group
                    CloudPanelContext db = null;
                    try
                    {
                        if (!Request.Form.CompanyCode.HasValue)
                            throw new MissingFieldException("", "CompanyCode");

                        db = new CloudPanelContext(Settings.ConnectionString);
                        db.Database.Connection.Open();

                        string companyCode = Request.Form.CompanyCode.Value;

                        var company = db.Companies.Where(x => x.CompanyCode == companyCode).FirstOrDefault();
                        var desktopGroup = db.CitrixDesktopGroup.Where(x => x.UUID == uuid).FirstOrDefault();
                        desktopGroup.Companies.Add(company);
                        db.SaveChanges();

                        return Negotiate.WithModel(new { success = "Successfully added company to the desktop group" })
                                        .WithStatusCode(HttpStatusCode.OK);
                    }
                    catch (Exception ex)
                    {
                        logger.ErrorFormat("Error adding company to desktop group {0}", uuid);
                        return Negotiate.WithModel(new { error = ex.Message })
                                        .WithView("Error/500.cshtml")
                                        .WithStatusCode(HttpStatusCode.InternalServerError);
                    }
                    finally
                    {
                        if (db != null)
                            db.Dispose();
                    }
                    #endregion
                };

            Post["/group/{UUID:Guid}/delete"] = _ =>
                {
                    Guid uuid = _.UUID;

                    #region Remove company to delivery group
                    CloudPanelContext db = null;
                    try
                    {
                        if (!Request.Form.CompanyCode.HasValue)
                            throw new MissingFieldException("", "CompanyCode");

                        db = new CloudPanelContext(Settings.ConnectionString);
                        db.Database.Connection.Open();

                        string companyCode = Request.Form.CompanyCode.Value;

                        var company = db.Companies.Where(x => x.CompanyCode == companyCode).FirstOrDefault();
                        var desktopGroup = db.CitrixDesktopGroup.Where(x => x.UUID == uuid).FirstOrDefault();
                        desktopGroup.Companies.Remove(company);
                        db.SaveChanges();

                        return Negotiate.WithModel(new { success = "Successfully removed company from the desktop group" })
                                        .WithStatusCode(HttpStatusCode.OK);
                    }
                    catch (Exception ex)
                    {
                        logger.ErrorFormat("Error removing company from desktop group {0}", uuid);
                        return Negotiate.WithModel(new { error = ex.Message })
                                        .WithView("Error/500.cshtml")
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

        public static void AddDesktopGroup(CitrixDesktopGroups desktopGroup, ref XenDesktop7 xd7)
        {
            CloudPanelContext db = null;
            ADGroups ad = null;
            ReverseActions reverse = new ReverseActions();
            try
            {
                db = new CloudPanelContext(Settings.ConnectionString);
                db.Database.Connection.Open();

                var existingGroup = (from d in db.CitrixDesktopGroup
                                                 .Include(x => x.Applications)
                                                 .Include(x => x.Desktops)
                                     where d.UUID == desktopGroup.UUID
                                     select d).FirstOrDefault();

                if (existingGroup == null)
                {
                    #region New desktop group
                    logger.DebugFormat("Desktop group is new. Creating security group");
                    var group = CreateSecurityGroup(desktopGroup.Name, desktopGroup.UUID.ToString());
                    reverse.AddAction(Actions.CreateSecurityGroup, group.Name);

                    logger.DebugFormat("Adding the new security group to the desktop group");
                    xd7.AddGroupOrUserToDesktopGroup(desktopGroup.Uid, group.SamAccountName);
                    desktopGroup.SecurityGroup = group.Name;

                    logger.DebugFormat("Checking applications if they are user filter enabled to create a security group");
                    if (desktopGroup.Applications != null)
                    {
                        foreach (var app in desktopGroup.Applications)
                        {
                            if (app.UserFilterEnabled)
                            {
                                // Add security group to the application
                                var appGroup = CreateSecurityGroup(app.Name, app.UUID.ToString());
                                reverse.AddAction(Actions.CreateSecurityGroup, appGroup.Name);

                                logger.DebugFormat("Adding the new security group to the application");
                                xd7.AddGroupOrUserToApplication(app.Uid, appGroup.SamAccountName);

                                // Add to database
                                app.SecurityGroup = appGroup.Name;
                            }
                        }
                    }

                    logger.DebugFormat("Adding to database");
                    db.CitrixDesktopGroup.Add(desktopGroup);
                    #endregion
                }
                else
                {
                    #region Update existing group
                    logger.DebugFormat("Updating desktop group {0}", desktopGroup.Name);
                    existingGroup.Name = desktopGroup.Name;
                    existingGroup.PublishedName = desktopGroup.PublishedName;
                    existingGroup.IsEnabled = desktopGroup.IsEnabled;
                    existingGroup.Description = desktopGroup.Description;
                    existingGroup.LastRetrieved = desktopGroup.LastRetrieved;

                    // Check if the security group is null
                    if (string.IsNullOrEmpty(existingGroup.SecurityGroup))
                    {
                        logger.DebugFormat("Desktop group is existing but missing security group. Creating security group");
                        var group = CreateSecurityGroup(existingGroup.Name, existingGroup.UUID.ToString());
                        reverse.AddAction(Actions.CreateSecurityGroup, group.Name);

                        logger.DebugFormat("Adding the new security group to the desktop group");
                        xd7.AddGroupOrUserToDesktopGroup(existingGroup.Uid, group.SamAccountName);

                        existingGroup.SecurityGroup = group.Name;
                    }

                    #region Update desktops
                    if (desktopGroup.Desktops != null && desktopGroup.Desktops.Count > 0)
                    {
                        logger.DebugFormat("Updating total of {0} desktops", desktopGroup.Desktops.Count);

                        var deletedDesktops = existingGroup.Desktops.Except(desktopGroup.Desktops, x => x.SID).ToList<CitrixDesktops>();
                        logger.DebugFormat("Removing a total of {0} desktops", deletedDesktops.Count);
                        deletedDesktops.ForEach(x =>
                        {
                            var child = existingGroup.Desktops.Where(a => a.SID == x.SID).First();
                            var ef = db.Entry(child);
                            ef.State = EntityState.Deleted;
                        });

                        var addedDesktops = desktopGroup.Desktops.Except(existingGroup.Desktops, x => x.SID).ToList<CitrixDesktops>();
                        logger.DebugFormat("Adding a total of {0} desktops", addedDesktops.Count);
                        addedDesktops.ForEach(x => existingGroup.Desktops.Add(x));
                    }
                    else
                        logger.DebugFormat("Desktop group desktops was null");
                    #endregion

                    #region Update applications
                    if (desktopGroup.Applications != null & desktopGroup.Applications.Count > 0)
                    {
                        logger.DebugFormat("Updating total of {0} applications", desktopGroup.Applications.Count);

                        //
                        // Delete applications that no longer exist
                        //
                        var deletedApplications = existingGroup.Applications.Except(desktopGroup.Applications, x => x.UUID).ToList<CitrixApplications>();
                        logger.DebugFormat("Removing a total of {0} applications", deletedApplications.Count);
                        deletedApplications.ForEach(x =>
                            {
                                var child = existingGroup.Applications.Where(a => a.UUID == x.UUID).First();
                                var ef = db.Entry(child);
                                ef.State = EntityState.Deleted;
                            });
                        db.SaveChanges();

                        //
                        // Add the new applications
                        //
                        var addedApplications = desktopGroup.Applications.Except(existingGroup.Applications, x => x.UUID).ToList<CitrixApplications>();
                        logger.DebugFormat("Adding a total of {0} applications", addedApplications.Count);
                        foreach (var app in addedApplications)
                        {
                            if (app.UserFilterEnabled)
                            {
                                // Add security group to the application
                                var appGroup = CreateSecurityGroup(app.Name, app.UUID.ToString());
                                reverse.AddAction(Actions.CreateSecurityGroup, appGroup.Name);

                                logger.DebugFormat("Adding the new security group to the application");
                                xd7.AddGroupOrUserToApplication(app.Uid, appGroup.SamAccountName);

                                // Add to database
                                app.SecurityGroup = appGroup.Name;
                            }
                            else
                                app.SecurityGroup = string.Empty;

                            existingGroup.Applications.Add(app);
                        }

                        //
                        // Verify the applications that haven't changed
                        //
                        var existingApplications = existingGroup.Applications.Except(addedApplications, x => x.UUID).ToList<CitrixApplications>();
                        logger.DebugFormat("Updating a total of {0} applications", existingApplications.Count);
                        foreach (var app in existingApplications)
                        {
                            var updatedApp = desktopGroup.Applications.Where(x => x.UUID == app.UUID).First();
                            app.Name = updatedApp.Name;
                            app.PublishedName = updatedApp.PublishedName;
                            app.CommandLineExecutable = updatedApp.CommandLineExecutable;
                            app.CommandLineArguments = updatedApp.CommandLineArguments;
                            app.ShortcutAddedToDesktop = updatedApp.ShortcutAddedToDesktop;
                            app.ShortcutAddedToStartMenu = updatedApp.ShortcutAddedToStartMenu;
                            app.LastRetrieved = updatedApp.LastRetrieved;
                            app.UserFilterEnabled = updatedApp.UserFilterEnabled;

                            if (updatedApp.UserFilterEnabled && string.IsNullOrEmpty(app.SecurityGroup))
                            {
                                // Add security group to the application
                                var appGroup = CreateSecurityGroup(app.Name, app.UUID.ToString());
                                reverse.AddAction(Actions.CreateSecurityGroup, appGroup.Name);

                                logger.DebugFormat("Adding the new security group to the application");
                                xd7.AddGroupOrUserToApplication(app.Uid, appGroup.SamAccountName);

                                // Add to database
                                app.SecurityGroup = appGroup.Name;
                            }
                            else if (!updatedApp.UserFilterEnabled)
                                app.SecurityGroup = string.Empty; // Clear the security group if no longer filtered
                        }
                    }
                    else
                    {
                        logger.DebugFormat("Desktop group applications were null");
                        existingGroup.Applications = null;
                    }
                    #endregion

                    #endregion
                }

                db.SaveChanges();
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error importing desktop groups {0}", ex.ToString());
                reverse.RollbackNow();
                throw;
            }
            finally
            {
                if (ad != null)
                    ad.Dispose();

                if (db != null)
                    db.Dispose();
            }
        }

        public static void AddDesktop(CitrixDesktops desktop)
        {
            CloudPanelContext db = null;
            try
            {
                db = new CloudPanelContext(Settings.ConnectionString);
                db.Database.Connection.Open();

                var existingDesktop = (from d in db.CitrixDesktop
                                       where d.SID == desktop.SID
                                       select d).FirstOrDefault();

                if (existingDesktop == null)
                    db.CitrixDesktop.Add(desktop);
                else
                {
                    existingDesktop.MachineName = desktop.MachineName;
                    existingDesktop.IPAddress = desktop.IPAddress;
                    existingDesktop.OSVersion = desktop.OSVersion;
                    existingDesktop.OSType = desktop.OSType;
                    existingDesktop.InMaintenanceMode = desktop.InMaintenanceMode;
                    existingDesktop.DNSName = desktop.DNSName;
                    existingDesktop.CatalogName = desktop.CatalogName;
                    existingDesktop.AgentVersion = desktop.AgentVersion;

                    if (desktop.DesktopGroup != null)
                        existingDesktop.DesktopGroup = desktop.DesktopGroup;
                }

                db.SaveChanges();
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                if (db != null)
                    db.Dispose();
            }
        }

        private static SecurityGroup CreateSecurityGroup(string name, string uuid)
        {
            ADGroups groups = null;
            try
            {
                groups = new ADGroups(Settings.Username, Settings.DecryptedPassword, Settings.PrimaryDC);

                SecurityGroup newGroup = groups.Create(Settings.ApplicationOuPath(Settings.HostingOU),
                                                       new SecurityGroup()
                                                       {
                                                            SamAccountName = uuid.Substring(0, 18),
                                                            Name = uuid,
                                                            DisplayName = uuid,
                                                            Description = string.Format("{0} - Created by CloudPanel", name)
                                                       });
                return newGroup;
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error creating security group for Citrix: {0}", ex.ToString());
                throw;
            }
            finally
            {
                if (groups != null)
                    groups.Dispose();
            }
        }

    }
}
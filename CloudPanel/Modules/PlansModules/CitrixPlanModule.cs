using CloudPanel.Base.Config;
using CloudPanel.Citrix;
using CloudPanel.Database.EntityFramework;
using log4net;
using Nancy;
using Nancy.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Data.Entity;
using CloudPanel.Base.Database.Models;
using Nancy.ViewEngines.Razor;
using System.Text;
using CloudPanel.Code;

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
                                                        .Take( (length > 0 ? length : desktopGroups.Count) )
                                                        .ToList();
                            else
                                desktopGroups = desktopGroups.OrderByDescending(x => x.GetType()
                                                        .GetProperty(orderColumnName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance).GetValue(x, null))
                                                        .Skip(start)
                                                        .Take( (length > 0 ? length : desktopGroups.Count) )
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

            Get["/groups/{GroupID:guid}"] = _ =>
                {
                    #region Get the desktop groups, its companies, and users from the database
                    CloudPanelContext db = null;
                    try
                    {
                        db = new CloudPanelContext(Settings.ConnectionString);
                        db.Database.Connection.Open();

                        logger.DebugFormat("Retrieving the desktop groups");
                        Guid desktopUUID = _.GroupID;
                        var desktopGroups = (from d in db.CitrixDesktopGroup
                                                         .Include(x => x.Companies)
                                                         .Include(x => x.Users)
                                                         .Include(x => x.Applications)
                                                         .Include(x => x.Desktops)
                                             where d.UUID == desktopUUID
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
                        return Negotiate.WithView("Plans/Citrix/groups.cshtml")
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

            Get["/groups/{GroupID:guid}/{DesktopUid:int}/sessions"] = _ =>
                {
                    #region Gets the sessions for a specific desktop group
                    XenDesktop7 xd7 = null;
                    try
                    {
                        xd7 = new XenDesktop7(Settings.CitrixUri, Settings.Username, Settings.DecryptedPassword);

                        int uid = _.DesktopUid;
                        var sessions = xd7.GetSessions(uid);

                        int draw = 0, start = 0, length = 0, recordsTotal = sessions.Count, recordsFiltered = sessions.Count, orderColumn = 0;
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
                                sessions = (from d in sessions
                                            where d.UserName.IndexOf(searchValue, StringComparison.InvariantCultureIgnoreCase) != -1
                                            select d).ToList();
                                recordsFiltered = sessions.Count;
                            }

                            if (isAscendingOrder)
                                sessions = sessions.OrderBy(x => x.GetType()
                                                        .GetProperty(orderColumnName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance).GetValue(x, null))
                                                        .Skip(start)
                                                        .Take((length > 0 ? length : sessions.Count))
                                                        .ToList();
                            else
                                sessions = sessions.OrderByDescending(x => x.GetType()
                                                        .GetProperty(orderColumnName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance).GetValue(x, null))
                                                        .Skip(start)
                                                        .Take((length > 0 ? length : sessions.Count))
                                                        .ToList();
                        }

                        logger.DebugFormat("Completed getting citrix session data");
                        return Negotiate.WithModel(new
                        {
                            draw = draw,
                            recordsTotal = recordsTotal,
                            recordsFiltered = recordsFiltered,
                            data = sessions
                        });
                    }
                    catch (Exception ex)
                    {
                        logger.ErrorFormat("Error getting citrix session data: {0}", ex.ToString());
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

                                    AddDesktopGroup(x);
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
        }

        public static void AddDesktopGroup(CitrixDesktopGroups desktopGroup)
        {
            CloudPanelContext db = null;
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
                    logger.DebugFormat("Desktop group is new. Adding to database");
                    db.CitrixDesktopGroup.Add(desktopGroup);
                }
                else
                {
                    logger.DebugFormat("Updating desktop group {0}", desktopGroup.Name);
                    existingGroup.Name = desktopGroup.Name;
                    existingGroup.PublishedName = desktopGroup.PublishedName;
                    existingGroup.IsEnabled = desktopGroup.IsEnabled;
                    existingGroup.Description = desktopGroup.Description;
                    existingGroup.LastRetrieved = desktopGroup.LastRetrieved;

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

                    if (desktopGroup.Applications != null & desktopGroup.Applications.Count > 0)
                    {
                        logger.DebugFormat("Updating total of {0} applications", desktopGroup.Applications.Count);
                        existingGroup.Applications = desktopGroup.Applications;
                    }
                    else
                        logger.DebugFormat("Desktop group applications were null");
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

    }
}
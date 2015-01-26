using CloudPanel.ActiveDirectory;
using CloudPanel.Base.AD;
using CloudPanel.Base.Config;
using CloudPanel.Base.Database.Models;
using CloudPanel.Citrix;
using CloudPanel.Code;
using CloudPanel.Database.EntityFramework;
using CloudPanel.Rollback;
using log4net;
using Nancy;
using Nancy.Security;
using System;
using System.Data.Entity;
using System.Linq;
using System.Reflection;

namespace CloudPanel.Modules.Citrix
{
    public class CitrixModule : NancyModule
    {
        private static readonly ILog logger = LogManager.GetLogger("Citrix");

        public CitrixModule() : base("/citrix")
        {
            this.RequiresAuthentication();

            Get["/", c => c.Request.Accept("text/html")] = _ =>
            {
                this.RequiresClaims(new[] { "SuperAdmin" });
                return View["Citrix/citrix.cshtml"];
            };

            Get["/", c => !c.Request.Accept("text/html")] = _ =>
            {
                this.RequiresClaims(new[] { "SuperAdmin" });
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

            Get["/sessions", c => c.Request.Accept("text/html")] = _ =>
            {
                this.RequiresClaims(new[] { "SuperAdmin" });
                return View["Citrix/sessions.cshtml"];
            };

            Get["/sessions", c => !c.Request.Accept("text/html")] = _ =>
            {
                this.RequiresClaims(new[] { "SuperAdmin" });
                string companyCode = _.CompanyCode;

                #region Gets sessions for all destop groups the user is assigned
                XenDesktop7 xd7 = null;
                try
                {
                   
                    xd7 = new XenDesktop7(Settings.CitrixUri, Settings.Username, Settings.DecryptedPassword);
                    var brokerSessions = xd7.GetSessions();

                    logger.DebugFormat("Found a total of {0} sessions in Citrix", brokerSessions.Count);
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
                }
                #endregion
            };

            Get["/all"] = _ =>
            {
                this.RequiresClaims(new[] { "SuperAdmin" });
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

            Get["/group/{UUID:Guid}"] = _ =>
            {
                this.RequiresClaims(new[] { "SuperAdmin" });
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
                                                                 CommandLineExecutable = app.CommandLineExecutable,
                                                                 CommandLineArguments = app.CommandLineArguments,
                                                                 Description = app.Description,
                                                                 IsEnabled = app.IsEnabled,
                                                                 LastRetrieved = app.LastRetrieved,
                                                                 UserFilterEnabled = app.UserFilterEnabled,
                                                                 SecurityGroup = app.SecurityGroup
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
                    return Negotiate.WithView("Citrix/group.cshtml")
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

            Get["/group/{UUID:Guid}/sessions"] = _ =>
            {
                this.RequiresClaims(new[] { "SuperAdmin" });

                Guid uuid = _.UUID;
                #region Gets the sessions for a specific desktop group
                XenDesktop7 xd7 = null;
                CloudPanelContext db = null;
                try
                {
                    db = new CloudPanelContext(Settings.ConnectionString);

                    var desktopGroup = db.CitrixDesktopGroup.Where(x => x.UUID == uuid).First();

                    xd7 = new XenDesktop7(Settings.CitrixUri, Settings.Username, Settings.DecryptedPassword);
                    var sessions = xd7.GetSessionsByDesktopGroup(desktopGroup.Uid);

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

            Get["/desktop/{DESKTOP:int}/sessions"] = _ =>
            {
                this.RequiresClaims(new[] { "SuperAdmin" });

                int desktopid = _.DESKTOP;
                #region Gets the sessions for a specific desktop
                XenDesktop7 xd7 = null;
                CloudPanelContext db = null;
                try
                {
                    db = new CloudPanelContext(Settings.ConnectionString);

                    var desktop = db.CitrixDesktop
                                    .Where(x => x.Uid == desktopid)
                                    .First();

                    xd7 = new XenDesktop7(Settings.CitrixUri, Settings.Username, Settings.DecryptedPassword);
                    var sessions = xd7.GetSessionsByDesktop(desktop.Uid);

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

            Post["/logoff"] = _ =>
            {
                this.RequiresValidatedClaims(c => ValidateClaims.AllowCompanyAdmin(Context.CurrentUser,"eCitrix"));
                #region Logs off users
                XenDesktop7 xd7 = null;
                try
                {
                    if (!Request.Form["values[]"].HasValue)
                        throw new MissingFieldException("", "values[]");

                    xd7 = new XenDesktop7(Settings.CitrixUri, Settings.Username, Settings.DecryptedPassword);

                    string[] values = Request.Form["values[]"].Value.Split(',');
                    xd7.LogOffSessionsBySessionKeys(values);

                    return Negotiate.WithModel(new { success = "Successfully sent command to log off users" })
                                    .WithStatusCode(HttpStatusCode.OK);
                }
                catch (Exception ex)
                {
                    logger.ErrorFormat("Error logging off users: {0}", ex.ToString());
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

            Post["/sendmessage"] = _ =>
            {
                this.RequiresValidatedClaims(c => ValidateClaims.AllowCompanyAdmin(Context.CurrentUser, "eCitrix"));
                #region Send message to users
                XenDesktop7 xd7 = null;
                try
                {
                    if (!Request.Form.Title.HasValue)
                        throw new MissingFieldException("", "Title");

                    if (!Request.Form.MessageStyle.HasValue)
                        throw new MissingFieldException("", "MessageStyle");

                    if (!Request.Form.Message.HasValue)
                        throw new MissingFieldException("", "Message");

                    if (!Request.Form["SessionKeys"].HasValue)
                        throw new MissingFieldException("", "SessionKeys");

                    xd7 = new XenDesktop7(Settings.CitrixUri, Settings.Username, Settings.DecryptedPassword);

                    string[] sessionKeys = Request.Form["SessionKeys"].Value.Split(',');
                    xd7.SendMessageBySessionKeys(sessionKeys,
                                                 Request.Form.MessageStyle.Value,
                                                 Request.Form.Title.Value,
                                                 Request.Form.Message.Value);

                    return Negotiate.WithModel(new { success = "Successfully sent message to users" })
                                    .WithStatusCode(HttpStatusCode.OK);
                }
                catch (Exception ex)
                {
                    logger.ErrorFormat("Error sending message to users: {0}", ex.ToString());
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

            Post["/app/{UUID:Guid}/securitygroup"] = _ =>
            {
                this.RequiresClaims(new[] { "SuperAdmin" });
                #region Updates the security group for a specific application
                Guid uuid = _.UUID;
                CloudPanelContext db = null;
                try
                {
                    if (!Request.Form.SecurityGroup.HasValue)
                        throw new MissingFieldException("", "SecurityGroup");

                    db = new CloudPanelContext(Settings.ConnectionString);

                    var app = db.CitrixApplication
                                .Where(x => x.UUID == uuid && x.UserFilterEnabled)
                                .FirstOrDefault();
                    app.SecurityGroup = Request.Form.SecurityGroup.Value;
                    db.SaveChanges();

                    return Negotiate.WithModel(new { success = "Successfully updated security group for the application" })
                                    .WithStatusCode(HttpStatusCode.OK);
                }
                catch (Exception ex)
                {
                    logger.ErrorFormat("Error updating security group for the application {0}", uuid);
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

            Post["/group/{UUID:Guid}/securitygroup"] = _ =>
            {
                this.RequiresClaims(new[] { "SuperAdmin" });

                #region Updates the security group for a specific desktop group
                Guid uuid = _.UUID;
                CloudPanelContext db = null;
                try
                {
                    if (!Request.Form.SecurityGroup.HasValue)
                        throw new MissingFieldException("", "SecurityGroup");

                    db = new CloudPanelContext(Settings.ConnectionString);

                    var desktopGroup = db.CitrixDesktopGroup
                                         .Where(x => x.UUID == uuid)
                                         .FirstOrDefault();
                    desktopGroup.SecurityGroup = Request.Form.SecurityGroup.Value;
                    db.SaveChanges();

                    return Negotiate.WithModel(new { success = "Successfully updated security group for desktop group" })
                                    .WithStatusCode(HttpStatusCode.OK);
                }
                catch (Exception ex)
                {
                    logger.ErrorFormat("Error updating security group for desktop group {0}", uuid);
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

            Post["/group/{UUID:Guid}/add"] = _ =>
            {
                this.RequiresClaims(new[] { "SuperAdmin" });

                #region Add company to delivery group
                Guid uuid = _.UUID;
                CloudPanelContext db = null;
                ADGroups groups = null;
                ReverseActions reverse = new ReverseActions();
                try
                {
                    if (!Request.Form.CompanyCode.HasValue)
                        throw new MissingFieldException("", "CompanyCode");

                    db = new CloudPanelContext(Settings.ConnectionString);
                    db.Database.Connection.Open();

                    string companyCode = Request.Form.CompanyCode.Value;

                    // Get the company so we have the ldap path for the security group
                    var company = db.Companies.Where(x => x.CompanyCode == companyCode).Single();

                    // Get the desktop group so we can name the new security group
                    var desktopGroup = db.CitrixDesktopGroup.Where(x => x.UUID == uuid).Single();

                    // Get the company's application OU path
                    string appPath = Settings.ApplicationOuPath(company.DistinguishedName);
                    logger.DebugFormat("Application path is {0}", appPath);

                    // Create the new security group object
                    CitrixSecurityGroups newGroup = new CitrixSecurityGroups();
                    newGroup.GroupName = string.Format("{0}@{1}", desktopGroup.SecurityGroup, companyCode);
                    newGroup.Description = string.Format("{0}'s security group for desktop group {1}", company.CompanyName, desktopGroup.Name);
                    newGroup.CompanyCode = companyCode;
                    newGroup.DesktopGroupID = desktopGroup.DesktopGroupID;

                    // Add the group to Active Directory
                    groups = new ADGroups(Settings.Username, Settings.DecryptedPassword, Settings.PrimaryDC);
                    groups.Create(appPath, new SecurityGroup()
                    {
                        Name = newGroup.GroupName,
                        DisplayName = newGroup.GroupName,
                        Description = newGroup.Description,
                        SamAccountName = newGroup.GroupName
                    });
                    reverse.AddAction(Actions.CreateSecurityGroup, newGroup.GroupName);

                    // Add the group to the company's AllTSUsers security group
                    groups.AddGroup("AllTSUsers@" + companyCode, newGroup.GroupName);

                    // Add the new group to the desktop group security group
                    groups.AddGroup(desktopGroup.SecurityGroup, newGroup.GroupName);

                    // Add new group to database
                    db.CitrixSecurityGroup.Add(newGroup);

                    // Add the company to the desktop group and save
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
                this.RequiresClaims(new[] { "SuperAdmin" });

                #region Remove company to delivery group
                Guid uuid = _.UUID;
                CloudPanelContext db = null;
                ADGroups groups = null;
                try
                {
                    if (!Request.Form.CompanyCode.HasValue)
                        throw new MissingFieldException("", "CompanyCode");

                    db = new CloudPanelContext(Settings.ConnectionString);
                    db.Database.Connection.Open();

                    string companyCode = Request.Form.CompanyCode.Value;

                    // Get the company from the database
                    var company = db.Companies.Where(x => x.CompanyCode == companyCode).Single();

                    // Get the desktop group from the database
                    var desktopGroup = db.CitrixDesktopGroup.Where(x => x.UUID == uuid).Single();

                    // Get the security group for the company relating to this desktop group
                    var securityGroup = db.CitrixSecurityGroup.Where(x => x.DesktopGroupID == desktopGroup.DesktopGroupID && x.CompanyCode == companyCode).FirstOrDefault();

                    if (securityGroup != null)
                    {
                        // Delete the security group from Active Directory
                        groups = new ADGroups(Settings.Username, Settings.DecryptedPassword, Settings.PrimaryDC);
                        groups.Delete(securityGroup.GroupName);

                        // Remove the security group from the database
                        db.CitrixSecurityGroup.Remove(securityGroup);
                    }

                    // Remove the company from the desktop group
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
                    //var group = CreateSecurityGroup(desktopGroup.Name, desktopGroup.UUID.ToString());
                    //reverse.AddAction(Actions.CreateSecurityGroup, group.Name);

                    //logger.DebugFormat("Adding the new security group to the desktop group");
                    //xd7.AddGroupOrUserToDesktopGroup(desktopGroup.Uid, group.SamAccountName);
                    //desktopGroup.SecurityGroup = group.Name;

                    logger.DebugFormat("Checking applications if they are user filter enabled to create a security group");
                    if (desktopGroup.Applications != null)
                    {
                        foreach (var app in desktopGroup.Applications)
                        {
                            if (app.UserFilterEnabled)
                            {
                                // Add security group to the application
                                //var appGroup = CreateSecurityGroup(app.Name, app.UUID.ToString());
                                //reverse.AddAction(Actions.CreateSecurityGroup, appGroup.Name);

                                //logger.DebugFormat("Adding the new security group to the application");
                                //xd7.AddGroupOrUserToApplication(app.Uid, appGroup.SamAccountName);

                                // Add to database
                                //app.SecurityGroup = appGroup.Name;
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

                    /* Check if the security group is null
                    if (string.IsNullOrEmpty(existingGroup.SecurityGroup))
                    {
                        logger.DebugFormat("Desktop group is existing but missing security group. Creating security group");
                        var group = CreateSecurityGroup(existingGroup.Name, existingGroup.UUID.ToString());
                        reverse.AddAction(Actions.CreateSecurityGroup, group.Name);

                        logger.DebugFormat("Adding the new security group to the desktop group");
                        xd7.AddGroupOrUserToDesktopGroup(existingGroup.Uid, group.SamAccountName);

                        existingGroup.SecurityGroup = group.Name;
                    }*/

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
                                //var appGroup = CreateSecurityGroup(app.Name, app.UUID.ToString());
                                //reverse.AddAction(Actions.CreateSecurityGroup, appGroup.Name);

                                //logger.DebugFormat("Adding the new security group to the application");
                                //xd7.AddGroupOrUserToApplication(app.Uid, appGroup.SamAccountName);

                                // Add to database
                                //app.SecurityGroup = appGroup.Name;
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
                                //var appGroup = CreateSecurityGroup(app.Name, app.UUID.ToString());
                                //reverse.AddAction(Actions.CreateSecurityGroup, appGroup.Name);

                                //logger.DebugFormat("Adding the new security group to the application");
                                //xd7.AddGroupOrUserToApplication(app.Uid, appGroup.SamAccountName);

                                // Add to database
                                //app.SecurityGroup = appGroup.Name;
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
                //reverse.RollbackNow();
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
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
using System.Web;
using System.Data.Entity;

namespace CloudPanel.Modules.PlansModules
{
    public class CitrixPlanModule : NancyModule
    {
        private static readonly ILog logger = LogManager.GetLogger("Citrix");

        public CitrixPlanModule() : base("/plans/citrix")
        {
            Get["/", c => c.Request.Accept("text/html")] = _ =>
                {
                    return View["Plans/citrix.cshtml"];
                };

            Get["/", c => !c.Request.Accept("text/html")] = _ =>
                {
                    CloudPanelContext db = null;
                    try
                    {
                        db = new CloudPanelContext(Settings.ConnectionString);
                        db.Database.Connection.Open();

                        logger.DebugFormat("Retrieving the desktop groups");
                        var desktopGroups = (from d in db.CitrixDesktopGroup
                                                         .Include(x => x.Companies)
                                                         .Include(x => x.Users)
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
                };

            Get["/all"] = _ =>
                {
                    CloudPanelContext db = null;
                    XenDesktop7 xd7 = null;

                    try
                    {
                        db = new CloudPanelContext(Settings.ConnectionString);
                        db.Database.Connection.Open();

                        xd7 = new XenDesktop7(Settings.CitrixUri, Settings.Username, Settings.DecryptedPassword);

                        // Get desktop groups
                        var desktopGroups = xd7.GetDesktopGroups();

                        if (desktopGroups == null)
                            logger.WarnFormat("No desktop groups were found.");
                        else
                        {
                            logger.DebugFormat("Found a total of {0} desktop groups", desktopGroups.Count);
                            foreach (var group in desktopGroups)
                            {
                                if (!db.CitrixDesktopGroup.Select(x => x.UUID == group.UUID).Any())
                                {
                                    db.CitrixDesktopGroup.Add(group);
                                }
                            }

                            db.SaveChanges();
                        }

                        return Negotiate.WithModel(new { success = "" })
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

                        if (db != null)
                            db.Dispose();
                    }
                };

            /*this.RequiresAnyClaim(new[] { "SuperAdmin" });
            

            Get["/"] = _ =>
            {
                #region Returns the citrix applications and servers
                CloudPanelContext db = null;
                try
                {
                    db = new CloudPanelContext(Settings.ConnectionString);
                    db.Database.Connection.Open();

                    // Retrieve all citrix plans from the database not assigned to a company
                    var plans = (from d in db.Plans_Citrix
                                     orderby d.Name
                                     orderby d.IsServer
                                     select d).ToList();

                    int draw = 0, start = 0, length = 0, recordsTotal = plans.Count, recordsFiltered = plans.Count, orderColumn = 0;
                    string searchValue = "", orderColumnName = "";
                    bool isAscendingOrder = true;

                    // Check for dataTables and process the values
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
                            plans = (from d in plans
                                         where d.CompanyCode.IndexOf(searchValue, StringComparison.InvariantCultureIgnoreCase) != -1 ||
                                               d.Name.IndexOf(searchValue, StringComparison.InvariantCultureIgnoreCase) != -1 ||
                                               d.GroupName.IndexOf(searchValue, StringComparison.InvariantCultureIgnoreCase) != -1 ||
                                               d.Description.IndexOf(searchValue, StringComparison.InvariantCultureIgnoreCase) != -1
                                         select d).ToList();
                            recordsFiltered = plans.Count;
                        }

                        if (isAscendingOrder)
                            plans = plans.OrderBy(x => x.GetType()
                                                    .GetProperty(orderColumnName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance).GetValue(x, null))
                                                    .Skip(start)
                                                    .Take(length)
                                                    .ToList();
                        else
                            plans = plans.OrderByDescending(x => x.GetType()
                                                    .GetProperty(orderColumnName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance).GetValue(x, null))
                                                    .Skip(start)
                                                    .Take(length)
                                                    .ToList();
                    }

                    return Negotiate.WithModel(new { count = plans.Count })
                                    .WithMediaRangeModel("application/json", new
                                    {
                                        draw = draw,
                                        recordsTotal = recordsTotal,
                                        recordsFiltered = recordsFiltered,
                                        data = plans
                                    })
                                    .WithView("Plans/plans_citrix.cshtml");
                }
                catch (Exception ex)
                {
                    return Negotiate.WithModel(new { error = ex.Message })
                                    .WithMediaRangeModel("application/json", new { error = ex.Message })
                                    .WithView("Plans/plans_citrix.cshtml");
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
                CloudPanelContext db = null;
                try
                {
                    db = new CloudPanelContext(Settings.ConnectionString);
                    db.Database.Connection.Open();

                    var applications = (from d in db.Plans_Citrix
                                        where d.IsServer != true
                                        where string.IsNullOrEmpty(d.CompanyCode)
                                        orderby d.Name
                                        select d).ToList();

                    var servers = (from d in db.Plans_Citrix
                                   where d.IsServer == true
                                   where string.IsNullOrEmpty(d.CompanyCode)
                                   orderby d.Name
                                   select d).ToList();

                    return Negotiate.WithModel(new { applications = applications, servers = servers })
                                    .WithMediaRangeModel("application/json", new { applications = applications, servers = servers })
                                    .WithView("Plans/plans_citrix.cshtml");
                }
                catch (Exception ex)
                {
                    logger.ErrorFormat("Error creating Citrix plan: {0}", ex.ToString());
                    throw;
                }
                finally
                {
                    if (db != null)
                        db.Dispose();
                }
            };

            Delete["/"] = _ =>
            {
                CloudPanelContext db = null;
                try
                {
                    logger.DebugFormat("Preparing to delete Citrix plan");

                    int id = Request.Form.CitrixPlanID.HasValue ? Request.Form.CitrixPlanID : 0;
                    if (id > 0)
                    {

                    }
                    else
                    {

                    }

                    db = new CloudPanelContext(Settings.ConnectionString);
                    db.Database.Connection.Open();

                    var applications = (from d in db.Plans_Citrix
                                        where d.IsServer != true
                                        where string.IsNullOrEmpty(d.CompanyCode)
                                        orderby d.Name
                                        select d).ToList();

                    var servers = (from d in db.Plans_Citrix
                                   where d.IsServer == true
                                   where string.IsNullOrEmpty(d.CompanyCode)
                                   orderby d.Name
                                   select d).ToList();

                    return Negotiate.WithModel(new { applications = applications, servers = servers })
                                    .WithMediaRangeModel("application/json", new { applications = applications, servers = servers })
                                    .WithView("Plans/plans_citrix.cshtml");
                }
                catch (Exception ex)
                {
                    logger.ErrorFormat("Error deleting Citrix plan: {0}", ex.ToString());
                    throw;
                }
                finally
                {
                    if (db != null)
                        db.Dispose();
                }
            };*/
        }
    }
}
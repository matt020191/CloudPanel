using CloudPanel.Base.Config;
using CloudPanel.Database.EntityFramework;
using log4net;
using Nancy;
using Nancy.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;

namespace CloudPanel.Modules.PlansModules
{
    public class CitrixPlanModule : NancyModule
    {
        private static readonly ILog logger = LogManager.GetLogger("Default");

        public CitrixPlanModule() : base("/plans/citrix")
        {
            this.RequiresAnyClaim(new[] { "SuperAdmin" });

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
            };
        }
    }
}
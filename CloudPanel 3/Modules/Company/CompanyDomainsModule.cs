using CloudPanel.Base.Config;
using CloudPanel.Database.EntityFramework;
using Nancy;
using Nancy.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;

namespace CloudPanel.Modules
{
    public class CompanyDomainsModule : NancyModule
    {
        public CompanyDomainsModule() : base("/company/domains/{CompanyCode}")
        {
            this.RequiresAuthentication();

            Get["/"] = _ =>
            {
                #region Returns the domains view with model or json data based on the request
                CloudPanelContext db = null;
                try
                {
                    db = new CloudPanelContext(Settings.ConnectionString);
                    db.Database.Connection.Open();

                    string companyCode = _.CompanyCode;
                    var domains = (from d in db.Domains
                                   where d.CompanyCode == companyCode
                                   orderby d.Domain1
                                   select d).ToList();

                    int draw = 0, start = 0, length = 0, recordsTotal = domains.Count, recordsFiltered = domains.Count, orderColumn = 0;
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
                            domains = (from d in domains
                                       where d.Domain1.IndexOf(searchValue, StringComparison.InvariantCultureIgnoreCase) != -1 
                                       select d).ToList();
                            recordsFiltered = domains.Count;
                        }

                        if (isAscendingOrder)
                            domains = domains.OrderBy(x => x.GetType()
                                                    .GetProperty(orderColumnName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance).GetValue(x, null))
                                                    .Skip(start)
                                                    .Take(length)
                                                    .ToList();
                        else
                            domains = domains.OrderByDescending(x => x.GetType()
                                                    .GetProperty(orderColumnName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance).GetValue(x, null))
                                                    .Skip(start)
                                                    .Take(length)
                                                    .ToList();
                    }

                    return Negotiate.WithModel(domains)
                                    .WithMediaRangeModel("application/json", new
                                    {
                                        draw = draw,
                                        recordsTotal = recordsTotal,
                                        recordsFiltered = recordsFiltered,
                                        data = domains
                                    })
                                    .WithView("Company/company_domains.cshtml");
                }
                catch (Exception ex)
                {
                    return Negotiate.WithMediaRangeModel("application/json", new { error = ex.Message })
                                    .WithView("Company/company_domains.cshtml");
                }
                finally
                {
                    if (db != null)
                        db.Dispose();
                }
                #endregion
            };
        }
    }
}
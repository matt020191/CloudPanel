using CloudPanel.Base.Config;
using CloudPanel.Database.EntityFramework;
using Nancy;
using Nancy.Security;
using System;
using System.Linq;
using System.Reflection;

namespace CloudPanel.Modules
{
    public class GroupsModule : NancyModule
    {
        public GroupsModule() : base("/company/exchange/groups/{CompanyCode}")
        {
            this.RequiresAuthentication();

            Get["/"] = _ =>
            {
                #region Returns the groups view with model or json data based on the request
                CloudPanelContext db = null;
                try
                {
                    db = new CloudPanelContext(Settings.ConnectionString);
                    db.Database.Connection.Open();

                    string companyCode = _.CompanyCode;
                    var groups = (from d in db.DistributionGroups
                                  where d.CompanyCode == companyCode
                                  orderby d.DisplayName
                                  select d).ToList();

                    int draw = 0, start = 0, length = 0, recordsTotal = groups.Count, recordsFiltered = groups.Count, orderColumn = 0;
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
                            groups = (from d in groups
                                       where d.DisplayName.IndexOf(searchValue, StringComparison.InvariantCultureIgnoreCase) != -1 || 
                                             d.Email.IndexOf(searchValue, StringComparison.InvariantCultureIgnoreCase) != -1
                                       select d).ToList();
                            recordsFiltered = groups.Count;
                        }

                        if (isAscendingOrder)
                            groups = groups.OrderBy(x => x.GetType()
                                                    .GetProperty(orderColumnName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance).GetValue(x, null))
                                                    .Skip(start)
                                                    .Take(length)
                                                    .ToList();
                        else
                            groups = groups.OrderByDescending(x => x.GetType()
                                                    .GetProperty(orderColumnName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance).GetValue(x, null))
                                                    .Skip(start)
                                                    .Take(length)
                                                    .ToList();
                    }

                    return Negotiate.WithModel(groups)
                                    .WithMediaRangeModel("application/json", new
                                    {
                                        draw = draw,
                                        recordsTotal = recordsTotal,
                                        recordsFiltered = recordsFiltered,
                                        data = groups
                                    })
                                    .WithView("Company/Exchange/company_groups.cshtml");
                }
                catch (Exception ex)
                {
                    return Negotiate.WithMediaRangeModel("application/json", new { error = ex.Message })
                                    .WithView("Company/Exchange/company_groups.cshtml");
                }
                finally
                {
                    if (db != null)
                        db.Dispose();
                }
                #endregion
            };

            Post["/{CompanyCode}"] = _ =>
            {
                return HttpStatusCode.InternalServerError;
            };

            Put["/{CompanyCode}"] = _ =>
            {
                return HttpStatusCode.InternalServerError;
            };

            Delete["/{CompanyCode}"] = _ =>
            {
                return HttpStatusCode.InternalServerError;
            };
        }
    }
}
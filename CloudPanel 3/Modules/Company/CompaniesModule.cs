using CloudPanel.Base.Config;
using CloudPanel.Database.EntityFramework;
using Nancy;
using System;
using System.Linq;
using System.Reflection;

namespace CloudPanel.Modules.Companies
{
    public class CompaniesModule : NancyModule
    {
        public CompaniesModule() : base("/companies/{ResellerCode}")
        {
            Get["/"] = _ =>
            {
                string resellerCode = _.ResellerCode;

                #region Returns the companies view with model or json data based on the request
                CloudPanelContext db = null;
                try
                {
                    db = new CloudPanelContext(Settings.ConnectionString);
                    db.Database.Connection.Open();

                    // Retrieve all resellers from the database
                    var companies = (from d in db.Companies
                                     where !d.IsReseller
                                     where d.ResellerCode == resellerCode
                                     orderby d.CompanyName
                                     select d).ToList();

                    int draw = 0, start = 0, length = 0, recordsTotal = companies.Count, recordsFiltered = companies.Count, orderColumn = 0;
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
                            companies = (from d in companies
                                         where d.CompanyCode.IndexOf(searchValue, StringComparison.InvariantCultureIgnoreCase) != -1 ||
                                               d.CompanyName.IndexOf(searchValue, StringComparison.InvariantCultureIgnoreCase) != -1 ||
                                               d.City.IndexOf(searchValue, StringComparison.InvariantCultureIgnoreCase) != -1 ||
                                               d.State.IndexOf(searchValue, StringComparison.InvariantCultureIgnoreCase) != -1 ||
                                               d.ZipCode.IndexOf(searchValue, StringComparison.InvariantCultureIgnoreCase) != -1 ||
                                               d.Country.IndexOf(searchValue, StringComparison.InvariantCultureIgnoreCase) != -1
                                         select d).ToList();
                            recordsFiltered = companies.Count;
                        }

                        if (isAscendingOrder)
                            companies = companies.OrderBy(x => x.GetType()
                                                    .GetProperty(orderColumnName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance).GetValue(x, null))
                                                    .Skip(start)
                                                    .Take(length)
                                                    .ToList();
                        else
                            companies = companies.OrderByDescending(x => x.GetType()
                                                    .GetProperty(orderColumnName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance).GetValue(x, null))
                                                    .Skip(start)
                                                    .Take(length)
                                                    .ToList();
                    }

                    return Negotiate.WithModel(resellerCode)
                                    .WithMediaRangeModel("application/json", new
                                    {
                                        draw = draw,
                                        recordsTotal = recordsTotal,
                                        recordsFiltered = recordsFiltered,
                                        data = companies
                                    })
                                    .WithView("companies.cshtml");
                }
                catch (Exception ex)
                {
                    return Negotiate.WithModel(resellerCode)
                                    .WithMediaRangeModel("application/json", new { error = ex.Message })
                                    .WithView("comapnies.cshtml");
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
using CloudPanel.Base.Config;
using CloudPanel.Database.EntityFramework;
using Nancy;
using System;
using System.Linq;
using System.Reflection;

namespace CloudPanel.Modules
{
    public class ResellersModule : NancyModule
    {
        public ResellersModule() : base("/resellers")
        {
            Get["/"] = _ =>
                {
                    #region Returns the resellers view with model or json data based on the request
                    CloudPanelContext db = null;
                    try
                    {
                        db = new CloudPanelContext(Settings.ConnectionString);
                        db.Database.Connection.Open();

                        // Retrieve all resellers from the database
                        var resellers = (from d in db.Companies
                                         where d.IsReseller
                                         orderby d.CompanyName
                                         select new
                                         {
                                             CompanyCode = d.CompanyCode,
                                             CompanyName = d.CompanyName,
                                             City = d.City,
                                             State = d.State,
                                             ZipCode = d.ZipCode,
                                             Country = d.Country,
                                             Created = d.Created,
                                             TotalCompanies = (from c in db.Companies
                                                               where c.ResellerCode == d.CompanyCode
                                                               select c).Count()
                                         }).ToList();

                        int draw = 0, start = 0, length = 0, recordsTotal = resellers.Count, recordsFiltered = resellers.Count, orderColumn = 0;
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
                                resellers = (from d in resellers
                                             where d.CompanyCode.IndexOf(searchValue, StringComparison.InvariantCultureIgnoreCase) != -1 ||
                                                   d.CompanyName.IndexOf(searchValue, StringComparison.InvariantCultureIgnoreCase) != -1 ||
                                                   d.City.IndexOf(searchValue, StringComparison.InvariantCultureIgnoreCase) != -1 ||
                                                   d.State.IndexOf(searchValue, StringComparison.InvariantCultureIgnoreCase) != -1 ||
                                                   d.ZipCode.IndexOf(searchValue, StringComparison.InvariantCultureIgnoreCase) != -1 ||
                                                   d.Country.IndexOf(searchValue, StringComparison.InvariantCultureIgnoreCase) != -1
                                             select d).ToList();
                                recordsFiltered = resellers.Count;
                            }

                            if (isAscendingOrder)
                                resellers = resellers.OrderBy(x => x.GetType()
                                                        .GetProperty(orderColumnName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance).GetValue(x, null))
                                                        .Skip(start)
                                                        .Take(length)
                                                        .ToList();
                            else
                                resellers = resellers.OrderByDescending(x => x.GetType()
                                                        .GetProperty(orderColumnName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance).GetValue(x, null))
                                                        .Skip(start)
                                                        .Take(length)
                                                        .ToList();
                        }

                        return Negotiate.WithModel(resellers)
                                        .WithMediaRangeModel("application/json", new
                                        {
                                            draw = draw,
                                            recordsTotal = recordsTotal,
                                            recordsFiltered = recordsFiltered,
                                            data = resellers
                                        })
                                        .WithView("resellers.cshtml");
                    }
                    catch (Exception ex)
                    {
                        return Negotiate.WithMediaRangeModel("application/json", new { error = ex.Message })
                                        .WithView("resellers.cshtml");
                    }
                    finally
                    {
                        if (db != null)
                            db.Dispose();
                    }
                    #endregion
                };

            Post["/{ResellerCode}"] = _ =>
                {
                    return HttpStatusCode.InternalServerError;
                };

            Put["/{ResellerCode}"] = _ =>
                {
                    return HttpStatusCode.InternalServerError;
                };

            Delete["/{ResellerCode}"] = _ =>
                {
                    return HttpStatusCode.InternalServerError;
                };
        }
    }
}
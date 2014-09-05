using CloudPanel.Base.Config;
using CloudPanel.Database.EntityFramework;
using Nancy;
using Nancy.Security;
using System;
using System.Linq;
using System.Reflection;

namespace CloudPanel.Modules
{
    public class ContactsModule : NancyModule
    {
        public ContactsModule() : base("/company/{CompanyCode}/exchange/contacts")
        {
            this.RequiresAuthentication();

            Get["/"] = _ =>
            {
                #region Returns the contacts view with model or json data based on the request
                CloudPanelContext db = null;
                try
                {
                    db = new CloudPanelContext(Settings.ConnectionString);
                    db.Database.Connection.Open();

                    string companyCode = _.CompanyCode;
                    var contacts = (from d in db.Contacts
                                    where d.CompanyCode == companyCode
                                    orderby d.DisplayName
                                    select d).ToList();

                    int draw = 0, start = 0, length = 0, recordsTotal = contacts.Count, recordsFiltered = contacts.Count, orderColumn = 0;
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
                            contacts = (from d in contacts
                                       where d.DisplayName.IndexOf(searchValue, StringComparison.InvariantCultureIgnoreCase) != -1 || 
                                             d.Email.IndexOf(searchValue, StringComparison.InvariantCultureIgnoreCase) != -1
                                       select d).ToList();
                            recordsFiltered = contacts.Count;
                        }

                        if (isAscendingOrder)
                            contacts = contacts.OrderBy(x => x.GetType()
                                                    .GetProperty(orderColumnName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance).GetValue(x, null))
                                                    .Skip(start)
                                                    .Take(length)
                                                    .ToList();
                        else
                            contacts = contacts.OrderByDescending(x => x.GetType()
                                                    .GetProperty(orderColumnName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance).GetValue(x, null))
                                                    .Skip(start)
                                                    .Take(length)
                                                    .ToList();
                    }

                    return Negotiate.WithModel(contacts)
                                    .WithMediaRangeModel("application/json", new
                                    {
                                        draw = draw,
                                        recordsTotal = recordsTotal,
                                        recordsFiltered = recordsFiltered,
                                        data = contacts
                                    })
                                    .WithView("Company/Exchange/company_contacts.cshtml");
                }
                catch (Exception ex)
                {
                    return Negotiate.WithMediaRangeModel("application/json", new { error = ex.Message })
                                    .WithView("Company/Exchange/company_contacts.cshtml");
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
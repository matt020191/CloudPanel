using CloudPanel.Base.Config;
using CloudPanel.Database.EntityFramework;
using log4net;
using Nancy;
using Nancy.Security;
using System;
using System.Linq;
using System.Reflection;

namespace CloudPanel.Modules
{
    public class CompanyUsersModule : NancyModule
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(CompanyUsersModule));

        public CompanyUsersModule() : base("/company/{CompanyCode}/users")
        {
            this.RequiresAuthentication();

            Get["/"] = _ =>
            {
                #region Returns the users view with model or json data based on the request
                CloudPanelContext db = null;
                try
                {
                    db = new CloudPanelContext(Settings.ConnectionString);
                    db.Database.Connection.Open();

                    string companyCode = _.CompanyCode;
                    var users = (from d in db.Users
                                 where d.CompanyCode == companyCode
                                 select new
                                 {
                                     UserGuid = d.UserGuid,
                                     CompanyCode = d.CompanyCode,
                                     DisplayName = d.DisplayName,
                                     UserPrincipalName = d.UserPrincipalName,
                                     SamAccountName = d.sAMAccountName,
                                     DistinguishedName = d.DistinguishedName,
                                     Department = d.Department,
                                     IsCompanyAdmin = d.IsCompanyAdmin == null ? false : (bool)d.IsCompanyAdmin,
                                     IsResellerAdmin = d.IsResellerAdmin == null ? false : (bool)d.IsResellerAdmin,
                                     IsEnabled = d.IsEnabled == null ? true : (bool)d.IsEnabled,
                                     MailboxPlan = d.MailboxPlan == null ? 0 : (int)d.MailboxPlan,
                                     Created = d.Created,
                                     Email = d.Email
                                 }).ToList();

                    int draw = 0, start = 0, length = 0, recordsTotal = users.Count, recordsFiltered = users.Count, orderColumn = 0;
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
                            users = (from d in users
                                     where d.DisplayName.IndexOf(searchValue, StringComparison.InvariantCultureIgnoreCase) != -1 ||
                                            d.UserPrincipalName.IndexOf(searchValue, StringComparison.InvariantCultureIgnoreCase) != -1 ||
                                            d.SamAccountName.IndexOf(searchValue, StringComparison.InvariantCultureIgnoreCase) != -1 ||
                                            d.Department.IndexOf(searchValue, StringComparison.InvariantCultureIgnoreCase) != -1 ||
                                            d.Email.IndexOf(searchValue, StringComparison.InvariantCultureIgnoreCase) != -1
                                     select d).ToList();
                            recordsFiltered = users.Count;
                        }

                        if (isAscendingOrder)
                            users = users.OrderBy(x => x.GetType()
                                                    .GetProperty(orderColumnName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance).GetValue(x, null))
                                                    .Skip(start)
                                                    .Take(length)
                                                    .ToList();
                        else
                            users = users.OrderByDescending(x => x.GetType()
                                                    .GetProperty(orderColumnName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance).GetValue(x, null))
                                                    .Skip(start)
                                                    .Take(length)
                                                    .ToList();
                    }

                    return Negotiate.WithModel(users)
                                    .WithMediaRangeModel("application/json", new
                                    {
                                        draw = draw,
                                        recordsTotal = recordsTotal,
                                        recordsFiltered = recordsFiltered,
                                        data = users
                                    })
                                    .WithView("Company/company_users.cshtml");
                }
                catch (Exception ex)
                {
                    return Negotiate.WithMediaRangeModel("application/json", new { error = ex.Message })
                                    .WithView("Company/company_users.cshtml");
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
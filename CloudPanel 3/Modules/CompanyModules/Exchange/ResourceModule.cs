using Nancy;
using Nancy.Security;
using Nancy.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CloudPanel.Database.EntityFramework;
using CloudPanel.Base.Config;
using System.Reflection;

namespace CloudPanel.Modules
{
    public class ResourceModule : NancyModule
    {
        public ResourceModule() : base("/company/{CompanyCode}/exchange/resourcemailboxes")
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
                    var resourceMailboxes = (from d in db.ResourceMailboxes
                                             join p in db.Plans_ExchangeMailbox on d.MailboxPlan equals p.MailboxPlanID into p1
                                             from data1 in p1.DefaultIfEmpty()
                                             where d.CompanyCode == companyCode
                                             orderby d.DisplayName
                                             orderby d.ResourceType
                                             select new
                                             {
                                                 ResourceID = d.ResourceID,
                                                 DisplayName = d.DisplayName,
                                                 CompanyCode = d.CompanyCode,
                                                 UserPrincipalName = d.UserPrincipalName,
                                                 PrimarySmtpAddress =d.PrimarySmtpAddress,
                                                 ResourceType = d.ResourceType,
                                                 MailboxPlanID = d.MailboxPlan,
                                                 MailboxPlanName = data1.MailboxPlanName,
                                                 AdditionalMB = d.AdditionalMB
                                             }).ToList();

                    int draw = 0, start = 0, length = 0, recordsTotal = resourceMailboxes.Count, recordsFiltered = resourceMailboxes.Count, orderColumn = 0;
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
                            resourceMailboxes = (from d in resourceMailboxes
                                                 where d.DisplayName.IndexOf(searchValue, StringComparison.InvariantCultureIgnoreCase) != -1 || 
                                                       d.UserPrincipalName.IndexOf(searchValue, StringComparison.InvariantCultureIgnoreCase) != -1 || 
                                                       d.PrimarySmtpAddress.IndexOf(searchValue, StringComparison.InvariantCultureIgnoreCase) != -1 || 
                                                       d.ResourceType.IndexOf(searchValue, StringComparison.InvariantCultureIgnoreCase) != -1 || 
                                                       d.MailboxPlanName.IndexOf(searchValue, StringComparison.InvariantCultureIgnoreCase) != -1
                                                select d).ToList();
                            recordsFiltered = resourceMailboxes.Count;
                        }

                        if (isAscendingOrder)
                            resourceMailboxes = resourceMailboxes.OrderBy(x => x.GetType()
                                                    .GetProperty(orderColumnName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance).GetValue(x, null))
                                                    .Skip(start)
                                                    .Take(length)
                                                    .ToList();
                        else
                            resourceMailboxes = resourceMailboxes.OrderByDescending(x => x.GetType()
                                                    .GetProperty(orderColumnName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance).GetValue(x, null))
                                                    .Skip(start)
                                                    .Take(length)
                                                    .ToList();
                    }

                    return Negotiate.WithModel(resourceMailboxes)
                                    .WithMediaRangeModel("application/json", new
                                    {
                                        draw = draw,
                                        recordsTotal = recordsTotal,
                                        recordsFiltered = recordsFiltered,
                                        data = resourceMailboxes
                                    })
                                    .WithView("Company/Exchange/company_resourcemailboxes.cshtml");
                }
                catch (Exception ex)
                {
                    return Negotiate.WithMediaRangeModel("application/json", new { error = ex.Message })
                                    .WithView("Company/Exchange/company_resourcemailboxes.cshtml");
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
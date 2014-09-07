using CloudPanel.Base.Config;
using CloudPanel.Base.Charting;
using CloudPanel.Database.EntityFramework;
using Nancy;
using Nancy.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using log4net;

namespace CloudPanel.Modules
{
    public class SuperChartsModule : NancyModule
    {
        private static readonly ILog logger = log4net.LogManager.GetLogger(typeof(SuperChartsModule));

        public SuperChartsModule() : base("/charts/super")
        {
            this.RequiresAuthentication();
            this.RequiresAnyClaim(new[] { "SuperAdmin" });

            Get["/customers/top/{X:int}"] = _ =>
                {
                    int top = _.X;

                    CloudPanelContext db = null;
                    try
                    {
                        db = new CloudPanelContext(Settings.ConnectionString);

                        logger.DebugFormat("Getting top {0} customers for super admin", _.X);
                        var topCustomers = (from d in db.Users
                                            group d by d.CompanyCode into grp
                                             select new
                                             {
                                                 CompanyCode = grp.Key,
                                                 Count = grp.Select(x => x.UserGuid).Distinct().Count()
                                             }).OrderByDescending(x => x.Count).Take(top).ToList();

                        logger.DebugFormat("Found a total of {0} companies", topCustomers.Count());
                        List<TopXCustomers> topX = new List<TopXCustomers>();
                        foreach (var data in topCustomers)
                        {
                            topX.Add(new TopXCustomers()
                                {
                                    CompanyCode = data.CompanyCode,
                                    TotalUsers = data.Count,
                                    CompanyName = (from d in db.Companies where d.CompanyCode == data.CompanyCode select d.CompanyName).FirstOrDefault(),
                                    TotalMailboxes = (from d in db.Users where d.CompanyCode == data.CompanyCode where d.MailboxPlan > 0 select d.ID).Count()
                                });
                        }

                        return Response.AsJson(new 
                        {
                            data = topX
                        });
                    }
                    catch (Exception ex)
                    {
                        logger.ErrorFormat("Error getting top X customers: {0}", ex.ToString());
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
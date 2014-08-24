using CloudPanel.Base.Config;
using CloudPanel.Base.Charting;
using CloudPanel.Database.EntityFramework;
using Nancy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CloudPanel.Modules
{
    public class SuperChartsModule : NancyModule
    {
        public SuperChartsModule() : base("/charts/super")
        {
            Get["/customers/top/{X:int}"] = _ =>
                {
                    int top = _.X;

                    CloudPanelContext db = null;
                    try
                    {
                        db = new CloudPanelContext(Settings.ConnectionString);

                        var topCustomers = (from d in db.Users
                                             group d by d.CompanyCode into grp
                                             select new
                                             {
                                                 CompanyCode = grp.Key,
                                                 Count = grp.Select(x => x.UserGuid).Distinct().Count()
                                             }).OrderByDescending(x => x.Count).Take(top).ToList();

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
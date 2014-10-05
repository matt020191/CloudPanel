using CloudPanel.Base.Config;
using CloudPanel.Database.EntityFramework;
using log4net;
using Nancy;
using Nancy.Authentication;
using Nancy.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CloudPanel.Modules.Admin
{
    public class MassEmailModule : NancyModule
    {
        private static readonly ILog logger = log4net.LogManager.GetLogger(typeof(MassEmailModule));

        public MassEmailModule() : base("/admin/email")
        {
            this.RequiresAuthentication();
            this.RequiresAnyClaim(new[] { "SuperAdmin", "ResellerAdmin" });

            Get["/"] = _ =>
            {
                CloudPanelContext db = null;
                try
                {
                    logger.DebugFormat("Loading mass email section");
                    db = new CloudPanelContext(Settings.ConnectionString);

                    if (NancyContextHelpers.IsSuperAdmin(this.Context))
                    {
                        var companies = (from d in db.Companies
                                         orderby d.CompanyName
                                         select d).ToList();
                        return View["Admin/massemail.cshtml", new { companies = companies }];
                    }
                    else
                    {
                        // Has to be a reseller admin in this case (only return companies for the reseller)
                        string resellerCode = NancyContextHelpers.GetSelectedResellerCode(this.Context);
                        var companies = (from d in db.Companies
                                         where d.ResellerCode == resellerCode
                                         orderby d.CompanyName
                                         select d).ToList();

                        return View["Admin/massemail.cshtml", new { companies = companies }];
                    }
                }
                catch (Exception ex)
                {
                    logger.ErrorFormat("Error loading mass email page: {0}", ex.ToString());

                    ViewBag.error = ex.Message;
                    return View["error.cshtml"];
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
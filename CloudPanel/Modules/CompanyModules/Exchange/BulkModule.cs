using Nancy;
using Nancy.Security;
using Nancy.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using log4net;

namespace CloudPanel.Modules.CompanyModules.Exchange
{
    public class BulkModule : NancyModule
    {
        private static readonly ILog logger = log4net.LogManager.GetLogger(typeof(BulkModule));

        public BulkModule() : base("/company/{CompanyCode}/exchange/bulk")
        {
            //this.RequiresAuthentication();

            Get["/", c => c.Request.Accept("text/html")] = _ =>
            {
                return View["Company/Exchange/Bulk.cshtml"];
            };
        }
    }
}
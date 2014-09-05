using Nancy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CloudPanel.Modules.CompanyModules.Exchange
{
    public class EnableModule : NancyModule
    {
        public EnableModule() : base("/company/{CompanyCode}/exchange/enable")
        {
            Get["/"] = _ =>
            {
                return View["Company/Exchange/enable.cshtml"];
            };
        }
    }
}
using log4net;
using Nancy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CloudPanel.Modules.CompanyModules.Exchange
{
    public class DisableModule : NancyModule
    {
        private static readonly ILog logger = log4net.LogManager.GetLogger(typeof(DisableModule));

        public DisableModule() : base("/company/{CompanyCode}/exchange/disable")
        {
            Get["/"] = _ =>
            {
                return View["Company/Exchange/disable.cshtml"];
            };
        }
    }
}
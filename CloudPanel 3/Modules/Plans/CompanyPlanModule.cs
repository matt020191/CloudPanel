using Nancy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CloudPanel.Modules
{
    public class CompanyPlanModule : NancyModule
    {
        public CompanyPlanModule() : base("/plans/company")
        {
            Get["/"] = _ =>
                {
                    return View["Plans/plans_company.cshtml"];
                };
        }
    }
}
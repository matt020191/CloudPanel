using CloudPanel.ActiveDirectory;
using CloudPanel.Base.AD;
using CloudPanel.Base.Config;
using CloudPanel.Base.Database.Models;
using CloudPanel.Citrix;
using CloudPanel.Database.EntityFramework;
using CloudPanel.Rollback;
using log4net;
using Nancy;
using Nancy.Security;
using System;
using System.Data.Entity;
using System.Linq;
using System.Reflection;

namespace CloudPanel.Modules.PlansModules
{
    public class CitrixPlanModule : NancyModule
    {
        private static readonly ILog logger = LogManager.GetLogger("Citrix");

        public CitrixPlanModule() : base("/plans/citrix")
        {
            this.RequiresClaims(new[] { "SuperAdmin"});

            
        }

        

    }
}
using CloudPanel.Base.Database.Models;
using CloudPanel.Exchange;
using Nancy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CloudPanel.Modules.PlansModules
{
    public class ActiveSyncPlanModule : NancyModule
    {
        public ActiveSyncPlanModule() : base("/plans/exchange/activesync")
        {
            Get["/new"] = _ =>
                {
                    return View["Plans/activesync.cshtml"];
                };

            Get["/all"] = _ =>
                {
                    dynamic powershell = null;

                    try
                    {
                        powershell = ExchPowershell.GetClass();
                        List<Plans_ExchangeActiveSync> policies = powershell.Get_ActiveSyncPolicies();

                        return Negotiate.WithModel(new { policies = policies })
                                        .WithStatusCode(HttpStatusCode.OK);
                    }
                    catch (Exception ex)
                    {
                        return Negotiate.WithModel(new { error = ex.Message })
                                        .WithStatusCode(HttpStatusCode.InternalServerError);
                    }
                    finally
                    {
                        if (powershell != null)
                            powershell.Dispose();
                    }
                };
        }
    }
}
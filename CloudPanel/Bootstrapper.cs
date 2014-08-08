using CloudPanel.Base.Config;
using CloudPanel.Database.EntityFramework;
using log4net;
using Nancy;
using Nancy.Authentication.Forms;
using Nancy.Bootstrapper;
using Nancy.Session;
using Nancy.TinyIoc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CloudPanel.code;

namespace CloudPanel
{
    public class Bootstrapper : DefaultNancyBootstrapper
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(Bootstrapper));
        private static readonly Auditor auditor = new Auditor();

        protected override void ApplicationStartup(Nancy.TinyIoc.TinyIoCContainer container, Nancy.Bootstrapper.IPipelines pipelines)
        {
            // Cache the settings from the config file in memory
            SettingsRequest.RetrieveSettings();

            // Cache the brandings from the config file in memory
            BrandingsRequest.RetrieveBrandings();

            // Enable cookie based sessions
            CookieBasedSessions.Enable(pipelines);

            auditor.AuditOn(pipelines);

            base.ApplicationStartup(container, pipelines);
        }

        protected override void ConfigureRequestContainer(TinyIoCContainer container, NancyContext context)
        {
            base.ConfigureRequestContainer(container, context);

            // Here we register our user mapper as a per-request singleton.
            // As this is now per-request we could inject a request scoped
            // database "context" or other request scoped services.
            container.Register<IUserMapper, UserMapper>();
            container.Register<CloudPanelContext>((x, y) => string.IsNullOrEmpty(Settings.ConnectionString) ? null : new CloudPanelContext(Settings.ConnectionString));
        }

        protected override void RequestStartup(TinyIoCContainer requestContainer, IPipelines pipelines, NancyContext context)
        {
            // At request startup we modify the request pipelines to
            // include forms authentication - passing in our now request
            // scoped user name mapper.
            //
            // The pipelines passed in here are specific to this request,
            // so we can add/remove/update items in them as we please.
            var formsAuthConfiguration =
                new FormsAuthenticationConfiguration()
                {
                    RedirectUrl = "~/login",
                    UserMapper = requestContainer.Resolve<IUserMapper>(),
                };

            FormsAuthentication.Enable(pipelines, formsAuthConfiguration);
        }
    }

    public static class AuditorMixins
    {
        public static void AuditOn(this Auditor This, IPipelines pipelines)
        {
            var methods = new[] { "POST", "DELETE", "PUT", "PATCH", };
            pipelines.AfterRequest += x =>
            {
                int statusCode = (int)x.Response.StatusCode;
                var info = x.Items.FirstOrDefault(y => y.Key == AUDIT_INFO_KEY).Value;

                if ((statusCode >= 200 && statusCode < 300 && methods.Contains(x.ResolvedRoute.Description.Method)) ||
                    (info != null))
                {
                    var currentUser = x.CurrentUser as AuthenticatedUser;

                    var ip = x.Request.UserHostAddress;
                    var user = x.CurrentUser.UserName;
                    var companyCode = currentUser == null ? "" : currentUser.CompanyCode ?? "";
                    var method = x.ResolvedRoute.Description.Method;
                    var path = x.Request.Path;

                    This.Audit(user, companyCode, method, path, ip, info as string);
                }
            };
        }

        public static void AuditIdentifyingInfo(this NancyModule This, string info)
        {
            This.Context.Items[AUDIT_INFO_KEY] = info;
        }

        private const string AUDIT_INFO_KEY = "AuditIdentifyingInfo";
    }
}

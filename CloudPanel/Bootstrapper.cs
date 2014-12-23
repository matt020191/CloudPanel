namespace CloudPanel
{
    using CloudPanel.Base.Config;
    using CloudPanel.Base.Database.Models;
    using CloudPanel.Code;
    using CloudPanel.Database.EntityFramework;
    using CloudPanel.Database.EntityFramework.Migrations;
    using CloudPanel.Modules;
    using log4net;
    using log4net.Config;
    using Nancy;
    using Nancy.Authentication.Forms;
    using Nancy.Authentication.Token;
    using Nancy.Bootstrapper;
    using Nancy.Session;
    using Nancy.TinyIoc;
    using System;
    using System.Text;
    using System.Threading.Tasks;
    using Entity = System.Data.Entity;

    public class Bootstrapper : DefaultNancyBootstrapper
    {
        private static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        protected override void ApplicationStartup(Nancy.TinyIoc.TinyIoCContainer container, Nancy.Bootstrapper.IPipelines pipelines)
        {
            //
            StaticConfiguration.DisableErrorTraces = false;

            // Enable the logger
            XmlConfigurator.Configure();

            // Load the settings
            StaticSettings.LoadSettings();

            // Upgrade database on start. Must be after load settings so we can get the connection string
            Entity.Database.SetInitializer(new Entity.MigrateDatabaseToLatestVersion<CloudPanelContext, Configuration>());

            // Enable cookie based sessions
            CookieBasedSessions.Enable(pipelines);

            // Load brandings
            BrandingModule.LoadAllBrandings();
            
            // Initialize auditing
            pipelines.AfterRequest += async (ctx) =>
                {
                    await Task.Run(() =>
                    {
                        if (!ctx.Request.Method.Equals("GET"))
                        {
                            using (var db = new CloudPanelContext(Settings.ConnectionString))
                            {
                                var sb = new StringBuilder();
                                if (ctx.Parameters.Count > 0)
                                    foreach (var k in ctx.Parameters.Keys)
                                        sb.AppendFormat("[{0}: {1}] ", k, ctx.Parameters[k].Value);

                                if (ctx.Request.Form.Count > 0)
                                    foreach (var k in ctx.Request.Form.Keys)
                                        if (k != "password") // Do not capture passwords in audit trace
                                            sb.AppendFormat("[{0}: {1}] ", k, ctx.Request.Form[k].Value);

                                db.AuditTrace.Add(new AuditTrace()
                                {
                                    TimeStamp = DateTime.Now,
                                    IPAddress = ctx.Request.UserHostAddress,
                                    Method = ctx.Request.Method,
                                    Route = ctx.Request.Path,
                                    Username = (ctx.CurrentUser == null ? string.Empty : ctx.CurrentUser.UserName),
                                    Parameters = sb.ToString()
                                });

                                db.SaveChanges();
                            }
                        }
                    });
                };

            //
            base.ApplicationStartup(container, pipelines);
        }

        protected override void ConfigureRequestContainer(TinyIoCContainer container, NancyContext context)
        {
            container.Register<IUserMapper, UserMapper>();
            container.Register<ITokenizer>(new Tokenizer());
            container.Register<CloudPanelContext>((x, y) => string.IsNullOrEmpty(Settings.ConnectionString) ? null : new CloudPanelContext(Settings.ConnectionString));

            base.ConfigureRequestContainer(container, context);
        }

        protected override void RequestStartup(TinyIoCContainer requestContainer, IPipelines pipelines, NancyContext context)
        {
            var formsAuthConfiguration = new FormsAuthenticationConfiguration() { RedirectUrl = "~/login", UserMapper = requestContainer.Resolve<IUserMapper>() };

            FormsAuthentication.Enable(pipelines, formsAuthConfiguration);
            TokenAuthentication.Enable(pipelines, new TokenAuthenticationConfiguration(requestContainer.Resolve<ITokenizer>()));

            base.RequestStartup(requestContainer, pipelines, context);
        }
    }
}
namespace CloudPanel
{
    using CloudPanel.Base.Config;
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
    using Nancy.Responses;
    using Nancy.Session;
    using Nancy.TinyIoc;
    using Entity = System.Data.Entity;

    public class Bootstrapper : DefaultNancyBootstrapper
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(Bootstrapper));

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
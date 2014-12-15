using CloudPanel.Base.Config;
using CloudPanel.Base.Database.Models;
using log4net;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Data.Entity.Validation;
using System.Linq;

// NB: http://stackoverflow.com/questions/16210771/entity-framework-code-first-without-app-config
namespace CloudPanel.Database.EntityFramework
{
    public partial class CloudPanelContext : DbContext
    {
        private static readonly ILog logger = log4net.LogManager.GetLogger(typeof(CloudPanelContext));
        
        public CloudPanelContext() : base(Settings.ConnectionString)
        {
            //logger.Debug("Context called without a connection string");
        }

        public CloudPanelContext(string connectionString) : base(connectionString)
        {
            //logger.DebugFormat("Context called with connection string {0}", connectionString);
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            //modelBuilder.Conventions.Remove<PluralizingTableNameConvention>(); 

            /*
            modelBuilder.Entity<Companies>()
                .Property(e => e.Country)
                .IsUnicode(false);
            
            modelBuilder.Entity<Contacts>()
                .Property(e => e.DistinguishedName)
                .IsUnicode(false);

            modelBuilder.Entity<Contacts>()
                .Property(e => e.CompanyCode)
                .IsUnicode(false);

            modelBuilder.Entity<Contacts>()
                .Property(e => e.DisplayName)
                .IsUnicode(false);

            modelBuilder.Entity<Contacts>()
                .Property(e => e.Email)
                .IsUnicode(false);

            modelBuilder.Entity<DistributionGroups>()
                .Property(e => e.DistinguishedName)
                .IsUnicode(false);

            modelBuilder.Entity<DistributionGroups>()
                .Property(e => e.CompanyCode)
                .IsUnicode(false);

            modelBuilder.Entity<DistributionGroups>()
                .Property(e => e.DisplayName)
                .IsUnicode(false);

            modelBuilder.Entity<DistributionGroups>()
                .Property(e => e.Email)
                .IsUnicode(false);

            modelBuilder.Entity<Domains>()
                .Property(e => e.CompanyCode)
                .IsUnicode(false);

            modelBuilder.Entity<Plans_Citrix>()
                .Property(e => e.Name)
                .IsUnicode(false);

            modelBuilder.Entity<Plans_Citrix>()
                .Property(e => e.GroupName)
                .IsUnicode(false);

            modelBuilder.Entity<Plans_Citrix>()
                .Property(e => e.CompanyCode)
                .IsUnicode(false);

            modelBuilder.Entity<Plans_Citrix>()
                .Property(e => e.PictureURL)
                .IsUnicode(false);

            modelBuilder.Entity<Plans_ExchangeActiveSync>()
                .Property(e => e.CompanyCode)
                .IsUnicode(false);

            modelBuilder.Entity<Plans_ExchangeActiveSync>()
                .Property(e => e.DisplayName)
                .IsUnicode(false);

            modelBuilder.Entity<Plans_ExchangeActiveSync>()
                .Property(e => e.ExchangeName)
                .IsUnicode(false);

            modelBuilder.Entity<Plans_ExchangeActiveSync>()
                .Property(e => e.IncludePastCalendarItems)
                .IsUnicode(false);

            modelBuilder.Entity<Plans_ExchangeActiveSync>()
                .Property(e => e.IncludePastEmailItems)
                .IsUnicode(false);

            modelBuilder.Entity<Plans_ExchangeActiveSync>()
                .Property(e => e.AllowBluetooth)
                .IsUnicode(false);

            modelBuilder.Entity<Plans_ExchangeMailbox>()
                .Property(e => e.ResellerCode)
                .IsUnicode(false);

            modelBuilder.Entity<Plans_ExchangeMailbox>()
                .Property(e => e.CompanyCode)
                .IsUnicode(false);

            modelBuilder.Entity<Plans_Organization>()
                .Property(e => e.ResellerCode)
                .IsUnicode(false);

            modelBuilder.Entity<Prices>()
                .Property(e => e.CompanyCode)
                .IsUnicode(false);

            modelBuilder.Entity<Prices>()
                .Property(e => e.Price)
                .HasPrecision(19, 4);

            modelBuilder.Entity<ResourceMailboxes>()
                .Property(e => e.CompanyCode)
                .IsUnicode(false);

            modelBuilder.Entity<ResourceMailboxes>()
                .Property(e => e.ResourceType)
                .IsUnicode(false);

            modelBuilder.Entity<SvcMailboxSizes>()
                .Property(e => e.UserPrincipalName)
                .IsUnicode(false);

            modelBuilder.Entity<SvcMailboxSizes>()
                .Property(e => e.MailboxDatabase)
                .IsUnicode(false);

            modelBuilder.Entity<SvcQueue>()
                .Property(e => e.UserPrincipalName)
                .IsUnicode(false);

            modelBuilder.Entity<SvcQueue>()
                .Property(e => e.CompanyCode)
                .IsUnicode(false);

            modelBuilder.Entity<Users>()
                .Property(e => e.CompanyCode)
                .IsUnicode(false);

            modelBuilder.Entity<Users>()
                .Property(e => e.DistinguishedName)
                .IsUnicode(false);

            modelBuilder.Entity<Plans_TerminalServices>()
                .Property(e => e.ResellerCode)
                .IsUnicode(false);

            modelBuilder.Entity<PriceOverride>()
                .Property(e => e.CompanyCode)
                .IsUnicode(false);

            modelBuilder.Entity<PriceOverride>()
                .Property(e => e.Product)
                .IsUnicode(false);

            modelBuilder.Entity<UserPlans>()
                .Property(e => e.CompanyCode)
                .IsUnicode(false);*/
        }

        public override int SaveChanges()
        {
            try
            {
                logger.DebugFormat("Saving database changes");
                return base.SaveChanges();
            }
            catch (DbEntityValidationException ex)
            {
                // Retrieve the error messages as a list of strings.
                var errorMessages = ex.EntityValidationErrors
                        .SelectMany(x => x.ValidationErrors)
                        .Select(x => x.ErrorMessage);

                // Join the list to a single string.
                var fullErrorMessage = string.Join("; ", errorMessages);

                // Combine the original exception message with the new one.
                var exceptionMessage = string.Concat(ex.Message, " The validation errors are: ", fullErrorMessage);

                // Throw a new DbEntityValidationException with the improved exception message.
                throw new DbEntityValidationException(exceptionMessage, ex.EntityValidationErrors);
            }
        }

        #region Tables

        public virtual DbSet<AuditTrace> AuditTrace { get; set; }
        public virtual DbSet<Companies> Companies { get; set; }
        public virtual DbSet<Contacts> Contacts { get; set; }
        public virtual DbSet<DistributionGroups> DistributionGroups { get; set; }
        public virtual DbSet<Domains> Domains { get; set; }
        public virtual DbSet<Plans_Citrix> Plans_Citrix { get; set; }
        public virtual DbSet<Plans_ExchangeActiveSync> Plans_ExchangeActiveSync { get; set; }
        public virtual DbSet<Plans_ExchangeMailbox> Plans_ExchangeMailbox { get; set; }
        public virtual DbSet<Plans_Organization> Plans_Organization { get; set; }
        public virtual DbSet<Plans_ExchangeArchiving> Plans_ExchangeArchiving { get; set; }
        public virtual DbSet<Prices> Prices { get; set; }
        public virtual DbSet<ResourceMailboxes> ResourceMailboxes { get; set; }
        public virtual DbSet<Statistics> Statistics { get; set; }
        public virtual DbSet<SvcMailboxDatabaseSizes> SvcMailboxDatabaseSizes { get; set; }
        public virtual DbSet<SvcMailboxSizes> SvcMailboxSizes { get; set; }
        public virtual DbSet<SvcQueue> SvcQueue { get; set; }
        public virtual DbSet<SvcTask> SvcTask { get; set; }
        public virtual DbSet<UserPermission> UserPermission { get; set; }
        public virtual DbSet<UserRoles> UserRoles { get; set; }
        public virtual DbSet<UserPlansCitrix> UserPlansCitrix { get; set; }
        public virtual DbSet<Users> Users { get; set; }
        public virtual DbSet<Plans_TerminalServices> Plans_TerminalServices { get; set; }
        public virtual DbSet<PriceOverride> PriceOverride { get; set; }
        public virtual DbSet<UserPlans> UserPlans { get; set; }
        public virtual DbSet<Branding> Brandings { get; set; }
        #endregion
    }
}

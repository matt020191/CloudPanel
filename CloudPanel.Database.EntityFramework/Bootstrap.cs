﻿using CloudPanel.Base.Config;
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
        private static readonly ILog logger = log4net.LogManager.GetLogger("SQL");
        
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
        public virtual DbSet<StatMailboxSizes> StatMailboxSize { get; set; }
        public virtual DbSet<StatMailboxArchiveSizes> StatMailboxArchiveSize { get; set; }
        #endregion
    }
}

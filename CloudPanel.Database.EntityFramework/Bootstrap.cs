using CloudPanel.Base.Database.Models;
using CloudPanel.Database.EntityFramework.Migrations;
using CloudPanel.Database.EntityFramework.Models;
using CloudPanel.Database.EntityFramework.Models.Mapping;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Migrations;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.Linq;

// NB: http://stackoverflow.com/questions/16210771/entity-framework-code-first-without-app-config
namespace CloudPanel.Database.EntityFramework
{
    internal sealed class CloudPanelDbConfiguration : DbConfiguration
    {
    }

    internal sealed class CloudPanelContextInitializer : IDatabaseInitializer<CloudPanelContext>
    {
        #region IDatabaseInitializer<CloudPanelContext> Members

        public void InitializeDatabase(CloudPanelContext context)
        {
            //if (!context.Database.Exists())
            //{
                var configuration = new Configuration
                {
                   TargetDatabase = new DbConnectionInfo(context.Database.Connection.ConnectionString, @"System.Data.SqlClient"),
                };

                var migrator = new DbMigrator(configuration);
                migrator.Update();
            //}
        }

        #endregion
    }

    public sealed class CloudPanelContext : DbContext
    {
        public CloudPanelContext()
        {
        }

        public CloudPanelContext(string connectionString) : base(connectionString)
        {
            System.Data.Entity.Database.SetInitializer(new CloudPanelContextInitializer());
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();

            modelBuilder.Configurations.Add(new ApiAccessMap());
            modelBuilder.Configurations.Add(new AuditMap());
            modelBuilder.Configurations.Add(new AuditLoginMap());
            modelBuilder.Configurations.Add(new CompanyMap());
            modelBuilder.Configurations.Add(new CompanyStatMap());
            modelBuilder.Configurations.Add(new ContactMap());
            modelBuilder.Configurations.Add(new DistributionGroupMap());
            modelBuilder.Configurations.Add(new DomainMap());
            modelBuilder.Configurations.Add(new LogTableMap());
            modelBuilder.Configurations.Add(new Plans_CitrixMap());
            modelBuilder.Configurations.Add(new Plans_ExchangeActiveSyncMap());
            modelBuilder.Configurations.Add(new Plans_ExchangeMailboxMap());
            modelBuilder.Configurations.Add(new Plans_OrganizationMap());
            modelBuilder.Configurations.Add(new PriceOverrideMap());
            modelBuilder.Configurations.Add(new PriceMap());
            modelBuilder.Configurations.Add(new ResourceMailboxMap());
            modelBuilder.Configurations.Add(new SettingMap());
            modelBuilder.Configurations.Add(new Stats_CitrixCountMap());
            modelBuilder.Configurations.Add(new Stats_ExchCountMap());
            modelBuilder.Configurations.Add(new Stats_UserCountMap());
            modelBuilder.Configurations.Add(new SvcMailboxDatabaseSizeMap());
            modelBuilder.Configurations.Add(new SvcMailboxSizeMap());
            modelBuilder.Configurations.Add(new SvcQueueMap());
            modelBuilder.Configurations.Add(new SvcTaskMap());
            modelBuilder.Configurations.Add(new UserPermissionMap());
            modelBuilder.Configurations.Add(new UserPlansCitrixMap());
            modelBuilder.Configurations.Add(new UserMap());

            
            /*modelBuilder.Entity<DeleteCompany>().MapToStoredProcedures(s =>
                {
                    s.Delete(d => d.HasName("DeleteCompany").Parameter(
                        p => p.CompanyCode, "CompanyCode")
                    );
                });
            
            modelBuilder.Entity<DeleteUser>().MapToStoredProcedures(s =>
                {
                    s.Delete(d => d.HasName("DeleteUser"));
                });

            modelBuilder.Entity<DisableExchange>().MapToStoredProcedures(s =>
                {
                    s.Delete(d => d.HasName("DisableExchange"));
                });

            modelBuilder.Entity<DisableMailbox>().MapToStoredProcedures(s =>
                {
                    s.Delete(d => d.HasName("DisableMailbox"));
                });

            modelBuilder.Entity<UpdatePriceOverride>().MapToStoredProcedures(s =>
                {
                    s.Update(u => u.HasName("UpdatePriceOverride"));
                });
             */
        }

        public override int SaveChanges()
        {
            try
            {
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

        #region Temp Stored Procedures Commands

        public void spDeleteCompany(string companyCode)
        {
            var companyCodeParm = new SqlParameter("CompanyCode", companyCode);

            this.Database.ExecuteSqlCommand("DeleteCompany @companyCode", companyCodeParm);
        }

        #endregion

        #region Tables

        public DbSet<ApiAccess> ApiAccesses { get; set; }
        public DbSet<Audit> Audits { get; set; }
        public DbSet<AuditLogin> AuditLogins { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<CompanyStat> CompanyStats { get; set; }
        public DbSet<Contact> Contacts { get; set; }
        public DbSet<DistributionGroup> DistributionGroups { get; set; }
        public DbSet<Domain> Domains { get; set; }
        public DbSet<LogTable> LogTables { get; set; }
        public DbSet<Plans_Citrix> Plans_Citrix { get; set; }
        public DbSet<Plans_ExchangeActiveSync> Plans_ExchangeActiveSync { get; set; }
        public DbSet<Plans_ExchangeMailbox> Plans_ExchangeMailbox { get; set; }
        public DbSet<Plans_Organization> Plans_Organization { get; set; }
        public DbSet<PriceOverride> PriceOverrides { get; set; }
        public DbSet<Price> Prices { get; set; }
        public DbSet<ResourceMailbox> ResourceMailboxes { get; set; }
        public DbSet<Setting> Settings { get; set; }
        public DbSet<Stats_CitrixCount> Stats_CitrixCount { get; set; }
        public DbSet<Stats_ExchCount> Stats_ExchCount { get; set; }
        public DbSet<Stats_UserCount> Stats_UserCount { get; set; }
        public DbSet<SvcMailboxDatabaseSize> SvcMailboxDatabaseSizes { get; set; }
        public DbSet<SvcMailboxSize> SvcMailboxSizes { get; set; }
        public DbSet<SvcQueue> SvcQueues { get; set; }
        public DbSet<SvcTask> SvcTasks { get; set; }
        public DbSet<UserPermission> UserPermissions { get; set; }
        public DbSet<UserPlansCitrix> UserPlansCitrix { get; set; }
        public DbSet<User> Users { get; set; }

        #endregion

        #region Stored Procedures
        
        /*
        public DbSet<DeleteCompany> DeleteCompany { get; set; }

        public DbSet<DeleteUser> DeleteUser { get; set; }

        public DbSet<DisableExchange> DisableExchange { get; set; }

        public DbSet<DisableMailbox> DisableMailbox { get; set; }

        public DbSet<UpdatePriceOverride> UpdatePriceOverride {  get;set ;}
        */
        #endregion
    }
}

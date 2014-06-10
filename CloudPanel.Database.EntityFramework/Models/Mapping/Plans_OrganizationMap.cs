using CloudPanel.Base.Database.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace CloudPanel.Database.EntityFramework.Models.Mapping
{
    public class Plans_OrganizationMap : EntityTypeConfiguration<Plans_Organization>
    {
        public Plans_OrganizationMap()
        {
            // Primary Key
            this.HasKey(t => t.OrgPlanID);

            // Properties
            this.Property(t => t.OrgPlanName)
                .IsRequired()
                .HasMaxLength(50);

            this.Property(t => t.ResellerCode)
                .HasMaxLength(255);

            // Table & Column Mappings
            this.ToTable("Plans_Organization");
            this.Property(t => t.OrgPlanID).HasColumnName("OrgPlanID");
            this.Property(t => t.OrgPlanName).HasColumnName("OrgPlanName");
            this.Property(t => t.ProductID).HasColumnName("ProductID");
            this.Property(t => t.ResellerCode).HasColumnName("ResellerCode");
            this.Property(t => t.MaxUsers).HasColumnName("MaxUsers");
            this.Property(t => t.MaxDomains).HasColumnName("MaxDomains");
            this.Property(t => t.MaxExchangeMailboxes).HasColumnName("MaxExchangeMailboxes");
            this.Property(t => t.MaxExchangeContacts).HasColumnName("MaxExchangeContacts");
            this.Property(t => t.MaxExchangeDistLists).HasColumnName("MaxExchangeDistLists");
            this.Property(t => t.MaxExchangePublicFolders).HasColumnName("MaxExchangePublicFolders");
            this.Property(t => t.MaxExchangeMailPublicFolders).HasColumnName("MaxExchangeMailPublicFolders");
            this.Property(t => t.MaxExchangeKeepDeletedItems).HasColumnName("MaxExchangeKeepDeletedItems");
            this.Property(t => t.MaxExchangeActivesyncPolicies).HasColumnName("MaxExchangeActivesyncPolicies");
            this.Property(t => t.MaxTerminalServerUsers).HasColumnName("MaxTerminalServerUsers");
            this.Property(t => t.MaxExchangeResourceMailboxes).HasColumnName("MaxExchangeResourceMailboxes");
        }
    }
}

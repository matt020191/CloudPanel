using CloudPanel.Base.Database.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace CloudPanel.Database.EntityFramework.Models.Mapping
{
    public class ResourceMailboxMap : EntityTypeConfiguration<ResourceMailbox>
    {
        public ResourceMailboxMap()
        {
            // Primary Key
            this.HasKey(t => t.ResourceID);

            // Properties
            this.Property(t => t.DisplayName)
                .IsRequired()
                .HasMaxLength(255);

            this.Property(t => t.CompanyCode)
                .IsRequired()
                .HasMaxLength(255);

            this.Property(t => t.UserPrincipalName)
                .IsRequired()
                .HasMaxLength(255);

            this.Property(t => t.PrimarySmtpAddress)
                .IsRequired()
                .HasMaxLength(255);

            this.Property(t => t.ResourceType)
                .IsRequired()
                .HasMaxLength(10);

            // Table & Column Mappings
            this.ToTable("ResourceMailboxes");
            this.Property(t => t.ResourceID).HasColumnName("ResourceID");
            this.Property(t => t.DisplayName).HasColumnName("DisplayName");
            this.Property(t => t.CompanyCode).HasColumnName("CompanyCode");
            this.Property(t => t.UserPrincipalName).HasColumnName("UserPrincipalName");
            this.Property(t => t.PrimarySmtpAddress).HasColumnName("PrimarySmtpAddress");
            this.Property(t => t.ResourceType).HasColumnName("ResourceType");
            this.Property(t => t.MailboxPlan).HasColumnName("MailboxPlan");
            this.Property(t => t.AdditionalMB).HasColumnName("AdditionalMB");
        }
    }
}

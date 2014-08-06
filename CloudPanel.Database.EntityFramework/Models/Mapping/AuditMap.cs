using CloudPanel.Base.Database.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace CloudPanel.Database.EntityFramework.Models.Mapping
{
    public class AuditMap : EntityTypeConfiguration<Audit>
    {
        public AuditMap()
        {
            // Primary Key
            this.HasKey(t => t.AuditID);

            // Properties
            this.Property(t => t.CompanyCode)
                .IsRequired()
                .HasMaxLength(255);

            this.Property(t => t.Username)
                .IsRequired()
                .HasMaxLength(50);

            this.Property(t => t.Timestamp)
                .IsRequired();

            this.Property(t => t.Method)
                .IsRequired()
                .HasMaxLength(16);

            this.Property(t => t.Path)
                .IsRequired()
                .HasMaxLength(512);

            this.Property(t => t.IdentifyingInfo)
                .IsOptional()
                .HasMaxLength(255);

            this.Property(t => t.UserHostAddress)
                .IsRequired()
                .HasMaxLength(64);

            // Table & Column Mappings
            this.ToTable("Audit");
            this.Property(t => t.AuditID).HasColumnName("AuditID");
            this.Property(t => t.CompanyCode).HasColumnName("CompanyCode");
            this.Property(t => t.Username).HasColumnName("Username");
            this.Property(t => t.Timestamp).HasColumnName("Timestamp");
            this.Property(t => t.Method).HasColumnName("Method");
            this.Property(t => t.Path).HasColumnName("Path");
            this.Property(t => t.IdentifyingInfo).HasColumnName("IdentifyingInfo");
            this.Property(t => t.UserHostAddress).HasColumnName("UserHostAddress");
        }
    }
}

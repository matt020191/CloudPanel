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

            this.Property(t => t.Variable1)
                .IsRequired();

            // Table & Column Mappings
            this.ToTable("Audit");
            this.Property(t => t.AuditID).HasColumnName("AuditID");
            this.Property(t => t.CompanyCode).HasColumnName("CompanyCode");
            this.Property(t => t.Username).HasColumnName("Username");
            this.Property(t => t.Date).HasColumnName("Date");
            this.Property(t => t.ActionID).HasColumnName("ActionID");
            this.Property(t => t.Variable1).HasColumnName("Variable1");
            this.Property(t => t.Variable2).HasColumnName("Variable2");
        }
    }
}

using CloudPanel.Base.Database.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace CloudPanel.Database.EntityFramework.Models.Mapping
{
    public class LogTableMap : EntityTypeConfiguration<LogTable>
    {
        public LogTableMap()
        {
            // Primary Key
            this.HasKey(t => t.AuditID);

            // Properties
            this.Property(t => t.UserPrincipalName)
                .HasMaxLength(255);

            this.Property(t => t.CompanyCode)
                .HasMaxLength(255);

            this.Property(t => t.ResellerCode)
                .HasMaxLength(255);

            this.Property(t => t.Message)
                .IsRequired();

            // Table & Column Mappings
            this.ToTable("LogTable");
            this.Property(t => t.AuditID).HasColumnName("AuditID");
            this.Property(t => t.LogTime).HasColumnName("LogTime");
            this.Property(t => t.LogType).HasColumnName("LogType");
            this.Property(t => t.CodeClass).HasColumnName("CodeClass");
            this.Property(t => t.UserPrincipalName).HasColumnName("UserPrincipalName");
            this.Property(t => t.CompanyCode).HasColumnName("CompanyCode");
            this.Property(t => t.ResellerCode).HasColumnName("ResellerCode");
            this.Property(t => t.Message).HasColumnName("Message");
            this.Property(t => t.RecommendedAction).HasColumnName("RecommendedAction");
            this.Property(t => t.Exception).HasColumnName("Exception");
        }
    }
}

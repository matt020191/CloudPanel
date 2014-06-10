using CloudPanel.Base.Database.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace CloudPanel.Database.EntityFramework.Models.Mapping
{
    public class AuditLoginMap : EntityTypeConfiguration<AuditLogin>
    {
        public AuditLoginMap()
        {
            // Primary Key
            this.HasKey(t => t.ID);

            // Properties
            this.Property(t => t.IPAddress)
                .IsRequired()
                .HasMaxLength(128);

            this.Property(t => t.Username)
                .IsRequired()
                .HasMaxLength(255);

            // Table & Column Mappings
            this.ToTable("AuditLogin");
            this.Property(t => t.ID).HasColumnName("ID");
            this.Property(t => t.IPAddress).HasColumnName("IPAddress");
            this.Property(t => t.Username).HasColumnName("Username");
            this.Property(t => t.LoginStatus).HasColumnName("LoginStatus");
            this.Property(t => t.AuditTimeStamp).HasColumnName("AuditTimeStamp");
        }
    }
}

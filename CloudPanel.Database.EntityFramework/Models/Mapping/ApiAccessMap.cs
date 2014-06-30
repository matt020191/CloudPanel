using CloudPanel.Base.Database.Models;
using System.Data.Entity.ModelConfiguration;

namespace CloudPanel.Database.EntityFramework.Models.Mapping
{
    public class ApiAccessMap : EntityTypeConfiguration<ApiAccess>
    {
        public ApiAccessMap()
        {
            // Primary Key
            this.HasKey(t => t.CustomerKey);

            // Properties
            this.Property(t => t.CustomerKey)
                .IsRequired()
                .HasMaxLength(255);

            this.Property(t => t.CustomerSecret)
                .IsRequired()
                .HasMaxLength(255);

            this.Property(t => t.CompanyCode)
                .IsRequired()
                .HasMaxLength(255);

            // Table & Column Mappings
            this.ToTable("ApiAccess");
            this.Property(t => t.CustomerKey).HasColumnName("CustomerKey");
            this.Property(t => t.CustomerSecret).HasColumnName("CustomerSecret");
            this.Property(t => t.CompanyCode).HasColumnName("CompanyCode");
        }
    }
}

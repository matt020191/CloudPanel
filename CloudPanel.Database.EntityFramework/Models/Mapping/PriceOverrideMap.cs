using CloudPanel.Base.Database.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace CloudPanel.Database.EntityFramework.Models.Mapping
{
    public class PriceOverrideMap : EntityTypeConfiguration<PriceOverride>
    {
        public PriceOverrideMap()
        {
            // Primary Key
            this.HasKey(t => t.CompanyCode);

            // Properties
            this.Property(t => t.CompanyCode)
                .IsRequired()
                .HasMaxLength(255);

            this.Property(t => t.Price)
                .HasMaxLength(25);

            this.Property(t => t.Product)
                .HasMaxLength(25);

            // Table & Column Mappings
            this.ToTable("PriceOverride");
            this.Property(t => t.CompanyCode).HasColumnName("CompanyCode");
            this.Property(t => t.Price).HasColumnName("Price");
            this.Property(t => t.PlanID).HasColumnName("PlanID");
            this.Property(t => t.Product).HasColumnName("Product");
        }
    }
}

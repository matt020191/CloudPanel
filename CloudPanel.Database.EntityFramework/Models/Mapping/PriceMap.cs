using CloudPanel.Base.Database.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace CloudPanel.Database.EntityFramework.Models.Mapping
{
    public class PriceMap : EntityTypeConfiguration<Price>
    {
        public PriceMap()
        {
            // Primary Key
            this.HasKey(t => t.PriceID);

            // Properties
            this.Property(t => t.CompanyCode)
                .IsRequired()
                .HasMaxLength(255);

            // Table & Column Mappings
            this.ToTable("Prices");
            this.Property(t => t.PriceID).HasColumnName("PriceID");
            this.Property(t => t.ProductID).HasColumnName("ProductID");
            this.Property(t => t.PlanID).HasColumnName("PlanID");
            this.Property(t => t.CompanyCode).HasColumnName("CompanyCode");
            this.Property(t => t.Price1).HasColumnName("Price");
        }
    }
}

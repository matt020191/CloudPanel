using CloudPanel.Base.Database.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace CloudPanel.Database.EntityFramework.Models.Mapping
{
    public class DomainMap : EntityTypeConfiguration<Domain>
    {
        public DomainMap()
        {
            // Primary Key
            this.HasKey(t => t.DomainID);

            // Properties
            this.Property(t => t.CompanyCode)
                .HasMaxLength(255);

            this.Property(t => t.Domain1)
                .IsRequired()
                .HasMaxLength(255);

            // Table & Column Mappings
            this.ToTable("Domains");
            this.Property(t => t.DomainID).HasColumnName("DomainID");
            this.Property(t => t.CompanyCode).HasColumnName("CompanyCode");
            this.Property(t => t.Domain1).HasColumnName("Domain");
            this.Property(t => t.IsSubDomain).HasColumnName("IsSubDomain");
            this.Property(t => t.IsDefault).HasColumnName("IsDefault");
            this.Property(t => t.IsAcceptedDomain).HasColumnName("IsAcceptedDomain");
            this.Property(t => t.IsLyncDomain).HasColumnName("IsLyncDomain");
            this.Property(t => t.DomainType).HasColumnName("DomainType");
        }
    }
}

using CloudPanel.Base.Database.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace CloudPanel.Database.EntityFramework.Models.Mapping
{
    public class CompanyMap : EntityTypeConfiguration<Company>
    {
        public CompanyMap()
        {
            // Primary Key
            this.HasKey(t => t.CompanyId);

            // Properties
            this.Property(t => t.ResellerCode)
                .HasMaxLength(255);

            this.Property(t => t.CompanyName)
                .IsRequired()
                .HasMaxLength(100);

            this.Property(t => t.CompanyCode)
                .IsRequired()
                .HasMaxLength(255);

            this.Property(t => t.Street)
                .IsRequired()
                .HasMaxLength(255);

            this.Property(t => t.City)
                .IsRequired()
                .HasMaxLength(100);

            this.Property(t => t.State)
                .IsRequired()
                .HasMaxLength(100);

            this.Property(t => t.ZipCode)
                .IsRequired()
                .HasMaxLength(50);

            this.Property(t => t.PhoneNumber)
                .IsRequired()
                .HasMaxLength(50);

            this.Property(t => t.Website)
                .HasMaxLength(255);

            this.Property(t => t.AdminName)
                .IsRequired()
                .HasMaxLength(100);

            this.Property(t => t.AdminEmail)
                .IsRequired()
                .HasMaxLength(255);

            this.Property(t => t.DistinguishedName)
                .IsRequired()
                .HasMaxLength(255);

            this.Property(t => t.Country)
                .HasMaxLength(50);

            // Table & Column Mappings
            this.ToTable("Companies");
            this.Property(t => t.CompanyId).HasColumnName("CompanyId");
            this.Property(t => t.IsReseller).HasColumnName("IsReseller");
            this.Property(t => t.ResellerCode).HasColumnName("ResellerCode");
            this.Property(t => t.OrgPlanID).HasColumnName("OrgPlanID");
            this.Property(t => t.CompanyName).HasColumnName("CompanyName");
            this.Property(t => t.CompanyCode).HasColumnName("CompanyCode");
            this.Property(t => t.Street).HasColumnName("Street");
            this.Property(t => t.City).HasColumnName("City");
            this.Property(t => t.State).HasColumnName("State");
            this.Property(t => t.ZipCode).HasColumnName("ZipCode");
            this.Property(t => t.PhoneNumber).HasColumnName("PhoneNumber");
            this.Property(t => t.Website).HasColumnName("Website");
            this.Property(t => t.Description).HasColumnName("Description");
            this.Property(t => t.AdminName).HasColumnName("AdminName");
            this.Property(t => t.AdminEmail).HasColumnName("AdminEmail");
            this.Property(t => t.DistinguishedName).HasColumnName("DistinguishedName");
            this.Property(t => t.Created).HasColumnName("Created");
            this.Property(t => t.ExchEnabled).HasColumnName("ExchEnabled");
            this.Property(t => t.LyncEnabled).HasColumnName("LyncEnabled");
            this.Property(t => t.CitrixEnabled).HasColumnName("CitrixEnabled");
            this.Property(t => t.ExchPFPlan).HasColumnName("ExchPFPlan");
            this.Property(t => t.Country).HasColumnName("Country");
            this.Property(t => t.ExchPermFixed).HasColumnName("ExchPermFixed");
        }
    }
}

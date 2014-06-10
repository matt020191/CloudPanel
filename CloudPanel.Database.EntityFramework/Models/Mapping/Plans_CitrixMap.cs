using CloudPanel.Base.Database.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace CloudPanel.Database.EntityFramework.Models.Mapping
{
    public class Plans_CitrixMap : EntityTypeConfiguration<Plans_Citrix>
    {
        public Plans_CitrixMap()
        {
            // Primary Key
            this.HasKey(t => t.CitrixPlanID);

            // Properties
            this.Property(t => t.Name)
                .IsRequired()
                .HasMaxLength(56);

            this.Property(t => t.GroupName)
                .IsRequired()
                .HasMaxLength(64);

            this.Property(t => t.CompanyCode)
                .HasMaxLength(255);

            this.Property(t => t.Price)
                .HasMaxLength(20);

            this.Property(t => t.Cost)
                .HasMaxLength(20);

            this.Property(t => t.PictureURL)
                .HasMaxLength(255);

            // Table & Column Mappings
            this.ToTable("Plans_Citrix");
            this.Property(t => t.CitrixPlanID).HasColumnName("CitrixPlanID");
            this.Property(t => t.Name).HasColumnName("Name");
            this.Property(t => t.GroupName).HasColumnName("GroupName");
            this.Property(t => t.Description).HasColumnName("Description");
            this.Property(t => t.IsServer).HasColumnName("IsServer");
            this.Property(t => t.CompanyCode).HasColumnName("CompanyCode");
            this.Property(t => t.Price).HasColumnName("Price");
            this.Property(t => t.Cost).HasColumnName("Cost");
            this.Property(t => t.PictureURL).HasColumnName("PictureURL");
        }
    }
}

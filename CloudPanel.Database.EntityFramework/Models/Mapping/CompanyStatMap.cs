using CloudPanel.Base.Database.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace CloudPanel.Database.EntityFramework.Models.Mapping
{
    public class CompanyStatMap : EntityTypeConfiguration<CompanyStat>
    {
        public CompanyStatMap()
        {
            // Primary Key
            this.HasKey(t => t.CompanyID);

            // Properties
            this.Property(t => t.CompanyID)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            this.Property(t => t.CompanyCode)
                .HasMaxLength(255);

            // Table & Column Mappings
            this.ToTable("CompanyStats");
            this.Property(t => t.CompanyID).HasColumnName("CompanyID");
            this.Property(t => t.CompanyCode).HasColumnName("CompanyCode");
            this.Property(t => t.UserCount).HasColumnName("UserCount");
            this.Property(t => t.DomainCount).HasColumnName("DomainCount");
            this.Property(t => t.SubDomainCount).HasColumnName("SubDomainCount");
            this.Property(t => t.ExchMailboxCount).HasColumnName("ExchMailboxCount");
            this.Property(t => t.ExchContactsCount).HasColumnName("ExchContactsCount");
            this.Property(t => t.ExchDistListsCount).HasColumnName("ExchDistListsCount");
            this.Property(t => t.ExchPublicFolderCount).HasColumnName("ExchPublicFolderCount");
            this.Property(t => t.ExchMailPublicFolderCount).HasColumnName("ExchMailPublicFolderCount");
            this.Property(t => t.ExchKeepDeletedItems).HasColumnName("ExchKeepDeletedItems");
            this.Property(t => t.RDPUserCount).HasColumnName("RDPUserCount");
        }
    }
}

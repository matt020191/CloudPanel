using CloudPanel.Base.Database.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace CloudPanel.Database.EntityFramework.Models.Mapping
{
    public class SvcMailboxSizeMap : EntityTypeConfiguration<SvcMailboxSize>
    {
        public SvcMailboxSizeMap()
        {
            // Primary Key
            this.HasKey(t => t.ID);

            // Properties
            this.Property(t => t.UserPrincipalName)
                .IsRequired()
                .HasMaxLength(64);

            this.Property(t => t.MailboxDatabase)
                .IsRequired()
                .HasMaxLength(255);

            this.Property(t => t.TotalItemSizeInKB)
                .IsRequired()
                .HasMaxLength(255);

            this.Property(t => t.TotalDeletedItemSizeInKB)
                .IsRequired()
                .HasMaxLength(255);

            // Table & Column Mappings
            this.ToTable("SvcMailboxSizes");
            this.Property(t => t.ID).HasColumnName("ID");
            this.Property(t => t.UserPrincipalName).HasColumnName("UserPrincipalName");
            this.Property(t => t.MailboxDatabase).HasColumnName("MailboxDatabase");
            this.Property(t => t.TotalItemSizeInKB).HasColumnName("TotalItemSizeInKB");
            this.Property(t => t.TotalDeletedItemSizeInKB).HasColumnName("TotalDeletedItemSizeInKB");
            this.Property(t => t.ItemCount).HasColumnName("ItemCount");
            this.Property(t => t.DeletedItemCount).HasColumnName("DeletedItemCount");
            this.Property(t => t.Retrieved).HasColumnName("Retrieved");
        }
    }
}

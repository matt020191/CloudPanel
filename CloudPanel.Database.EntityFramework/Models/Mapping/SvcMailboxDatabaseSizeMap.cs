using CloudPanel.Base.Database.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace CloudPanel.Database.EntityFramework.Models.Mapping
{
    public class SvcMailboxDatabaseSizeMap : EntityTypeConfiguration<SvcMailboxDatabaseSize>
    {
        public SvcMailboxDatabaseSizeMap()
        {
            // Primary Key
            this.HasKey(t => t.ID);

            // Properties
            this.Property(t => t.DatabaseName)
                .IsRequired()
                .HasMaxLength(64);

            this.Property(t => t.Server)
                .IsRequired()
                .HasMaxLength(64);

            this.Property(t => t.DatabaseSize)
                .IsRequired()
                .HasMaxLength(255);

            // Table & Column Mappings
            this.ToTable("SvcMailboxDatabaseSizes");
            this.Property(t => t.ID).HasColumnName("ID");
            this.Property(t => t.DatabaseName).HasColumnName("DatabaseName");
            this.Property(t => t.Server).HasColumnName("Server");
            this.Property(t => t.DatabaseSize).HasColumnName("DatabaseSize");
            this.Property(t => t.Retrieved).HasColumnName("Retrieved");
        }
    }
}

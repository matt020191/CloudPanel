using CloudPanel.Base.Database.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace CloudPanel.Database.EntityFramework.Models.Mapping
{
    public class UserPermissionMap : EntityTypeConfiguration<UserPermission>
    {
        public UserPermissionMap()
        {
            // Primary Key
            this.HasKey(t => t.UserID);

            // Properties
            this.Property(t => t.UserID)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            // Table & Column Mappings
            this.ToTable("UserPermissions");
            this.Property(t => t.UserID).HasColumnName("UserID");
            this.Property(t => t.EnableExchange).HasColumnName("EnableExchange");
            this.Property(t => t.DisableExchange).HasColumnName("DisableExchange");
            this.Property(t => t.AddDomain).HasColumnName("AddDomain");
            this.Property(t => t.DeleteDomain).HasColumnName("DeleteDomain");
            this.Property(t => t.EnableAcceptedDomain).HasColumnName("EnableAcceptedDomain");
            this.Property(t => t.DisableAcceptedDomain).HasColumnName("DisableAcceptedDomain");
        }
    }
}

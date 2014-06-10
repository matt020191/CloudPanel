using CloudPanel.Base.Database.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace CloudPanel.Database.EntityFramework.Models.Mapping
{
    public class UserPlansCitrixMap : EntityTypeConfiguration<UserPlansCitrix>
    {
        public UserPlansCitrixMap()
        {
            // Primary Key
            this.HasKey(t => t.UPCID);

            // Properties
            // Table & Column Mappings
            this.ToTable("UserPlansCitrix");
            this.Property(t => t.UPCID).HasColumnName("UPCID");
            this.Property(t => t.UserID).HasColumnName("UserID");
            this.Property(t => t.CitrixPlanID).HasColumnName("CitrixPlanID");
        }
    }
}

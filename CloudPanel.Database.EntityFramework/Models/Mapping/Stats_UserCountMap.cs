using CloudPanel.Base.Database.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace CloudPanel.Database.EntityFramework.Models.Mapping
{
    public class Stats_UserCountMap : EntityTypeConfiguration<Stats_UserCount>
    {
        public Stats_UserCountMap()
        {
            // Primary Key
            this.HasKey(t => t.StatDate);

            // Properties
            // Table & Column Mappings
            this.ToTable("Stats_UserCount");
            this.Property(t => t.StatDate).HasColumnName("StatDate");
            this.Property(t => t.UserCount).HasColumnName("UserCount");
        }
    }
}

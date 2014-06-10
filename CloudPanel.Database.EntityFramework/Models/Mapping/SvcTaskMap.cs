using CloudPanel.Base.Database.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace CloudPanel.Database.EntityFramework.Models.Mapping
{
    public class SvcTaskMap : EntityTypeConfiguration<SvcTask>
    {
        public SvcTaskMap()
        {
            // Primary Key
            this.HasKey(t => t.SvcTaskID);

            // Properties
            // Table & Column Mappings
            this.ToTable("SvcTask");
            this.Property(t => t.SvcTaskID).HasColumnName("SvcTaskID");
            this.Property(t => t.TaskType).HasColumnName("TaskType");
            this.Property(t => t.LastRun).HasColumnName("LastRun");
            this.Property(t => t.NextRun).HasColumnName("NextRun");
            this.Property(t => t.TaskOutput).HasColumnName("TaskOutput");
            this.Property(t => t.TaskDelayInMinutes).HasColumnName("TaskDelayInMinutes");
            this.Property(t => t.TaskCreated).HasColumnName("TaskCreated");
            this.Property(t => t.Reoccurring).HasColumnName("Reoccurring");
        }
    }
}

using CloudPanel.Base.Database.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace CloudPanel.Database.EntityFramework.Models.Mapping
{
    public class SvcQueueMap : EntityTypeConfiguration<SvcQueue>
    {
        public SvcQueueMap()
        {
            // Primary Key
            this.HasKey(t => t.SvcQueueID);

            // Properties
            this.Property(t => t.UserPrincipalName)
                .HasMaxLength(255);

            this.Property(t => t.CompanyCode)
                .HasMaxLength(255);

            // Table & Column Mappings
            this.ToTable("SvcQueue");
            this.Property(t => t.SvcQueueID).HasColumnName("SvcQueueID");
            this.Property(t => t.TaskID).HasColumnName("TaskID");
            this.Property(t => t.UserPrincipalName).HasColumnName("UserPrincipalName");
            this.Property(t => t.CompanyCode).HasColumnName("CompanyCode");
            this.Property(t => t.TaskOutput).HasColumnName("TaskOutput");
            this.Property(t => t.TaskCreated).HasColumnName("TaskCreated");
            this.Property(t => t.TaskCompleted).HasColumnName("TaskCompleted");
            this.Property(t => t.TaskDelayInMinutes).HasColumnName("TaskDelayInMinutes");
            this.Property(t => t.TaskSuccess).HasColumnName("TaskSuccess");
        }
    }
}

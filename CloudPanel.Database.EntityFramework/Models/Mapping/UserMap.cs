using CloudPanel.Base.Database.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace CloudPanel.Database.EntityFramework.Models.Mapping
{
    public class UserMap : EntityTypeConfiguration<User>
    {
        public UserMap()
        {
            // Primary Key
            this.HasKey(t => t.UserGuid);

            // Properties
            this.Property(t => t.ID)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            this.Property(t => t.CompanyCode)
                .HasMaxLength(255);

            this.Property(t => t.sAMAccountName)
                .HasMaxLength(255);

            this.Property(t => t.UserPrincipalName)
                .IsRequired()
                .HasMaxLength(255);

            this.Property(t => t.DistinguishedName)
                .HasMaxLength(255);

            this.Property(t => t.DisplayName)
                .IsRequired()
                .HasMaxLength(100);

            this.Property(t => t.Firstname)
                .HasMaxLength(50);

            this.Property(t => t.Middlename)
                .HasMaxLength(50);

            this.Property(t => t.Lastname)
                .HasMaxLength(50);

            this.Property(t => t.Email)
                .HasMaxLength(255);

            this.Property(t => t.Department)
                .HasMaxLength(255);

            // Table & Column Mappings
            this.ToTable("Users");
            this.Property(t => t.ID).HasColumnName("ID");
            this.Property(t => t.UserGuid).HasColumnName("UserGuid");
            this.Property(t => t.CompanyCode).HasColumnName("CompanyCode");
            this.Property(t => t.sAMAccountName).HasColumnName("sAMAccountName");
            this.Property(t => t.UserPrincipalName).HasColumnName("UserPrincipalName");
            this.Property(t => t.DistinguishedName).HasColumnName("DistinguishedName");
            this.Property(t => t.DisplayName).HasColumnName("DisplayName");
            this.Property(t => t.Firstname).HasColumnName("Firstname");
            this.Property(t => t.Middlename).HasColumnName("Middlename");
            this.Property(t => t.Lastname).HasColumnName("Lastname");
            this.Property(t => t.Email).HasColumnName("Email");
            this.Property(t => t.Department).HasColumnName("Department");
            this.Property(t => t.IsEnabled).HasColumnName("IsEnabled");
            this.Property(t => t.IsResellerAdmin).HasColumnName("IsResellerAdmin");
            this.Property(t => t.IsCompanyAdmin).HasColumnName("IsCompanyAdmin");
            this.Property(t => t.MailboxPlan).HasColumnName("MailboxPlan");
            this.Property(t => t.TSPlan).HasColumnName("TSPlan");
            this.Property(t => t.LyncPlan).HasColumnName("LyncPlan");
            this.Property(t => t.Created).HasColumnName("Created");
            this.Property(t => t.AdditionalMB).HasColumnName("AdditionalMB");
            this.Property(t => t.ActiveSyncPlan).HasColumnName("ActiveSyncPlan");
            this.Property(t => t.ExchArchivePlan).HasColumnName("ExchArchivePlan");
        }
    }
}

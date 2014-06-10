using CloudPanel.Base.Database.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace CloudPanel.Database.EntityFramework.Models.Mapping
{
    public class Plans_ExchangeActiveSyncMap : EntityTypeConfiguration<Plans_ExchangeActiveSync>
    {
        public Plans_ExchangeActiveSyncMap()
        {
            // Primary Key
            this.HasKey(t => t.ASID);

            // Properties
            this.Property(t => t.CompanyCode)
                .IsRequired()
                .HasMaxLength(255);

            this.Property(t => t.DisplayName)
                .IsRequired()
                .HasMaxLength(150);

            this.Property(t => t.ExchangeName)
                .HasMaxLength(75);

            this.Property(t => t.IncludePastCalendarItems)
                .HasMaxLength(20);

            this.Property(t => t.IncludePastEmailItems)
                .HasMaxLength(20);

            this.Property(t => t.AllowBluetooth)
                .HasMaxLength(10);

            // Table & Column Mappings
            this.ToTable("Plans_ExchangeActiveSync");
            this.Property(t => t.ASID).HasColumnName("ASID");
            this.Property(t => t.CompanyCode).HasColumnName("CompanyCode");
            this.Property(t => t.DisplayName).HasColumnName("DisplayName");
            this.Property(t => t.Description).HasColumnName("Description");
            this.Property(t => t.ExchangeName).HasColumnName("ExchangeName");
            this.Property(t => t.AllowNonProvisionableDevices).HasColumnName("AllowNonProvisionableDevices");
            this.Property(t => t.RefreshIntervalInHours).HasColumnName("RefreshIntervalInHours");
            this.Property(t => t.RequirePassword).HasColumnName("RequirePassword");
            this.Property(t => t.RequireAlphanumericPassword).HasColumnName("RequireAlphanumericPassword");
            this.Property(t => t.EnablePasswordRecovery).HasColumnName("EnablePasswordRecovery");
            this.Property(t => t.RequireEncryptionOnDevice).HasColumnName("RequireEncryptionOnDevice");
            this.Property(t => t.RequireEncryptionOnStorageCard).HasColumnName("RequireEncryptionOnStorageCard");
            this.Property(t => t.AllowSimplePassword).HasColumnName("AllowSimplePassword");
            this.Property(t => t.NumberOfFailedAttempted).HasColumnName("NumberOfFailedAttempted");
            this.Property(t => t.MinimumPasswordLength).HasColumnName("MinimumPasswordLength");
            this.Property(t => t.InactivityTimeoutInMinutes).HasColumnName("InactivityTimeoutInMinutes");
            this.Property(t => t.PasswordExpirationInDays).HasColumnName("PasswordExpirationInDays");
            this.Property(t => t.EnforcePasswordHistory).HasColumnName("EnforcePasswordHistory");
            this.Property(t => t.IncludePastCalendarItems).HasColumnName("IncludePastCalendarItems");
            this.Property(t => t.IncludePastEmailItems).HasColumnName("IncludePastEmailItems");
            this.Property(t => t.LimitEmailSizeInKB).HasColumnName("LimitEmailSizeInKB");
            this.Property(t => t.AllowDirectPushWhenRoaming).HasColumnName("AllowDirectPushWhenRoaming");
            this.Property(t => t.AllowHTMLEmail).HasColumnName("AllowHTMLEmail");
            this.Property(t => t.AllowAttachmentsDownload).HasColumnName("AllowAttachmentsDownload");
            this.Property(t => t.MaximumAttachmentSizeInKB).HasColumnName("MaximumAttachmentSizeInKB");
            this.Property(t => t.AllowRemovableStorage).HasColumnName("AllowRemovableStorage");
            this.Property(t => t.AllowCamera).HasColumnName("AllowCamera");
            this.Property(t => t.AllowWiFi).HasColumnName("AllowWiFi");
            this.Property(t => t.AllowInfrared).HasColumnName("AllowInfrared");
            this.Property(t => t.AllowInternetSharing).HasColumnName("AllowInternetSharing");
            this.Property(t => t.AllowRemoteDesktop).HasColumnName("AllowRemoteDesktop");
            this.Property(t => t.AllowDesktopSync).HasColumnName("AllowDesktopSync");
            this.Property(t => t.AllowBluetooth).HasColumnName("AllowBluetooth");
            this.Property(t => t.AllowBrowser).HasColumnName("AllowBrowser");
            this.Property(t => t.AllowConsumerMail).HasColumnName("AllowConsumerMail");
            this.Property(t => t.IsEnterpriseCAL).HasColumnName("IsEnterpriseCAL");
            this.Property(t => t.AllowTextMessaging).HasColumnName("AllowTextMessaging");
            this.Property(t => t.AllowUnsignedApplications).HasColumnName("AllowUnsignedApplications");
            this.Property(t => t.AllowUnsignedInstallationPackages).HasColumnName("AllowUnsignedInstallationPackages");
        }
    }
}

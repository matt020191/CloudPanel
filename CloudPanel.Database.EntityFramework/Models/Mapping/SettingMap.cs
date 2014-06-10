using CloudPanel.Base.Database.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace CloudPanel.Database.EntityFramework.Models.Mapping
{
    public class SettingMap : EntityTypeConfiguration<Setting>
    {
        public SettingMap()
        {
            // Primary Key
            this.HasKey(t => t.ID);

            // Properties
            this.Property(t => t.BaseOU)
                .IsRequired();

            this.Property(t => t.PrimaryDC)
                .IsRequired()
                .HasMaxLength(50);

            this.Property(t => t.Username)
                .IsRequired()
                .HasMaxLength(50);

            this.Property(t => t.Password)
                .IsRequired();

            this.Property(t => t.SuperAdmins)
                .IsRequired();

            this.Property(t => t.BillingAdmins)
                .IsRequired();

            this.Property(t => t.ExchangeFqdn)
                .IsRequired()
                .HasMaxLength(50);

            this.Property(t => t.ExchangePFServer)
                .IsRequired()
                .HasMaxLength(50);

            this.Property(t => t.ExchangeConnectionType)
                .IsRequired()
                .HasMaxLength(10);

            this.Property(t => t.CurrencySymbol)
                .HasMaxLength(10);

            this.Property(t => t.CurrencyEnglishName)
                .HasMaxLength(200);

            this.Property(t => t.CompanysName)
                .HasMaxLength(255);

            this.Property(t => t.UsersOU)
                .HasMaxLength(255);

            this.Property(t => t.BrandingLoginLogo)
                .HasMaxLength(255);

            this.Property(t => t.BrandingCornerLogo)
                .HasMaxLength(255);

            this.Property(t => t.LyncFrontEnd)
                .HasMaxLength(255);

            this.Property(t => t.LyncUserPool)
                .HasMaxLength(255);

            this.Property(t => t.LyncMeetingUrl)
                .HasMaxLength(255);

            this.Property(t => t.LyncDialinUrl)
                .HasMaxLength(255);

            this.Property(t => t.CompanysLogo)
                .HasMaxLength(255);

            this.Property(t => t.SupportMailAddress)
                .HasMaxLength(255);

            this.Property(t => t.SupportMailServer)
                .HasMaxLength(255);

            this.Property(t => t.SupportMailUsername)
                .HasMaxLength(255);

            this.Property(t => t.SupportMailPassword)
                .HasMaxLength(255);

            this.Property(t => t.SupportMailFrom)
                .HasMaxLength(255);

            // Table & Column Mappings
            this.ToTable("Settings");
            this.Property(t => t.BaseOU).HasColumnName("BaseOU");
            this.Property(t => t.PrimaryDC).HasColumnName("PrimaryDC");
            this.Property(t => t.Username).HasColumnName("Username");
            this.Property(t => t.Password).HasColumnName("Password");
            this.Property(t => t.SuperAdmins).HasColumnName("SuperAdmins");
            this.Property(t => t.BillingAdmins).HasColumnName("BillingAdmins");
            this.Property(t => t.ExchangeFqdn).HasColumnName("ExchangeFqdn");
            this.Property(t => t.ExchangePFServer).HasColumnName("ExchangePFServer");
            this.Property(t => t.ExchangeVersion).HasColumnName("ExchangeVersion");
            this.Property(t => t.ExchangeSSLEnabled).HasColumnName("ExchangeSSLEnabled");
            this.Property(t => t.ExchangeConnectionType).HasColumnName("ExchangeConnectionType");
            this.Property(t => t.PasswordMinLength).HasColumnName("PasswordMinLength");
            this.Property(t => t.PasswordComplexityType).HasColumnName("PasswordComplexityType");
            this.Property(t => t.CitrixEnabled).HasColumnName("CitrixEnabled");
            this.Property(t => t.PublicFolderEnabled).HasColumnName("PublicFolderEnabled");
            this.Property(t => t.LyncEnabled).HasColumnName("LyncEnabled");
            this.Property(t => t.WebsiteEnabled).HasColumnName("WebsiteEnabled");
            this.Property(t => t.SQLEnabled).HasColumnName("SQLEnabled");
            this.Property(t => t.CurrencySymbol).HasColumnName("CurrencySymbol");
            this.Property(t => t.CurrencyEnglishName).HasColumnName("CurrencyEnglishName");
            this.Property(t => t.ResellersEnabled).HasColumnName("ResellersEnabled");
            this.Property(t => t.CompanysName).HasColumnName("CompanysName");
            this.Property(t => t.AllowCustomNameAttrib).HasColumnName("AllowCustomNameAttrib");
            this.Property(t => t.ExchStats).HasColumnName("ExchStats");
            this.Property(t => t.IPBlockingEnabled).HasColumnName("IPBlockingEnabled");
            this.Property(t => t.IPBlockingFailedCount).HasColumnName("IPBlockingFailedCount");
            this.Property(t => t.IPBlockingLockedMinutes).HasColumnName("IPBlockingLockedMinutes");
            this.Property(t => t.ExchDatabases).HasColumnName("ExchDatabases");
            this.Property(t => t.UsersOU).HasColumnName("UsersOU");
            this.Property(t => t.BrandingLoginLogo).HasColumnName("BrandingLoginLogo");
            this.Property(t => t.BrandingCornerLogo).HasColumnName("BrandingCornerLogo");
            this.Property(t => t.LockdownEnabled).HasColumnName("LockdownEnabled");
            this.Property(t => t.LyncFrontEnd).HasColumnName("LyncFrontEnd");
            this.Property(t => t.LyncUserPool).HasColumnName("LyncUserPool");
            this.Property(t => t.LyncMeetingUrl).HasColumnName("LyncMeetingUrl");
            this.Property(t => t.LyncDialinUrl).HasColumnName("LyncDialinUrl");
            this.Property(t => t.CompanysLogo).HasColumnName("CompanysLogo");
            this.Property(t => t.SupportMailEnabled).HasColumnName("SupportMailEnabled");
            this.Property(t => t.SupportMailAddress).HasColumnName("SupportMailAddress");
            this.Property(t => t.SupportMailServer).HasColumnName("SupportMailServer");
            this.Property(t => t.SupportMailPort).HasColumnName("SupportMailPort");
            this.Property(t => t.SupportMailUsername).HasColumnName("SupportMailUsername");
            this.Property(t => t.SupportMailPassword).HasColumnName("SupportMailPassword");
            this.Property(t => t.SupportMailFrom).HasColumnName("SupportMailFrom");
            this.Property(t => t.ID).HasColumnName("ID");
        }
    }
}

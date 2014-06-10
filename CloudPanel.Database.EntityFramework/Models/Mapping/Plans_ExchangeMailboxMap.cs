using CloudPanel.Base.Database.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace CloudPanel.Database.EntityFramework.Models.Mapping
{
    public class Plans_ExchangeMailboxMap : EntityTypeConfiguration<Plans_ExchangeMailbox>
    {
        public Plans_ExchangeMailboxMap()
        {
            // Primary Key
            this.HasKey(t => t.MailboxPlanID);

            // Properties
            this.Property(t => t.MailboxPlanName)
                .IsRequired()
                .HasMaxLength(50);

            this.Property(t => t.ResellerCode)
                .HasMaxLength(255);

            this.Property(t => t.CompanyCode)
                .HasMaxLength(255);

            this.Property(t => t.Price)
                .HasMaxLength(20);

            this.Property(t => t.Cost)
                .HasMaxLength(20);

            this.Property(t => t.AdditionalGBPrice)
                .HasMaxLength(20);

            // Table & Column Mappings
            this.ToTable("Plans_ExchangeMailbox");
            this.Property(t => t.MailboxPlanID).HasColumnName("MailboxPlanID");
            this.Property(t => t.MailboxPlanName).HasColumnName("MailboxPlanName");
            this.Property(t => t.ProductID).HasColumnName("ProductID");
            this.Property(t => t.ResellerCode).HasColumnName("ResellerCode");
            this.Property(t => t.CompanyCode).HasColumnName("CompanyCode");
            this.Property(t => t.MailboxSizeMB).HasColumnName("MailboxSizeMB");
            this.Property(t => t.MaxMailboxSizeMB).HasColumnName("MaxMailboxSizeMB");
            this.Property(t => t.MaxSendKB).HasColumnName("MaxSendKB");
            this.Property(t => t.MaxReceiveKB).HasColumnName("MaxReceiveKB");
            this.Property(t => t.MaxRecipients).HasColumnName("MaxRecipients");
            this.Property(t => t.EnablePOP3).HasColumnName("EnablePOP3");
            this.Property(t => t.EnableIMAP).HasColumnName("EnableIMAP");
            this.Property(t => t.EnableOWA).HasColumnName("EnableOWA");
            this.Property(t => t.EnableMAPI).HasColumnName("EnableMAPI");
            this.Property(t => t.EnableAS).HasColumnName("EnableAS");
            this.Property(t => t.EnableECP).HasColumnName("EnableECP");
            this.Property(t => t.MaxKeepDeletedItems).HasColumnName("MaxKeepDeletedItems");
            this.Property(t => t.MailboxPlanDesc).HasColumnName("MailboxPlanDesc");
            this.Property(t => t.Price).HasColumnName("Price");
            this.Property(t => t.Cost).HasColumnName("Cost");
            this.Property(t => t.AdditionalGBPrice).HasColumnName("AdditionalGBPrice");
        }
    }
}

namespace CloudPanel.Database.EntityFramework.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ChangeASPolicyBooleanToNotNull : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Plans_ExchangeActiveSync", "AllowNonProvisionableDevices", c => c.Boolean(nullable: false));
            AlterColumn("dbo.Plans_ExchangeActiveSync", "RequirePassword", c => c.Boolean(nullable: false));
            AlterColumn("dbo.Plans_ExchangeActiveSync", "RequireAlphanumericPassword", c => c.Boolean(nullable: false));
            AlterColumn("dbo.Plans_ExchangeActiveSync", "EnablePasswordRecovery", c => c.Boolean(nullable: false));
            AlterColumn("dbo.Plans_ExchangeActiveSync", "RequireEncryptionOnDevice", c => c.Boolean(nullable: false));
            AlterColumn("dbo.Plans_ExchangeActiveSync", "RequireEncryptionOnStorageCard", c => c.Boolean(nullable: false));
            AlterColumn("dbo.Plans_ExchangeActiveSync", "AllowSimplePassword", c => c.Boolean(nullable: false));
            AlterColumn("dbo.Plans_ExchangeActiveSync", "AllowDirectPushWhenRoaming", c => c.Boolean(nullable: false));
            AlterColumn("dbo.Plans_ExchangeActiveSync", "AllowHTMLEmail", c => c.Boolean(nullable: false));
            AlterColumn("dbo.Plans_ExchangeActiveSync", "AllowAttachmentsDownload", c => c.Boolean(nullable: false));
            AlterColumn("dbo.Plans_ExchangeActiveSync", "AllowRemovableStorage", c => c.Boolean(nullable: false));
            AlterColumn("dbo.Plans_ExchangeActiveSync", "AllowCamera", c => c.Boolean(nullable: false));
            AlterColumn("dbo.Plans_ExchangeActiveSync", "AllowWiFi", c => c.Boolean(nullable: false));
            AlterColumn("dbo.Plans_ExchangeActiveSync", "AllowInfrared", c => c.Boolean(nullable: false));
            AlterColumn("dbo.Plans_ExchangeActiveSync", "AllowInternetSharing", c => c.Boolean(nullable: false));
            AlterColumn("dbo.Plans_ExchangeActiveSync", "AllowRemoteDesktop", c => c.Boolean(nullable: false));
            AlterColumn("dbo.Plans_ExchangeActiveSync", "AllowDesktopSync", c => c.Boolean(nullable: false));
            AlterColumn("dbo.Plans_ExchangeActiveSync", "AllowBrowser", c => c.Boolean(nullable: false));
            AlterColumn("dbo.Plans_ExchangeActiveSync", "AllowConsumerMail", c => c.Boolean(nullable: false));
            AlterColumn("dbo.Plans_ExchangeActiveSync", "AllowTextMessaging", c => c.Boolean(nullable: false));
            AlterColumn("dbo.Plans_ExchangeActiveSync", "AllowUnsignedApplications", c => c.Boolean(nullable: false));
            AlterColumn("dbo.Plans_ExchangeActiveSync", "AllowUnsignedInstallationPackages", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Plans_ExchangeActiveSync", "AllowUnsignedInstallationPackages", c => c.Boolean());
            AlterColumn("dbo.Plans_ExchangeActiveSync", "AllowUnsignedApplications", c => c.Boolean());
            AlterColumn("dbo.Plans_ExchangeActiveSync", "AllowTextMessaging", c => c.Boolean());
            AlterColumn("dbo.Plans_ExchangeActiveSync", "AllowConsumerMail", c => c.Boolean());
            AlterColumn("dbo.Plans_ExchangeActiveSync", "AllowBrowser", c => c.Boolean());
            AlterColumn("dbo.Plans_ExchangeActiveSync", "AllowDesktopSync", c => c.Boolean());
            AlterColumn("dbo.Plans_ExchangeActiveSync", "AllowRemoteDesktop", c => c.Boolean());
            AlterColumn("dbo.Plans_ExchangeActiveSync", "AllowInternetSharing", c => c.Boolean());
            AlterColumn("dbo.Plans_ExchangeActiveSync", "AllowInfrared", c => c.Boolean());
            AlterColumn("dbo.Plans_ExchangeActiveSync", "AllowWiFi", c => c.Boolean());
            AlterColumn("dbo.Plans_ExchangeActiveSync", "AllowCamera", c => c.Boolean());
            AlterColumn("dbo.Plans_ExchangeActiveSync", "AllowRemovableStorage", c => c.Boolean());
            AlterColumn("dbo.Plans_ExchangeActiveSync", "AllowAttachmentsDownload", c => c.Boolean());
            AlterColumn("dbo.Plans_ExchangeActiveSync", "AllowHTMLEmail", c => c.Boolean());
            AlterColumn("dbo.Plans_ExchangeActiveSync", "AllowDirectPushWhenRoaming", c => c.Boolean());
            AlterColumn("dbo.Plans_ExchangeActiveSync", "AllowSimplePassword", c => c.Boolean());
            AlterColumn("dbo.Plans_ExchangeActiveSync", "RequireEncryptionOnStorageCard", c => c.Boolean());
            AlterColumn("dbo.Plans_ExchangeActiveSync", "RequireEncryptionOnDevice", c => c.Boolean());
            AlterColumn("dbo.Plans_ExchangeActiveSync", "EnablePasswordRecovery", c => c.Boolean());
            AlterColumn("dbo.Plans_ExchangeActiveSync", "RequireAlphanumericPassword", c => c.Boolean());
            AlterColumn("dbo.Plans_ExchangeActiveSync", "RequirePassword", c => c.Boolean());
            AlterColumn("dbo.Plans_ExchangeActiveSync", "AllowNonProvisionableDevices", c => c.Boolean());
        }
    }
}

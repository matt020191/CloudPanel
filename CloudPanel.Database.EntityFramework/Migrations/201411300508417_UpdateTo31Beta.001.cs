namespace CloudPanel.Database.EntityFramework.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateTo31Beta001 : DbMigration
    {
        public override void Up()
        {
            //First_31_Update
            AddColumn("dbo.Domains", "DomainType", c => c.Int());
            AddColumn("dbo.Users", "IsEnabled", c => c.Boolean());
            DropTable("dbo.ApiAccess");
            DropTable("dbo.Audit");
            DropTable("dbo.AuditLogin");
            DropTable("dbo.CompanyStats");
            DropTable("dbo.Settings");

            //AdditionalColumnsToUserTable
            AddColumn("dbo.Users", "Street", c => c.String());
            AddColumn("dbo.Users", "City", c => c.String());
            AddColumn("dbo.Users", "State", c => c.String());
            AddColumn("dbo.Users", "PostalCode", c => c.String());
            AddColumn("dbo.Users", "Country", c => c.String());
            AddColumn("dbo.Users", "Company", c => c.String());
            AddColumn("dbo.Users", "JobTitle", c => c.String());
            AddColumn("dbo.Users", "TelephoneNumber", c => c.String());
            AddColumn("dbo.Users", "Fax", c => c.String());
            AddColumn("dbo.Users", "HomePhone", c => c.String());
            AddColumn("dbo.Users", "MobilePhone", c => c.String());
            AddColumn("dbo.Users", "Notes", c => c.String());

            //ChangeKeyInTables
            DropPrimaryKey("DistributionGroups");
            AlterColumn("DistributionGroups", "DistinguishedName", c => c.String(maxLength: 255, unicode: false));
            AddPrimaryKey("DistributionGroups", "ID");
            DropPrimaryKey("Contacts");
            AddColumn("dbo.Contacts", "ID", c => c.Int(false, true));
            AddPrimaryKey("Contacts", "ID");

            //AddBrandingsTable
            CreateTable(
                "dbo.Brandings",
                c => new
                {
                    BrandingID = c.Int(nullable: false, identity: true),
                    Name = c.String(nullable: false),
                    HostName = c.String(nullable: false),
                    Email = c.String(nullable: false),
                    LoginLogo = c.String(nullable: false),
                    HeaderLogo = c.String(nullable: false),
                    Theme = c.String(),
                })
                .PrimaryKey(t => t.BrandingID);

            //UpdateActiveSync
            AddColumn("dbo.Plans_ExchangeActiveSync", "MinDevicePasswordComplexCharacters", c => c.Int());

            //UpdateActiveSyncCompanyCodeNull
            AlterColumn("dbo.Plans_ExchangeActiveSync", "CompanyCode", c => c.String(maxLength: 255, unicode: false));

            //ConvertSizeFromKBToBytes
            AddColumn("dbo.SvcMailboxSizes", "TotalItemSize", c => c.String(nullable: false, maxLength: 255));
            AddColumn("dbo.SvcMailboxSizes", "TotalDeletedItemSize", c => c.String(nullable: false, maxLength: 255));
            DropColumn("dbo.SvcMailboxSizes", "TotalItemSizeInKB");
            DropColumn("dbo.SvcMailboxSizes", "TotalDeletedItemSizeInKB");

            //AddBytesColumn
            AddColumn("dbo.SvcMailboxDatabaseSizes", "DatabaseSizeInBytes", c => c.Long(nullable: false));
            AddColumn("dbo.SvcMailboxSizes", "TotalItemSizeInBytes", c => c.Long(nullable: false));
            AddColumn("dbo.SvcMailboxSizes", "TotalDeletedItemSizeInBytes", c => c.Long(nullable: false));
        
            //AddDNToResourceTable
            AddColumn("dbo.ResourceMailboxes", "DistinguishedName", c => c.String());

            //UserPermissionTables
            CreateTable(
                "dbo.UserPermission",
                c => new
                {
                    UserID = c.Int(nullable: false),
                    RoleID = c.Int(nullable: false),
                })
                .PrimaryKey(t => t.UserID);

            CreateTable(
                "dbo.UserRole",
                c => new
                {
                    RoleID = c.Int(nullable: false, identity: true),
                    DisplayName = c.String(),
                    CompanyCode = c.String(),
                    vDomains = c.Boolean(nullable: false),
                    vUsers = c.Boolean(nullable: false),
                    vUsersEdit = c.Boolean(nullable: false),
                    vExchangeContacts = c.Boolean(nullable: false),
                    vExchangeGroups = c.Boolean(nullable: false),
                    vExchangeResources = c.Boolean(nullable: false),
                    vExchangePublicFolders = c.Boolean(nullable: false),
                    vCitrix = c.Boolean(nullable: false),
                    vLync = c.Boolean(nullable: false),
                    eDomains = c.Boolean(nullable: false),
                    eUsers = c.Boolean(nullable: false),
                    eExchangeContacts = c.Boolean(nullable: false),
                    eExchangeGroups = c.Boolean(nullable: false),
                    eExchangeResources = c.Boolean(nullable: false),
                    eExchangePublicFolders = c.Boolean(nullable: false),
                    eCitrix = c.Boolean(nullable: false),
                    eLync = c.Boolean(nullable: false),
                    ePermissions = c.Boolean(nullable: false),
                    dDomains = c.Boolean(nullable: false),
                    dUsers = c.Boolean(nullable: false),
                    dExchangeContacts = c.Boolean(nullable: false),
                    dExchangeGroups = c.Boolean(nullable: false),
                    dExchangeResources = c.Boolean(nullable: false),
                    dExchangePublicFolders = c.Boolean(nullable: false),
                    dCitrix = c.Boolean(nullable: false),
                    dLync = c.Boolean(nullable: false),
                    dPermissions = c.Boolean(nullable: false),
                })
                .PrimaryKey(t => t.RoleID);

            DropTable("dbo.UserPermissions");

            //AddStatisticsRemoveOld
            CreateTable(
                "dbo.Statistics",
                c => new
                {
                    ID = c.Int(nullable: false, identity: true),
                    Retrieved = c.DateTime(nullable: false),
                    UserCount = c.Int(nullable: false),
                    MailboxCount = c.Int(nullable: false),
                    CitrixCount = c.Int(nullable: false),
                    ResellerCode = c.String(),
                    CompanyCode = c.String(),
                })
                .PrimaryKey(t => t.ID);

            DropTable("dbo.Stats_CitrixCount");
            DropTable("dbo.Stats_ExchCount");
            DropTable("dbo.Stats_UserCount");

            //RenameUserRoleTable
            RenameTable("UserRole", "UserRoles");

            //AddRoleIDToUsersTable
            AddColumn("dbo.Users", "RoleID", c => c.Int());

            //AddNewColumnsToRoles
            AddColumn("dbo.UserRoles", "cDomains", c => c.Boolean(nullable: false));
            AddColumn("dbo.UserRoles", "cUsers", c => c.Boolean(nullable: false));
            AddColumn("dbo.UserRoles", "cExchangeContacts", c => c.Boolean(nullable: false));
            AddColumn("dbo.UserRoles", "cExchangeGroups", c => c.Boolean(nullable: false));
            AddColumn("dbo.UserRoles", "cExchangeResources", c => c.Boolean(nullable: false));
            AddColumn("dbo.UserRoles", "cExchangePublicFolders", c => c.Boolean(nullable: false));
            AddColumn("dbo.UserRoles", "cCitrix", c => c.Boolean(nullable: false));
            AddColumn("dbo.UserRoles", "cLync", c => c.Boolean(nullable: false));

            //AdjustBrandingOptions
            AddColumn("dbo.Brandings", "MenuType", c => c.Int(nullable: false));
            AlterColumn("dbo.Brandings", "Theme", c => c.String(nullable: false));

            //AddArchivePlanTableAndColumns
            CreateTable(
                "dbo.Plans_ExchangeArchiving",
                c => new
                {
                    ArchivingID = c.Int(nullable: false, identity: true),
                    DisplayName = c.String(nullable: false, maxLength: 50),
                    Database = c.String(),
                    ResellerCode = c.String(maxLength: 255),
                    CompanyCode = c.String(maxLength: 255),
                    Description = c.String(nullable: false, storeType: "ntext"),
                    Price = c.Decimal(nullable: false),
                    Cost = c.Decimal(nullable: false),
                    ArchiveSizeMB = c.Int(nullable: false),
                })
                .PrimaryKey(t => t.ArchivingID);

            AddColumn("dbo.Users", "ArchivePlan", c => c.Int());

            //ChangeASPolicyBooleanToNotNull
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
        }
    }
}

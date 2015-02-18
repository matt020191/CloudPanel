namespace CloudPanel.Database.EntityFramework.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Upgrade31Beta
    {/*
        public override void Up()
        {
            CreateTable(
                "dbo.AuditTraces",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        TimeStamp = c.DateTime(nullable: false),
                        Username = c.String(),
                        IPAddress = c.String(),
                        Method = c.String(),
                        Route = c.String(),
                        Parameters = c.String(),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.Brandings",
                c => new
                    {
                        BrandingID = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                        Phone = c.String(nullable: false),
                        HostName = c.String(nullable: false),
                        Email = c.String(nullable: false),
                        LoginLogo = c.String(nullable: false),
                        HeaderLogo = c.String(nullable: false),
                        Theme = c.String(nullable: false),
                        MenuType = c.Int(nullable: false)
                    })
                .PrimaryKey(t => t.BrandingID);
            
            CreateTable(
                "dbo.CitrixApplications",
                c => new
                    {
                        ApplicationID = c.Int(nullable: false, identity: true),
                        Uid = c.Int(nullable: false),
                        UUID = c.Guid(nullable: false),
                        Name = c.String(),
                        PublishedName = c.String(),
                        ApplicationName = c.String(),
                        Description = c.String(),
                        SecurityGroup = c.String(),
                        CommandLineExecutable = c.String(),
                        CommandLineArguments = c.String(),
                        ShortcutAddedToDesktop = c.Boolean(nullable: false),
                        ShortcutAddedToStartMenu = c.Boolean(nullable: false),
                        IsEnabled = c.Boolean(nullable: false),
                        UserFilterEnabled = c.Boolean(nullable: false),
                        LastRetrieved = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.ApplicationID);
            
            CreateTable(
                "dbo.CitrixDesktopGroups",
                c => new
                    {
                        DesktopGroupID = c.Int(nullable: false, identity: true),
                        Uid = c.Int(nullable: false),
                        UUID = c.Guid(nullable: false),
                        Name = c.String(nullable: false),
                        PublishedName = c.String(),
                        SecurityGroup = c.String(),
                        Description = c.String(),
                        IsEnabled = c.Boolean(nullable: false),
                        LastRetrieved = c.DateTime(nullable: false),
                        ApplicationId = c.Int(nullable: false),
                        DesktopId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.DesktopGroupID);
            
            CreateTable(
                "dbo.CitrixDesktops",
                c => new
                    {
                        DesktopID = c.Int(nullable: false, identity: true),
                        Uid = c.Int(nullable: false),
                        DesktopGroupID = c.Int(nullable: false),
                        SID = c.String(nullable: false),
                        AgentVersion = c.String(nullable: false),
                        CatalogUid = c.Int(nullable: false),
                        CatalogName = c.String(),
                        DNSName = c.String(),
                        MachineName = c.String(),
                        MachineUid = c.Int(nullable: false),
                        OSType = c.String(),
                        OSVersion = c.String(),
                        IPAddress = c.String(),
                        InMaintenanceMode = c.Boolean(nullable: false),
                        LastRetrieved = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.DesktopID)
                .ForeignKey("dbo.CitrixDesktopGroups", t => t.DesktopGroupID, cascadeDelete: true)
                .Index(t => t.DesktopGroupID);
            
            CreateTable(
                "dbo.UserActiveSyncDevices",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        UserID = c.Int(nullable: false),
                        DeviceGuid = c.Guid(nullable: false),
                        FirstSyncTime = c.DateTime(),
                        LastPolicyUpdateTime = c.DateTime(),
                        LastSyncAttemptTime = c.DateTime(),
                        LastSuccessSync = c.DateTime(),
                        DeviceWipeSentTime = c.DateTime(),
                        DeviceWipeRequestTime = c.DateTime(),
                        DeviceWipeAckTime = c.DateTime(),
                        LastPingHeartbeat = c.Int(nullable: false),
                        Identity = c.String(),
                        DeviceType = c.String(),
                        DeviceID = c.String(),
                        DeviceUserAgent = c.String(),
                        DeviceModel = c.String(),
                        DeviceImei = c.String(),
                        DeviceFriendlyName = c.String(),
                        DeviceOS = c.String(),
                        DevicePhoneNumber = c.String(),
                        Status = c.String(),
                        StatusNote = c.String(),
                        DevicePolicyApplied = c.String(),
                        DevicePolicyApplicationStatus = c.String(),
                        DeviceActiveSyncVersion = c.String(),
                        NumberOfFoldersSynced = c.String(),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.Users", t => t.UserID, cascadeDelete: true)
                .Index(t => t.UserID);
            
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
                        Price = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Cost = c.Decimal(nullable: false, precision: 18, scale: 2),
                        ArchiveSizeMB = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ArchivingID);
            
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
            
            CreateTable(
                "dbo.StatMailboxArchiveSizes",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        UserGuid = c.Guid(nullable: false),
                        UserPrincipalName = c.String(nullable: false, maxLength: 64),
                        MailboxDatabase = c.String(nullable: false, maxLength: 255),
                        TotalItemSize = c.String(nullable: false, maxLength: 255),
                        TotalItemSizeInBytes = c.Long(nullable: false),
                        TotalDeletedItemSize = c.String(nullable: false, maxLength: 255),
                        TotalDeletedItemSizeInBytes = c.Long(nullable: false),
                        ItemCount = c.Int(nullable: false),
                        DeletedItemCount = c.Int(nullable: false),
                        Retrieved = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.StatMailboxSizes",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        UserGuid = c.Guid(nullable: false),
                        UserPrincipalName = c.String(nullable: false, maxLength: 64),
                        MailboxDatabase = c.String(nullable: false, maxLength: 255),
                        TotalItemSize = c.String(nullable: false, maxLength: 255),
                        TotalItemSizeInBytes = c.Long(nullable: false),
                        TotalDeletedItemSize = c.String(nullable: false, maxLength: 255),
                        TotalDeletedItemSizeInBytes = c.Long(nullable: false),
                        ItemCount = c.Int(nullable: false),
                        DeletedItemCount = c.Int(nullable: false),
                        Retrieved = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.ID);

            CreateTable(
                "dbo.UserPermission",
                c => new
                {
                    UserID = c.Int(nullable: false),
                    RoleID = c.Int(nullable: false),
                })
                .PrimaryKey(t => t.UserID);

            CreateTable(
                "dbo.UserRoles",
                c => new
                    {
                        RoleID = c.Int(nullable: false, identity: true),
                        CompanyCode = c.String(),
                        DisplayName = c.String(),
                        cDomains = c.Boolean(nullable: false),
                        cUsers = c.Boolean(nullable: false),
                        cExchangeActiveSyncPlans = c.Boolean(nullable: false),
                        cExchangeContacts = c.Boolean(nullable: false),
                        cExchangeGroups = c.Boolean(nullable: false),
                        cExchangeResources = c.Boolean(nullable: false),
                        cExchangePublicFolders = c.Boolean(nullable: false),
                        cCitrix = c.Boolean(nullable: false),
                        cLync = c.Boolean(nullable: false),
                        vDomains = c.Boolean(nullable: false),
                        vUsers = c.Boolean(nullable: false),
                        vUsersEdit = c.Boolean(nullable: false),
                        vExchangeActiveSyncPlans = c.Boolean(nullable: false),
                        vExchangeContacts = c.Boolean(nullable: false),
                        vExchangeGroups = c.Boolean(nullable: false),
                        vExchangeResources = c.Boolean(nullable: false),
                        vExchangePublicFolders = c.Boolean(nullable: false),
                        vCitrix = c.Boolean(nullable: false),
                        vLync = c.Boolean(nullable: false),
                        eDomains = c.Boolean(nullable: false),
                        eUsers = c.Boolean(nullable: false),
                        eExchangeActiveSyncPlans = c.Boolean(nullable: false),
                        eExchangeContacts = c.Boolean(nullable: false),
                        eExchangeGroups = c.Boolean(nullable: false),
                        eExchangeResources = c.Boolean(nullable: false),
                        eExchangePublicFolders = c.Boolean(nullable: false),
                        eCitrix = c.Boolean(nullable: false),
                        eLync = c.Boolean(nullable: false),
                        ePermissions = c.Boolean(nullable: false),
                        dDomains = c.Boolean(nullable: false),
                        dUsers = c.Boolean(nullable: false),
                        dExchangeActiveSyncPlans = c.Boolean(nullable: false),
                        dExchangeContacts = c.Boolean(nullable: false),
                        dExchangeGroups = c.Boolean(nullable: false),
                        dExchangeResources = c.Boolean(nullable: false),
                        dExchangePublicFolders = c.Boolean(nullable: false),
                        dCitrix = c.Boolean(nullable: false),
                        dLync = c.Boolean(nullable: false),
                        dPermissions = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.RoleID);
            
            CreateTable(
                "dbo.CitrixCompanyToDesktopGroup",
                c => new
                    {
                        CompanyRefDesktopGroupId = c.Int(nullable: false),
                        DesktopGroupRefCompanyId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.CompanyRefDesktopGroupId, t.DesktopGroupRefCompanyId })
                .ForeignKey("dbo.Companies", t => t.CompanyRefDesktopGroupId, cascadeDelete: true)
                .ForeignKey("dbo.CitrixDesktopGroups", t => t.DesktopGroupRefCompanyId, cascadeDelete: true)
                .Index(t => t.CompanyRefDesktopGroupId)
                .Index(t => t.DesktopGroupRefCompanyId);
            
            CreateTable(
                "dbo.CitrixUserToApplications",
                c => new
                    {
                        UserRefApplicationId = c.Int(nullable: false),
                        ApplicationRefUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.UserRefApplicationId, t.ApplicationRefUserId })
                .ForeignKey("dbo.Users", t => t.UserRefApplicationId, cascadeDelete: true)
                .ForeignKey("dbo.CitrixApplications", t => t.ApplicationRefUserId, cascadeDelete: true)
                .Index(t => t.UserRefApplicationId)
                .Index(t => t.ApplicationRefUserId);
            
            CreateTable(
                "dbo.CitrixUserToDesktopGroup",
                c => new
                    {
                        UserRefDesktopGroupId = c.Int(nullable: false),
                        DesktopGroupRefUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.UserRefDesktopGroupId, t.DesktopGroupRefUserId })
                .ForeignKey("dbo.Users", t => t.UserRefDesktopGroupId, cascadeDelete: true)
                .ForeignKey("dbo.CitrixDesktopGroups", t => t.DesktopGroupRefUserId, cascadeDelete: true)
                .Index(t => t.UserRefDesktopGroupId)
                .Index(t => t.DesktopGroupRefUserId);
            
            CreateTable(
                "dbo.CitrixApplicationsToDesktop",
                c => new
                    {
                        ApplicationRefDesktopGroupId = c.Int(nullable: false),
                        DesktopGroupRefApplicationId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.ApplicationRefDesktopGroupId, t.DesktopGroupRefApplicationId })
                .ForeignKey("dbo.CitrixApplications", t => t.ApplicationRefDesktopGroupId, cascadeDelete: true)
                .ForeignKey("dbo.CitrixDesktopGroups", t => t.DesktopGroupRefApplicationId, cascadeDelete: true)
                .Index(t => t.ApplicationRefDesktopGroupId)
                .Index(t => t.DesktopGroupRefApplicationId);

            AddColumn("dbo.Domains", "DomainType", c => c.Int());
            AddColumn("dbo.Users", "IsEnabled", c => c.Boolean());
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
            AddColumn("dbo.Plans_ExchangeActiveSync", "MinDevicePasswordComplexCharacters", c => c.Int());
            AddColumn("dbo.ResourceMailboxes", "DistinguishedName", c => c.String());
            AddColumn("dbo.Users", "RoleID", c => c.Int());
            AddColumn("dbo.Users", "ArchivePlan", c => c.Int());
            AddColumn("dbo.PriceOverride", "ID", c => c.Int(nullable: false, identity: true));
            AddColumn("dbo.Contacts", "ID", c => c.Int(false, true));

            DropPrimaryKey("DistributionGroups");
            DropPrimaryKey("Contacts");
            DropPrimaryKey("Users");

            AlterColumn("dbo.Plans_ExchangeActiveSync", "CompanyCode", c => c.String(maxLength: 255, unicode: false));
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
            AlterColumn("dbo.PriceOverride", "CompanyCode", c => c.String(maxLength: 255));
            AlterColumn("DistributionGroups", "DistinguishedName", c => c.String(maxLength: 255, unicode: false));
            AlterColumn("dbo.Plans_Citrix", "Price", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.Plans_Citrix", "Cost", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.Plans_ExchangeArchiving", "Price", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.Plans_ExchangeArchiving", "Cost", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.Plans_ExchangeMailbox", "Price", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.Plans_ExchangeMailbox", "Cost", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.Plans_ExchangeMailbox", "AdditionalGBPrice", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.PriceOverride", "Price", c => c.Decimal(nullable: false, precision: 18, scale: 2));

            DropTable("dbo.ApiAccess");
            DropTable("dbo.Audit");
            DropTable("dbo.AuditLogin");
            DropTable("dbo.CompanyStats");
            DropTable("dbo.Settings");
            DropTable("dbo.UserPermissions");
            DropTable("dbo.Stats_CitrixCount");
            DropTable("dbo.Stats_ExchCount");
            DropTable("dbo.Stats_UserCount");
            DropTable("dbo.SvcMailboxSizes");

            AddPrimaryKey("PriceOverride", "ID");
            AddPrimaryKey("Contacts", "ID");
            AddPrimaryKey("DistributionGroups", "ID");
            AddPrimaryKey("Users", "ID");

            Sql(@"IF OBJECT_ID ('Trigger_UserStats', 'TR') IS NOT NULL DROP TRIGGER Trigger_UserStats");
        }
        
        public override void Down()
        {
        }*/
    }
}

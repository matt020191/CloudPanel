namespace CloudPanel.Database.EntityFramework.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Original : DbMigration
    {
        public override void Up()
        {
            #region Tables

            CreateTable(
                "dbo.ApiAccess",
                c => new
                    {
                        CustomerKey = c.String(nullable: false, maxLength: 255),
                        CustomerSecret = c.String(nullable: false, maxLength: 255),
                        CompanyCode = c.String(nullable: false, maxLength: 255),
                    })
                .PrimaryKey(t => t.CustomerKey);
            
            CreateTable(
                "dbo.AuditLogin",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        IPAddress = c.String(nullable: false, maxLength: 128),
                        Username = c.String(nullable: false, maxLength: 255),
                        LoginStatus = c.Boolean(nullable: false),
                        AuditTimeStamp = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.Audit",
                c => new
                    {
                        AuditID = c.Int(nullable: false, identity: true),
                        CompanyCode = c.String(nullable: false, maxLength: 255),
                        Username = c.String(nullable: false, maxLength: 50),
                        Date = c.DateTime(nullable: false),
                        ActionID = c.Int(nullable: false),
                        Variable1 = c.String(nullable: false),
                        Variable2 = c.String(),
                    })
                .PrimaryKey(t => t.AuditID);
            
            CreateTable(
                "dbo.Companies",
                c => new
                    {
                        CompanyId = c.Int(nullable: false, identity: true),
                        IsReseller = c.Boolean(nullable: false),
                        ResellerCode = c.String(maxLength: 255),
                        OrgPlanID = c.Int(),
                        CompanyName = c.String(nullable: false, maxLength: 100),
                        CompanyCode = c.String(nullable: false, maxLength: 255),
                        Street = c.String(nullable: false, maxLength: 255),
                        City = c.String(nullable: false, maxLength: 100),
                        State = c.String(nullable: false, maxLength: 100),
                        ZipCode = c.String(nullable: false, maxLength: 50),
                        PhoneNumber = c.String(nullable: false, maxLength: 50),
                        Website = c.String(maxLength: 255),
                        Description = c.String(),
                        AdminName = c.String(nullable: false, maxLength: 100),
                        AdminEmail = c.String(nullable: false, maxLength: 255),
                        DistinguishedName = c.String(nullable: false, maxLength: 255),
                        Created = c.DateTime(nullable: false),
                        ExchEnabled = c.Boolean(nullable: false),
                        LyncEnabled = c.Boolean(),
                        CitrixEnabled = c.Boolean(),
                        ExchPFPlan = c.Int(),
                        Country = c.String(maxLength: 50),
                        ExchPermFixed = c.Boolean(),
                    })
                .PrimaryKey(t => t.CompanyId);
            
            CreateTable(
                "dbo.CompanyStats",
                c => new
                    {
                        CompanyID = c.Int(nullable: false),
                        CompanyCode = c.String(maxLength: 255),
                        UserCount = c.Int(nullable: false),
                        DomainCount = c.Int(nullable: false),
                        SubDomainCount = c.Int(nullable: false),
                        ExchMailboxCount = c.Int(nullable: false),
                        ExchContactsCount = c.Int(nullable: false),
                        ExchDistListsCount = c.Int(nullable: false),
                        ExchPublicFolderCount = c.Int(nullable: false),
                        ExchMailPublicFolderCount = c.Int(nullable: false),
                        ExchKeepDeletedItems = c.Int(nullable: false),
                        RDPUserCount = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.CompanyID);
            
            CreateTable(
                "dbo.Contacts",
                c => new
                    {
                        DistinguishedName = c.String(nullable: false, maxLength: 255),
                        CompanyCode = c.String(maxLength: 255),
                        DisplayName = c.String(nullable: false, maxLength: 255),
                        Email = c.String(nullable: false, maxLength: 255),
                        Hidden = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.DistinguishedName);
            
            CreateTable(
                "dbo.DistributionGroups",
                c => new
                    {
                        DistinguishedName = c.String(nullable: false, maxLength: 255),
                        ID = c.Int(nullable: false, identity: true),
                        CompanyCode = c.String(maxLength: 255),
                        DisplayName = c.String(nullable: false, maxLength: 255),
                        Email = c.String(nullable: false, maxLength: 255),
                        Hidden = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.DistinguishedName);
            
            CreateTable(
                "dbo.Domains",
                c => new
                    {
                        DomainID = c.Int(nullable: false, identity: true),
                        CompanyCode = c.String(maxLength: 255),
                        Domain = c.String(nullable: false, maxLength: 255),
                        IsSubDomain = c.Boolean(),
                        IsDefault = c.Boolean(nullable: false),
                        IsAcceptedDomain = c.Boolean(nullable: false),
                        IsLyncDomain = c.Boolean(),
                        DomainType = c.Int(),
                    })
                .PrimaryKey(t => t.DomainID);
            
            CreateTable(
                "dbo.LogTable",
                c => new
                    {
                        AuditID = c.Int(nullable: false, identity: true),
                        LogTime = c.DateTime(nullable: false),
                        LogType = c.Int(nullable: false),
                        CodeClass = c.String(),
                        UserPrincipalName = c.String(maxLength: 255),
                        CompanyCode = c.String(maxLength: 255),
                        ResellerCode = c.String(maxLength: 255),
                        Message = c.String(nullable: false),
                        RecommendedAction = c.String(),
                        Exception = c.String(),
                    })
                .PrimaryKey(t => t.AuditID);
            
            CreateTable(
                "dbo.Plans_Citrix",
                c => new
                    {
                        CitrixPlanID = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 56),
                        GroupName = c.String(nullable: false, maxLength: 64),
                        Description = c.String(),
                        IsServer = c.Boolean(nullable: false),
                        CompanyCode = c.String(maxLength: 255),
                        Price = c.String(maxLength: 20),
                        Cost = c.String(maxLength: 20),
                        PictureURL = c.String(maxLength: 255),
                    })
                .PrimaryKey(t => t.CitrixPlanID);
            
            CreateTable(
                "dbo.Plans_ExchangeActiveSync",
                c => new
                    {
                        ASID = c.Int(nullable: false, identity: true),
                        CompanyCode = c.String(nullable: false, maxLength: 255),
                        DisplayName = c.String(nullable: false, maxLength: 150),
                        Description = c.String(),
                        ExchangeName = c.String(maxLength: 75),
                        AllowNonProvisionableDevices = c.Boolean(),
                        RefreshIntervalInHours = c.Int(),
                        RequirePassword = c.Boolean(),
                        RequireAlphanumericPassword = c.Boolean(),
                        EnablePasswordRecovery = c.Boolean(),
                        RequireEncryptionOnDevice = c.Boolean(),
                        RequireEncryptionOnStorageCard = c.Boolean(),
                        AllowSimplePassword = c.Boolean(),
                        NumberOfFailedAttempted = c.Int(),
                        MinimumPasswordLength = c.Int(),
                        InactivityTimeoutInMinutes = c.Int(),
                        PasswordExpirationInDays = c.Int(),
                        EnforcePasswordHistory = c.Int(),
                        IncludePastCalendarItems = c.String(maxLength: 20),
                        IncludePastEmailItems = c.String(maxLength: 20),
                        LimitEmailSizeInKB = c.Int(),
                        AllowDirectPushWhenRoaming = c.Boolean(),
                        AllowHTMLEmail = c.Boolean(),
                        AllowAttachmentsDownload = c.Boolean(),
                        MaximumAttachmentSizeInKB = c.Int(),
                        AllowRemovableStorage = c.Boolean(),
                        AllowCamera = c.Boolean(),
                        AllowWiFi = c.Boolean(),
                        AllowInfrared = c.Boolean(),
                        AllowInternetSharing = c.Boolean(),
                        AllowRemoteDesktop = c.Boolean(),
                        AllowDesktopSync = c.Boolean(),
                        AllowBluetooth = c.String(maxLength: 10),
                        AllowBrowser = c.Boolean(),
                        AllowConsumerMail = c.Boolean(),
                        IsEnterpriseCAL = c.Boolean(),
                        AllowTextMessaging = c.Boolean(),
                        AllowUnsignedApplications = c.Boolean(),
                        AllowUnsignedInstallationPackages = c.Boolean(),
                    })
                .PrimaryKey(t => t.ASID);
            
            CreateTable(
                "dbo.Plans_ExchangeMailbox",
                c => new
                    {
                        MailboxPlanID = c.Int(nullable: false, identity: true),
                        MailboxPlanName = c.String(nullable: false, maxLength: 50),
                        ProductID = c.Int(),
                        ResellerCode = c.String(maxLength: 255),
                        CompanyCode = c.String(maxLength: 255),
                        MailboxSizeMB = c.Int(nullable: false),
                        MaxMailboxSizeMB = c.Int(),
                        MaxSendKB = c.Int(nullable: false),
                        MaxReceiveKB = c.Int(nullable: false),
                        MaxRecipients = c.Int(nullable: false),
                        EnablePOP3 = c.Boolean(nullable: false),
                        EnableIMAP = c.Boolean(nullable: false),
                        EnableOWA = c.Boolean(nullable: false),
                        EnableMAPI = c.Boolean(nullable: false),
                        EnableAS = c.Boolean(nullable: false),
                        EnableECP = c.Boolean(nullable: false),
                        MaxKeepDeletedItems = c.Int(nullable: false),
                        MailboxPlanDesc = c.String(),
                        Price = c.String(maxLength: 20),
                        Cost = c.String(maxLength: 20),
                        AdditionalGBPrice = c.String(maxLength: 20),
                    })
                .PrimaryKey(t => t.MailboxPlanID);
            
            CreateTable(
                "dbo.Plans_Organization",
                c => new
                    {
                        OrgPlanID = c.Int(nullable: false, identity: true),
                        OrgPlanName = c.String(nullable: false, maxLength: 50),
                        ProductID = c.Int(nullable: false),
                        ResellerCode = c.String(maxLength: 255),
                        MaxUsers = c.Int(nullable: false),
                        MaxDomains = c.Int(nullable: false),
                        MaxExchangeMailboxes = c.Int(nullable: false),
                        MaxExchangeContacts = c.Int(nullable: false),
                        MaxExchangeDistLists = c.Int(nullable: false),
                        MaxExchangePublicFolders = c.Int(nullable: false),
                        MaxExchangeMailPublicFolders = c.Int(nullable: false),
                        MaxExchangeKeepDeletedItems = c.Int(nullable: false),
                        MaxExchangeActivesyncPolicies = c.Int(),
                        MaxTerminalServerUsers = c.Int(nullable: false),
                        MaxExchangeResourceMailboxes = c.Int(),
                    })
                .PrimaryKey(t => t.OrgPlanID);
            
            CreateTable(
                "dbo.PriceOverride",
                c => new
                    {
                        CompanyCode = c.String(nullable: false, maxLength: 255),
                        Price = c.String(maxLength: 25),
                        PlanID = c.Int(),
                        Product = c.String(maxLength: 25),
                    })
                .PrimaryKey(t => t.CompanyCode);
            
            CreateTable(
                "dbo.Prices",
                c => new
                    {
                        PriceID = c.Int(nullable: false, identity: true),
                        ProductID = c.Int(nullable: false),
                        PlanID = c.Int(nullable: false),
                        CompanyCode = c.String(nullable: false, maxLength: 255),
                        Price = c.Decimal(nullable: false, precision: 18, scale: 2),
                    })
                .PrimaryKey(t => t.PriceID);
            
            CreateTable(
                "dbo.ResourceMailboxes",
                c => new
                    {
                        ResourceID = c.Int(nullable: false, identity: true),
                        DisplayName = c.String(nullable: false, maxLength: 255),
                        CompanyCode = c.String(nullable: false, maxLength: 255),
                        UserPrincipalName = c.String(nullable: false, maxLength: 255),
                        PrimarySmtpAddress = c.String(nullable: false, maxLength: 255),
                        ResourceType = c.String(nullable: false, maxLength: 10),
                        MailboxPlan = c.Int(nullable: false),
                        AdditionalMB = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ResourceID);
            
            CreateTable(
                "dbo.Settings",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        BaseOU = c.String(nullable: false),
                        PrimaryDC = c.String(nullable: false, maxLength: 50),
                        Username = c.String(nullable: false, maxLength: 50),
                        Password = c.String(nullable: false),
                        SuperAdmins = c.String(nullable: false),
                        BillingAdmins = c.String(nullable: false),
                        ExchangeFqdn = c.String(nullable: false, maxLength: 50),
                        ExchangePFServer = c.String(nullable: false, maxLength: 50),
                        ExchangeVersion = c.Int(nullable: false),
                        ExchangeSSLEnabled = c.Boolean(nullable: false),
                        ExchangeConnectionType = c.String(nullable: false, maxLength: 10),
                        PasswordMinLength = c.Int(nullable: false),
                        PasswordComplexityType = c.Int(nullable: false),
                        CitrixEnabled = c.Boolean(nullable: false),
                        PublicFolderEnabled = c.Boolean(nullable: false),
                        LyncEnabled = c.Boolean(nullable: false),
                        WebsiteEnabled = c.Boolean(nullable: false),
                        SQLEnabled = c.Boolean(nullable: false),
                        CurrencySymbol = c.String(maxLength: 10),
                        CurrencyEnglishName = c.String(maxLength: 200),
                        ResellersEnabled = c.Boolean(),
                        CompanysName = c.String(maxLength: 255),
                        AllowCustomNameAttrib = c.Boolean(),
                        ExchStats = c.Boolean(),
                        IPBlockingEnabled = c.Boolean(),
                        IPBlockingFailedCount = c.Int(),
                        IPBlockingLockedMinutes = c.Int(),
                        ExchDatabases = c.String(),
                        UsersOU = c.String(maxLength: 255),
                        BrandingLoginLogo = c.String(maxLength: 255),
                        BrandingCornerLogo = c.String(maxLength: 255),
                        LockdownEnabled = c.Boolean(),
                        LyncFrontEnd = c.String(maxLength: 255),
                        LyncUserPool = c.String(maxLength: 255),
                        LyncMeetingUrl = c.String(maxLength: 255),
                        LyncDialinUrl = c.String(maxLength: 255),
                        CompanysLogo = c.String(maxLength: 255),
                        SupportMailEnabled = c.Boolean(),
                        SupportMailAddress = c.String(maxLength: 255),
                        SupportMailServer = c.String(maxLength: 255),
                        SupportMailPort = c.Int(),
                        SupportMailUsername = c.String(maxLength: 255),
                        SupportMailPassword = c.String(maxLength: 255),
                        SupportMailFrom = c.String(maxLength: 255),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.Stats_CitrixCount",
                c => new
                    {
                        StatDate = c.DateTime(nullable: false),
                        UserCount = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.StatDate);
            
            CreateTable(
                "dbo.Stats_ExchCount",
                c => new
                    {
                        StatDate = c.DateTime(nullable: false),
                        UserCount = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.StatDate);
            
            CreateTable(
                "dbo.Stats_UserCount",
                c => new
                    {
                        StatDate = c.DateTime(nullable: false),
                        UserCount = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.StatDate);
            
            CreateTable(
                "dbo.SvcMailboxDatabaseSizes",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        DatabaseName = c.String(nullable: false, maxLength: 64),
                        Server = c.String(nullable: false, maxLength: 64),
                        DatabaseSize = c.String(nullable: false, maxLength: 255),
                        Retrieved = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.SvcMailboxSizes",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        UserPrincipalName = c.String(nullable: false, maxLength: 64),
                        MailboxDatabase = c.String(nullable: false, maxLength: 255),
                        TotalItemSizeInKB = c.String(nullable: false, maxLength: 255),
                        TotalDeletedItemSizeInKB = c.String(nullable: false, maxLength: 255),
                        ItemCount = c.Int(nullable: false),
                        DeletedItemCount = c.Int(nullable: false),
                        Retrieved = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.SvcQueue",
                c => new
                    {
                        SvcQueueID = c.Int(nullable: false, identity: true),
                        TaskID = c.Int(nullable: false),
                        UserPrincipalName = c.String(maxLength: 255),
                        CompanyCode = c.String(maxLength: 255),
                        TaskOutput = c.String(),
                        TaskCreated = c.DateTime(nullable: false),
                        TaskCompleted = c.DateTime(),
                        TaskDelayInMinutes = c.Int(nullable: false),
                        TaskSuccess = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.SvcQueueID);
            
            CreateTable(
                "dbo.SvcTask",
                c => new
                    {
                        SvcTaskID = c.Int(nullable: false, identity: true),
                        TaskType = c.Int(nullable: false),
                        LastRun = c.DateTime(nullable: false),
                        NextRun = c.DateTime(),
                        TaskOutput = c.String(),
                        TaskDelayInMinutes = c.Int(nullable: false),
                        TaskCreated = c.DateTime(),
                        Reoccurring = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.SvcTaskID);
            
            CreateTable(
                "dbo.UserPermissions",
                c => new
                    {
                        UserID = c.Int(nullable: false),
                        EnableExchange = c.Boolean(nullable: false),
                        DisableExchange = c.Boolean(nullable: false),
                        AddDomain = c.Boolean(nullable: false),
                        DeleteDomain = c.Boolean(nullable: false),
                        EnableAcceptedDomain = c.Boolean(nullable: false),
                        DisableAcceptedDomain = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.UserID);
            
            CreateTable(
                "dbo.UserPlansCitrix",
                c => new
                    {
                        UPCID = c.Int(nullable: false, identity: true),
                        UserID = c.Int(nullable: false),
                        CitrixPlanID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.UPCID);
            
            CreateTable(
                "dbo.Users",
                c => new
                    {
                        UserGuid = c.Guid(nullable: false),
                        ID = c.Int(nullable: false, identity: true),
                        CompanyCode = c.String(maxLength: 255),
                        sAMAccountName = c.String(maxLength: 255),
                        UserPrincipalName = c.String(nullable: false, maxLength: 255),
                        DistinguishedName = c.String(maxLength: 255),
                        DisplayName = c.String(nullable: false, maxLength: 100),
                        Firstname = c.String(maxLength: 50),
                        Middlename = c.String(maxLength: 50),
                        Lastname = c.String(maxLength: 50),
                        Email = c.String(maxLength: 255),
                        Department = c.String(maxLength: 255),
                        IsEnabled = c.Boolean(),
                        IsResellerAdmin = c.Boolean(),
                        IsCompanyAdmin = c.Boolean(),
                        MailboxPlan = c.Int(),
                        TSPlan = c.Int(),
                        LyncPlan = c.Int(),
                        Created = c.DateTime(),
                        AdditionalMB = c.Int(),
                        ActiveSyncPlan = c.Int(),
                        ExchArchivePlan = c.Int(),
                    })
                .PrimaryKey(t => t.UserGuid);

            #endregion

            #region Stored Procedure

            CreateStoredProcedure(
                "dbo.UpdatePriceOverride",
                c => new
                {
                    CompanyCode = c.String(maxLength: 255),
                    CustomPrice = c.String(maxLength: 25),
                    PlanID = c.Int(),
                    Product = c.String(maxLength: 25)
                },
                body: @"IF EXISTS (SELECT Price FROM PriceOverride WHERE CompanyCode=@CompanyCode AND PlanID=@PlanID AND Product=@Product)
		                    UPDATE
			                    PriceOverride
		                    SET
			                    Price=@CustomPrice
		                    WHERE
			                    CompanyCode=@CompanyCode
		                    AND
			                    PlanID=@PlanID
		                    AND
			                    Product=@Product
	                    ELSE
		                    INSERT INTO PriceOverride
			                    (
				                    CompanyCode,
				                    Price,
				                    PlanID,
				                    Product
			                    )
			                    VALUES
			                    (
				                    @CompanyCode,
				                    @CustomPrice,
				                    @PlanID,
				                    @Product
			                    )"
                );

            CreateStoredProcedure(
                "dbo.DisableMailbox",
                c => new
                {
                    UserPrincipalName = c.String(maxLength: 255)
                },
                body: @"/* UPDATE THE USER IFORMATION */
	                    UPDATE Users SET MailboxPlan=0, Email='', ExchArchivePlan=0, ActiveSyncPlan=0, AdditionalMB=0 WHERE UserPrincipalName=@UserPrincipalName;

	                    /* CLEAR THE MAILBOX SIZE TABLE */
	                    DELETE FROM SvcMailboxSizes WHERE UserPrincipalName=@UserPrincipalName;"                
                );

            CreateStoredProcedure(
                "dbo.DeleteUser",
                c => new
                {
                    UserPrincipalName = c.String(maxLength: 255)
                },
                body:
                    @" -- Get User's ID
	                    DECLARE @UsersID AS INT
	                    SET @UsersID = (Select ID FROM Users WHERE UserPrincipalName=@UserPrincipalName)

	                    -- Delete from tables
	                    DELETE FROM Users WHERE ID=@UsersID;
	                    DELETE FROM UserPlansCitrix WHERE UserID=@UsersID;
	                    DELETE FROM SvcMailboxSizes WHERE UserPrincipalName=@UserPrincipalName;
	                    DELETE FROM SvcQueue WHERE UserPrincipalName=@UserPrincipalName;
	                    DELETE FROM UserPermissions WHERE UserID=@UsersID;"
                );

            CreateStoredProcedure(
                "dbo.DisableExchange",
                c => new
                {
                    CompanyCode = c.String(maxLength: 255)
                },
                body:
                    @"/* DELETE CONTACTS */
                    DELETE FROM Contacts WHERE CompanyCode=@CompanyCode;
    
                    /* DELETE DISTRIBUTION GROUPS */
                    DELETE FROM DistributionGroups WHERE CompanyCode=@CompanyCode;
    
                    /* RESET ALL DOMAINS TO NOT ACCEPTED DOMAINS */
                    UPDATE Domains SET IsAcceptedDomain=0, DomainType=3 WHERE CompanyCode=@CompanyCode;
    
                    /* RESET ALL USERS MAILBOX PLAN AND EMAIL ADDRESS */
                    UPDATE Users SET MailboxPlan=0, Email='', ExchArchivePlan=0, ActiveSyncPlan=0, AdditionalMB=0 WHERE CompanyCode=@CompanyCode;
    
                    /* RESET COMPANY TO EXCHANGE DISABLED AND PUBLIC FOLDER DISABLED */
                    UPDATE Companies SET ExchPFPlan=0, ExchEnabled=0 WHERE CompanyCode=@CompanyCode;"
            );

            CreateStoredProcedure(
                "dbo.DeleteCompany",
                c => new
                {
                    CompanyCode = c.String(maxLength: 255)
                },
                body:
                    @"/* RUN THE DISABLE EXCHANGE AGAIN */
                    exec dbo.DisableExchange @CompanyCode;
    
                    /* DELETE CITRIX PLANS ASSIGNED TO USERS */
                    DELETE FROM UserPlansCitrix WHERE UserID IN (SELECT ID FROM Users WHERE CompanyCode=@CompanyCode);
    
                    /* DELETE PRICE OVERRIDE STUFF */
                    DELETE FROM PriceOverride WHERE CompanyCode=@CompanyCode;
    
                    /* DELETE PLANS ASSIGNED TO COMPANIES */
                    DELETE FROM Plans_Citrix WHERE CompanyCode=@CompanyCode;
                    DELETE FROM Plans_ExchangeActiveSync WHERE CompanyCode=@CompanyCode;
                    DELETE FROM Plans_ExchangeMailbox WHERE CompanyCode=@CompanyCode;
    
                    /* DELETE ALL USERS */
                    DELETE FROM Users WHERE CompanyCode=@CompanyCode;
    
                    /* DELETE DOMAINS */
                    DELETE FROM Domains WHERE CompanyCode=@CompanyCode;
    
                    /* REMOVE COMPANY */
                    DELETE FROM Companies WHERE CompanyCode=@CompanyCode AND IsReseller=0;"
            );

            #endregion

            #region Scalar-valued Functions

            // ??

            #endregion
        }
        
        public override void Down()
        {
            DropTable("dbo.Users");
            DropTable("dbo.UserPlansCitrix");
            DropTable("dbo.UserPermissions");
            DropTable("dbo.SvcTask");
            DropTable("dbo.SvcQueue");
            DropTable("dbo.SvcMailboxSizes");
            DropTable("dbo.SvcMailboxDatabaseSizes");
            DropTable("dbo.Stats_UserCount");
            DropTable("dbo.Stats_ExchCount");
            DropTable("dbo.Stats_CitrixCount");
            DropTable("dbo.Settings");
            DropTable("dbo.ResourceMailboxes");
            DropTable("dbo.Prices");
            DropTable("dbo.PriceOverride");
            DropTable("dbo.Plans_Organization");
            DropTable("dbo.Plans_ExchangeMailbox");
            DropTable("dbo.Plans_ExchangeActiveSync");
            DropTable("dbo.Plans_Citrix");
            DropTable("dbo.LogTable");
            DropTable("dbo.Domains");
            DropTable("dbo.DistributionGroups");
            DropTable("dbo.Contacts");
            DropTable("dbo.CompanyStats");
            DropTable("dbo.Companies");
            DropTable("dbo.Audit");
            DropTable("dbo.AuditLogin");
            DropTable("dbo.ApiAccess");

            DropStoredProcedure("dbo.DeleteCompany");
            DropStoredProcedure("dbo.DeleteUser");
            DropStoredProcedure("dbo.DisableExchange");
            DropStoredProcedure("dbo.DisableMailbox");
            DropStoredProcedure("dbo.UpdatePriceOverride");
        }
    }
}

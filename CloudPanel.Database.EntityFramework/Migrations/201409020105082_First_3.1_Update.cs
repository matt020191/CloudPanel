namespace CloudPanel.Database.EntityFramework.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class First_31_Update : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Domains", "DomainType", c => c.Int());
            AddColumn("dbo.Users", "IsEnabled", c => c.Boolean());
            DropTable("dbo.ApiAccess");
            DropTable("dbo.Audit");
            DropTable("dbo.AuditLogin");
            DropTable("dbo.CompanyStats");
            DropTable("dbo.Settings");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.Settings",
                c => new
                    {
                        BaseOU = c.String(nullable: false, storeType: "ntext"),
                        PrimaryDC = c.String(nullable: false, maxLength: 50, unicode: false),
                        Username = c.String(nullable: false, maxLength: 50, unicode: false),
                        Password = c.String(nullable: false, storeType: "ntext"),
                        SuperAdmins = c.String(nullable: false, storeType: "ntext"),
                        BillingAdmins = c.String(nullable: false, storeType: "ntext"),
                        ExchangeFqdn = c.String(nullable: false, maxLength: 50, unicode: false),
                        ExchangePFServer = c.String(nullable: false, maxLength: 50, unicode: false),
                        ExchangeVersion = c.Int(nullable: false),
                        ExchangeSSLEnabled = c.Boolean(nullable: false),
                        ExchangeConnectionType = c.String(nullable: false, maxLength: 10, unicode: false),
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
                        CompanysName = c.String(maxLength: 255, unicode: false),
                        CompanysLogo = c.String(maxLength: 255, unicode: false),
                        AllowCustomNameAttrib = c.Boolean(),
                        ExchStats = c.Boolean(),
                        IPBlockingEnabled = c.Boolean(),
                        IPBlockingFailedCount = c.Int(),
                        IPBlockingLockedMinutes = c.Int(),
                        ExchDatabases = c.String(storeType: "ntext"),
                        UsersOU = c.String(maxLength: 255, unicode: false),
                        BrandingLoginLogo = c.String(maxLength: 255, unicode: false),
                        BrandingCornerLogo = c.String(maxLength: 255, unicode: false),
                        LockdownEnabled = c.Boolean(),
                        LyncFrontEnd = c.String(maxLength: 255, unicode: false),
                        LyncUserPool = c.String(maxLength: 255, unicode: false),
                        LyncMeetingUrl = c.String(maxLength: 255, unicode: false),
                        LyncDialinUrl = c.String(maxLength: 255, unicode: false),
                        SupportMailEnabled = c.Boolean(),
                        SupportMailAddress = c.String(maxLength: 255, unicode: false),
                        SupportMailServer = c.String(maxLength: 255, unicode: false),
                        SupportMailPort = c.Int(),
                        SupportMailUsername = c.String(maxLength: 255, unicode: false),
                        SupportMailPassword = c.String(maxLength: 255, unicode: false),
                        SupportMailFrom = c.String(maxLength: 255, unicode: false),
                    })
                .PrimaryKey(t => new { t.BaseOU, t.PrimaryDC, t.Username, t.Password, t.SuperAdmins, t.BillingAdmins, t.ExchangeFqdn, t.ExchangePFServer, t.ExchangeVersion, t.ExchangeSSLEnabled, t.ExchangeConnectionType, t.PasswordMinLength, t.PasswordComplexityType, t.CitrixEnabled, t.PublicFolderEnabled, t.LyncEnabled, t.WebsiteEnabled, t.SQLEnabled });
            
            CreateTable(
                "dbo.CompanyStats",
                c => new
                    {
                        CompanyID = c.Int(nullable: false),
                        CompanyCode = c.String(maxLength: 255, unicode: false),
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
                "dbo.AuditLogin",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        IPAddress = c.String(nullable: false, maxLength: 128, unicode: false),
                        Username = c.String(nullable: false, maxLength: 255, unicode: false),
                        LoginStatus = c.Boolean(nullable: false),
                        AuditTimeStamp = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.Audit",
                c => new
                    {
                        AuditID = c.Int(nullable: false, identity: true),
                        Username = c.String(nullable: false, maxLength: 50),
                        Date = c.DateTime(nullable: false),
                        SeverityID = c.Int(),
                        MethodName = c.String(maxLength: 100, unicode: false),
                        Parameters = c.String(storeType: "ntext"),
                        Message = c.String(nullable: false, storeType: "ntext"),
                    })
                .PrimaryKey(t => t.AuditID);
            
            CreateTable(
                "dbo.ApiAccess",
                c => new
                    {
                        CustomerKey = c.String(nullable: false, maxLength: 255, unicode: false),
                        CustomerSecret = c.String(nullable: false, maxLength: 255, unicode: false),
                        CompanyCode = c.String(nullable: false, maxLength: 255, unicode: false),
                    })
                .PrimaryKey(t => t.CustomerKey);
            
            DropColumn("dbo.Users", "IsEnabled");
            DropColumn("dbo.Domains", "DomainType");
        }
    }
}

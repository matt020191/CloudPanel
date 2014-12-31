namespace CloudPanel.Database.EntityFramework.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddNewCitrixTables : DbMigration
    {
        public override void Up()
        {
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
                        ShortcutAddedToDesktop = c.Boolean(nullable: false),
                        ShortcutAddedToStartMenu = c.Boolean(nullable: false),
                        IsEnabled = c.Boolean(nullable: false),
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
                        UserRefApplicationId = c.Guid(nullable: false),
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
                        UserRefDesktopGroupId = c.Guid(nullable: false),
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
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.CitrixApplicationsToDesktop", "DesktopGroupRefApplicationId", "dbo.CitrixDesktopGroups");
            DropForeignKey("dbo.CitrixApplicationsToDesktop", "ApplicationRefDesktopGroupId", "dbo.CitrixApplications");
            DropForeignKey("dbo.CitrixUserToDesktopGroup", "DesktopGroupRefUserId", "dbo.CitrixDesktopGroups");
            DropForeignKey("dbo.CitrixUserToDesktopGroup", "UserRefDesktopGroupId", "dbo.Users");
            DropForeignKey("dbo.CitrixUserToApplications", "ApplicationRefUserId", "dbo.CitrixApplications");
            DropForeignKey("dbo.CitrixUserToApplications", "UserRefApplicationId", "dbo.Users");
            DropForeignKey("dbo.CitrixDesktops", "DesktopGroupID", "dbo.CitrixDesktopGroups");
            DropForeignKey("dbo.CitrixCompanyToDesktopGroup", "DesktopGroupRefCompanyId", "dbo.CitrixDesktopGroups");
            DropForeignKey("dbo.CitrixCompanyToDesktopGroup", "CompanyRefDesktopGroupId", "dbo.Companies");
            DropIndex("dbo.CitrixApplicationsToDesktop", new[] { "DesktopGroupRefApplicationId" });
            DropIndex("dbo.CitrixApplicationsToDesktop", new[] { "ApplicationRefDesktopGroupId" });
            DropIndex("dbo.CitrixUserToDesktopGroup", new[] { "DesktopGroupRefUserId" });
            DropIndex("dbo.CitrixUserToDesktopGroup", new[] { "UserRefDesktopGroupId" });
            DropIndex("dbo.CitrixUserToApplications", new[] { "ApplicationRefUserId" });
            DropIndex("dbo.CitrixUserToApplications", new[] { "UserRefApplicationId" });
            DropIndex("dbo.CitrixCompanyToDesktopGroup", new[] { "DesktopGroupRefCompanyId" });
            DropIndex("dbo.CitrixCompanyToDesktopGroup", new[] { "CompanyRefDesktopGroupId" });
            DropIndex("dbo.CitrixDesktops", new[] { "DesktopGroupID" });
            DropTable("dbo.CitrixApplicationsToDesktop");
            DropTable("dbo.CitrixUserToDesktopGroup");
            DropTable("dbo.CitrixUserToApplications");
            DropTable("dbo.CitrixCompanyToDesktopGroup");
            DropTable("dbo.CitrixDesktops");
            DropTable("dbo.CitrixDesktopGroups");
            DropTable("dbo.CitrixApplications");
        }
    }
}

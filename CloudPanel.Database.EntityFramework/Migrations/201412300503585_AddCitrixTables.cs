namespace CloudPanel.Database.EntityFramework.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddCitrixTables : DbMigration
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
                "dbo.UsersCitrixApplications",
                c => new
                    {
                        Users_UserGuid = c.Guid(nullable: false),
                        CitrixApplications_ApplicationID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.Users_UserGuid, t.CitrixApplications_ApplicationID })
                .ForeignKey("dbo.Users", t => t.Users_UserGuid, cascadeDelete: true)
                .ForeignKey("dbo.CitrixApplications", t => t.CitrixApplications_ApplicationID, cascadeDelete: true)
                .Index(t => t.Users_UserGuid)
                .Index(t => t.CitrixApplications_ApplicationID);
            
            CreateTable(
                "dbo.UsersCitrixDesktopGroups",
                c => new
                    {
                        Users_UserGuid = c.Guid(nullable: false),
                        CitrixDesktopGroups_DesktopGroupID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.Users_UserGuid, t.CitrixDesktopGroups_DesktopGroupID })
                .ForeignKey("dbo.Users", t => t.Users_UserGuid, cascadeDelete: true)
                .ForeignKey("dbo.CitrixDesktopGroups", t => t.CitrixDesktopGroups_DesktopGroupID, cascadeDelete: true)
                .Index(t => t.Users_UserGuid)
                .Index(t => t.CitrixDesktopGroups_DesktopGroupID);
            
            CreateTable(
                "dbo.AppDesktop",
                c => new
                    {
                        ApplicationRefId = c.Int(nullable: false),
                        DesktopGroupRefId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.ApplicationRefId, t.DesktopGroupRefId })
                .ForeignKey("dbo.CitrixApplications", t => t.ApplicationRefId, cascadeDelete: true)
                .ForeignKey("dbo.CitrixDesktopGroups", t => t.DesktopGroupRefId, cascadeDelete: true)
                .Index(t => t.ApplicationRefId)
                .Index(t => t.DesktopGroupRefId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AppDesktop", "DesktopGroupRefId", "dbo.CitrixDesktopGroups");
            DropForeignKey("dbo.AppDesktop", "ApplicationRefId", "dbo.CitrixApplications");
            DropForeignKey("dbo.UsersCitrixDesktopGroups", "CitrixDesktopGroups_DesktopGroupID", "dbo.CitrixDesktopGroups");
            DropForeignKey("dbo.UsersCitrixDesktopGroups", "Users_UserGuid", "dbo.Users");
            DropForeignKey("dbo.UsersCitrixApplications", "CitrixApplications_ApplicationID", "dbo.CitrixApplications");
            DropForeignKey("dbo.UsersCitrixApplications", "Users_UserGuid", "dbo.Users");
            DropForeignKey("dbo.CitrixDesktops", "DesktopGroupID", "dbo.CitrixDesktopGroups");
            DropIndex("dbo.AppDesktop", new[] { "DesktopGroupRefId" });
            DropIndex("dbo.AppDesktop", new[] { "ApplicationRefId" });
            DropIndex("dbo.UsersCitrixDesktopGroups", new[] { "CitrixDesktopGroups_DesktopGroupID" });
            DropIndex("dbo.UsersCitrixDesktopGroups", new[] { "Users_UserGuid" });
            DropIndex("dbo.UsersCitrixApplications", new[] { "CitrixApplications_ApplicationID" });
            DropIndex("dbo.UsersCitrixApplications", new[] { "Users_UserGuid" });
            DropIndex("dbo.CitrixDesktops", new[] { "DesktopGroupID" });
            DropTable("dbo.AppDesktop");
            DropTable("dbo.UsersCitrixDesktopGroups");
            DropTable("dbo.UsersCitrixApplications");
            DropTable("dbo.CitrixDesktops");
            DropTable("dbo.CitrixDesktopGroups");
            DropTable("dbo.CitrixApplications");
        }
    }
}

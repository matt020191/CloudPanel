namespace CloudPanel.Database.EntityFramework.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddCitrixSecurityGroupsTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.CitrixSecurityGroups",
                c => new
                    {
                        GroupID = c.Int(nullable: false, identity: true),
                        GroupName = c.String(nullable: false),
                        Description = c.String(),
                        CompanyCode = c.String(nullable: false),
                        DesktopGroupID = c.Int(nullable: false),
                        ApplicationID = c.Int(),
                    })
                .PrimaryKey(t => t.GroupID)
                .ForeignKey("dbo.CitrixDesktopGroups", t => t.DesktopGroupID, cascadeDelete: true)
                .Index(t => t.DesktopGroupID);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.CitrixSecurityGroups", "DesktopGroupID", "dbo.CitrixDesktopGroups");
            DropIndex("dbo.CitrixSecurityGroups", new[] { "DesktopGroupID" });
            DropTable("dbo.CitrixSecurityGroups");
        }
    }
}

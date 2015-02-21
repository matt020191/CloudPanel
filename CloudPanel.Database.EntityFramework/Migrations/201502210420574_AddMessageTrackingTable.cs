namespace CloudPanel.Database.EntityFramework.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddMessageTrackingTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.StatMessageTrackingCounts",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        UserID = c.Int(nullable: false),
                        Date = c.DateTime(nullable: false),
                        TotalSent = c.Int(nullable: false),
                        TotalReceived = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.Users", t => t.UserID, cascadeDelete: true)
                .Index(t => t.UserID);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.StatMessageTrackingCounts", "UserID", "dbo.Users");
            DropIndex("dbo.StatMessageTrackingCounts", new[] { "UserID" });
            DropTable("dbo.StatMessageTrackingCounts");
        }
    }
}

namespace CloudPanel.Database.EntityFramework.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateMessageTrackingTable : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.StatMessageTrackingCounts", "Start", c => c.DateTime(nullable: false));
            AddColumn("dbo.StatMessageTrackingCounts", "End", c => c.DateTime(nullable: false));
            AddColumn("dbo.StatMessageTrackingCounts", "TotalBytesSent", c => c.Long(nullable: false));
            AddColumn("dbo.StatMessageTrackingCounts", "TotalBytesReceived", c => c.Long(nullable: false));
            DropColumn("dbo.StatMessageTrackingCounts", "Date");
        }
        
        public override void Down()
        {
            AddColumn("dbo.StatMessageTrackingCounts", "Date", c => c.DateTime(nullable: false));
            DropColumn("dbo.StatMessageTrackingCounts", "TotalBytesReceived");
            DropColumn("dbo.StatMessageTrackingCounts", "TotalBytesSent");
            DropColumn("dbo.StatMessageTrackingCounts", "End");
            DropColumn("dbo.StatMessageTrackingCounts", "Start");
        }
    }
}

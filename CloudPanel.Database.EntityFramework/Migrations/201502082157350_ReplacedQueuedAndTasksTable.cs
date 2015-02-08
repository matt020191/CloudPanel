namespace CloudPanel.Database.EntityFramework.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ReplacedQueuedAndTasksTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.DelayedUserTasks",
                c => new
                    {
                        TaskID = c.Int(nullable: false, identity: true),
                        UserID = c.Int(nullable: false),
                        Status = c.Int(nullable: false),
                        LastMessage = c.String(storeType: "ntext"),
                        Created = c.DateTime(nullable: false),
                        DelayedUntil = c.DateTime(nullable: false),
                        LastUpdated = c.DateTime(),
                    })
                .PrimaryKey(t => t.TaskID)
                .ForeignKey("dbo.Users", t => t.UserID, cascadeDelete: true)
                .Index(t => t.UserID);
            
            DropTable("dbo.SvcQueue");
            DropTable("dbo.SvcTask");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.SvcTask",
                c => new
                    {
                        SvcTaskID = c.Int(nullable: false, identity: true),
                        TaskType = c.Int(nullable: false),
                        LastRun = c.DateTime(nullable: false),
                        NextRun = c.DateTime(),
                        TaskOutput = c.String(storeType: "ntext"),
                        TaskDelayInMinutes = c.Int(nullable: false),
                        TaskCreated = c.DateTime(),
                        Reoccurring = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.SvcTaskID);
            
            CreateTable(
                "dbo.SvcQueue",
                c => new
                    {
                        SvcQueueID = c.Int(nullable: false, identity: true),
                        TaskID = c.Int(nullable: false),
                        UserPrincipalName = c.String(maxLength: 255),
                        CompanyCode = c.String(maxLength: 255),
                        TaskOutput = c.String(storeType: "ntext"),
                        TaskCreated = c.DateTime(nullable: false),
                        TaskCompleted = c.DateTime(),
                        TaskDelayInMinutes = c.Int(nullable: false),
                        TaskSuccess = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.SvcQueueID);
            
            DropForeignKey("dbo.DelayedUserTasks", "UserID", "dbo.Users");
            DropIndex("dbo.DelayedUserTasks", new[] { "UserID" });
            DropTable("dbo.DelayedUserTasks");
        }
    }
}

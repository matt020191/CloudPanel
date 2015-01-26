namespace CloudPanel.Database.EntityFramework.Migrations
{
    using log4net;
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddApiKeyTable : DbMigration
    {
        private static readonly ILog logger = log4net.LogManager.GetLogger("SQL");

        public override void Up()
        {
            logger.InfoFormat("Creating table ApiKeys");
            CreateTable(
                "dbo.ApiKeys",
                c => new
                    {
                        UserID = c.Int(nullable: false),
                        Key = c.String(),
                    })
                .PrimaryKey(t => t.UserID)
                .ForeignKey("dbo.Users", t => t.UserID)
                .Index(t => t.UserID);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ApiKeys", "UserID", "dbo.Users");
            DropIndex("dbo.ApiKeys", new[] { "UserID" });
            DropTable("dbo.ApiKeys");
        }
    }
}

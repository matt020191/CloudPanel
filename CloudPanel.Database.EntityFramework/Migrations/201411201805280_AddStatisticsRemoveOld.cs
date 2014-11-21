namespace CloudPanel.Database.EntityFramework.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddStatisticsRemoveOld : DbMigration
    {
        public override void Up()
        {
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
            
            DropTable("dbo.Stats_CitrixCount");
            DropTable("dbo.Stats_ExchCount");
            DropTable("dbo.Stats_UserCount");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.Stats_UserCount",
                c => new
                    {
                        StatDate = c.DateTime(nullable: false, storeType: "date"),
                        UserCount = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.StatDate);
            
            CreateTable(
                "dbo.Stats_ExchCount",
                c => new
                    {
                        StatDate = c.DateTime(nullable: false, storeType: "date"),
                        UserCount = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.StatDate);
            
            CreateTable(
                "dbo.Stats_CitrixCount",
                c => new
                    {
                        StatDate = c.DateTime(nullable: false, storeType: "date"),
                        UserCount = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.StatDate);
            
            DropTable("dbo.Statistics");
        }
    }
}

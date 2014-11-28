namespace CloudPanel.Database.EntityFramework.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddArchivePlanTableAndColumnes : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Plans_ExchangeArchiving",
                c => new
                    {
                        ArchivingID = c.Int(nullable: false, identity: true),
                        DisplayName = c.String(nullable: false, maxLength: 50),
                        Database = c.String(),
                        ResellerCode = c.String(maxLength: 255),
                        CompanyCode = c.String(maxLength: 255),
                        Description = c.String(nullable: false, storeType: "ntext"),
                        Price = c.String(nullable: false, maxLength: 20),
                        Cost = c.String(nullable: false, maxLength: 20),
                        ArchiveSizeMB = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ArchivingID);
            
            AddColumn("dbo.Users", "ArchivePlan", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Users", "ArchivePlan");
            DropTable("dbo.Plans_ExchangeArchiving");
        }
    }
}

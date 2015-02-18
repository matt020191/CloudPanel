namespace CloudPanel.Database.EntityFramework.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddPublicFolderTables
    {/*
        public override void Up()
        {
            CreateTable(
                "dbo.PublicFolderMailboxes",
                c => new
                    {
                        MailboxID = c.Int(nullable: false, identity: true),
                        CompanyID = c.Int(nullable: false),
                        PlanID = c.Int(nullable: false),
                        Identity = c.String(),
                    })
                .PrimaryKey(t => t.MailboxID)
                .ForeignKey("dbo.Companies", t => t.CompanyID, cascadeDelete: true)
                .ForeignKey("dbo.Plans_ExchangePublicFolders", t => t.PlanID, cascadeDelete: true)
                .Index(t => t.CompanyID)
                .Index(t => t.PlanID);
            
            CreateTable(
                "dbo.Plans_ExchangePublicFolders",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                        MailboxSizeMB = c.Int(nullable: false),
                        CompanyCode = c.String(),
                        Description = c.String(storeType: "ntext"),
                        Price = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Cost = c.Decimal(nullable: false, precision: 18, scale: 2),
                    })
                .PrimaryKey(t => t.ID);
            
            DropTable("dbo.Plans_TerminalServices");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.Plans_TerminalServices",
                c => new
                    {
                        TSPlanID = c.Int(nullable: false),
                        TSPlanName = c.String(nullable: false, maxLength: 50),
                        ResellerCode = c.String(nullable: false, maxLength: 255),
                        ProductID = c.Int(nullable: false),
                        MaxUserSpaceMB = c.Int(),
                    })
                .PrimaryKey(t => new { t.TSPlanID, t.TSPlanName, t.ResellerCode, t.ProductID });
            
            DropForeignKey("dbo.PublicFolderMailboxes", "PlanID", "dbo.Plans_ExchangePublicFolders");
            DropForeignKey("dbo.PublicFolderMailboxes", "CompanyID", "dbo.Companies");
            DropIndex("dbo.PublicFolderMailboxes", new[] { "PlanID" });
            DropIndex("dbo.PublicFolderMailboxes", new[] { "CompanyID" });
            DropTable("dbo.Plans_ExchangePublicFolders");
            DropTable("dbo.PublicFolderMailboxes");
        }*/
    }
}

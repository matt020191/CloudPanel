namespace CloudPanel.Database.EntityFramework.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddIdentityCustomPrices : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PriceOverride", "ID", c => c.Int(nullable: false, identity: true));
            AlterColumn("dbo.PriceOverride", "CompanyCode", c => c.String(maxLength: 255));
            AddPrimaryKey("PriceOverride", "ID");
        }
        
        public override void Down()
        {
            AlterColumn("dbo.PriceOverride", "CompanyCode", c => c.String(nullable: false, maxLength: 255));
            DropColumn("dbo.PriceOverride", "ID");
        }
    }
}

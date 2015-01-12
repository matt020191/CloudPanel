namespace CloudPanel.Database.EntityFramework.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ChangeMoneyStringToDecimal : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Plans_Citrix", "Price", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.Plans_Citrix", "Cost", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.Plans_ExchangeArchiving", "Price", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.Plans_ExchangeArchiving", "Cost", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.Plans_ExchangeMailbox", "Price", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.Plans_ExchangeMailbox", "Cost", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.Plans_ExchangeMailbox", "AdditionalGBPrice", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.PriceOverride", "Price", c => c.Decimal(nullable: false, precision: 18, scale: 2));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.PriceOverride", "Price", c => c.String(maxLength: 25));
            AlterColumn("dbo.Plans_ExchangeMailbox", "AdditionalGBPrice", c => c.String(maxLength: 20));
            AlterColumn("dbo.Plans_ExchangeMailbox", "Cost", c => c.String(maxLength: 20));
            AlterColumn("dbo.Plans_ExchangeMailbox", "Price", c => c.String(maxLength: 20));
            AlterColumn("dbo.Plans_ExchangeArchiving", "Cost", c => c.String(nullable: false, maxLength: 20));
            AlterColumn("dbo.Plans_ExchangeArchiving", "Price", c => c.String(nullable: false, maxLength: 20));
            AlterColumn("dbo.Plans_Citrix", "Cost", c => c.String(maxLength: 20));
            AlterColumn("dbo.Plans_Citrix", "Price", c => c.String(maxLength: 20));
        }
    }
}

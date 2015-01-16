namespace CloudPanel.Database.EntityFramework.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddMissingColumns : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Plans_Organization", "MaxExchangeActivesyncPolicies", c => c.Int(nullable: true));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Plans_Organization", "MaxExchangeActivesyncPolicies");
        }
    }
}

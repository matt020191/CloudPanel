namespace CloudPanel.Database.EntityFramework.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateActiveSync : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Plans_ExchangeActiveSync", "MinDevicePasswordComplexCharacters", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Plans_ExchangeActiveSync", "MinDevicePasswordComplexCharacters");
        }
    }
}

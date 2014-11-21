namespace CloudPanel.Database.EntityFramework.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddStatisticsRemoveOld2 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Statistics", "Retrieved", c => c.DateTime(nullable: false));
            AddColumn("dbo.Statistics", "ResellerCode", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Statistics", "ResellerCode");
            DropColumn("dbo.Statistics", "Retrieved");
        }
    }
}

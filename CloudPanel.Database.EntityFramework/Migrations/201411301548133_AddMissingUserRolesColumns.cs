namespace CloudPanel.Database.EntityFramework.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddMissingUserRolesColumns : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.UserRoles", "cExchangeActiveSyncPlans", c => c.Boolean(nullable: false));
            AddColumn("dbo.UserRoles", "vExchangeActiveSyncPlans", c => c.Boolean(nullable: false));
            AddColumn("dbo.UserRoles", "eExchangeActiveSyncPlans", c => c.Boolean(nullable: false));
            AddColumn("dbo.UserRoles", "dExchangeActiveSyncPlans", c => c.Boolean(nullable: false));

            AddColumn("dbo.Brandings", "Phone", c => c.String(nullable: false));
        }
        
        public override void Down()
        {
        }
    }
}

namespace CloudPanel.Database.EntityFramework.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddSecurityGroupToCitrixDesktopGroups : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.CitrixDesktopGroups", "SecurityGroup", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.CitrixDesktopGroups", "SecurityGroup");
        }
    }
}

namespace CloudPanel.Database.EntityFramework.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddCitrixApplicationColumns : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.CitrixApplications", "SecurityGroup", c => c.String());
            AddColumn("dbo.CitrixApplications", "UserFilterEnabled", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.CitrixApplications", "UserFilterEnabled");
            DropColumn("dbo.CitrixApplications", "SecurityGroup");
        }
    }
}

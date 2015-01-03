namespace CloudPanel.Database.EntityFramework.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddNewColumnsToUserTables : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Users", "Skype", c => c.String());
            AddColumn("dbo.Users", "Facebook", c => c.String());
            AddColumn("dbo.Users", "Twitter", c => c.String());
            AddColumn("dbo.Users", "Dribbble", c => c.String());
            AddColumn("dbo.Users", "Tumblr", c => c.String());
            AddColumn("dbo.Users", "LinkedIn", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Users", "LinkedIn");
            DropColumn("dbo.Users", "Facebook");
            DropColumn("dbo.Users", "Twitter");
            DropColumn("dbo.Users", "Dribbble");
            DropColumn("dbo.Users", "Tumblr");
            DropColumn("dbo.Users", "LinkedIn");
        }
    }
}

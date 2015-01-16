namespace CloudPanel.Database.EntityFramework.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddMissingUserColumns : DbMigration
    {
        public override void Up()
        {
            AddColumn("Users", "Skype", c => c.String());
            AddColumn("Users", "Facebook", c => c.String());
            AddColumn("Users", "Twitter", c => c.String());
            AddColumn("Users", "Dribbble", c => c.String());
            AddColumn("Users", "Tumblr", c => c.String());
            AddColumn("Users", "LinkedIn", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("Users", "LinkedIn");
            DropColumn("Users", "Tumblr");
            DropColumn("Users", "Dribbble");
            DropColumn("Users", "Twitter");
            DropColumn("Users", "Facebook");
            DropColumn("Users", "Skype");
        }
    }
}

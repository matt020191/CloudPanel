namespace CloudPanel.Database.EntityFramework.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpgradeTo31 : DbMigration
    {
        public override void Up()
        {
            AddColumn("Contacts", "ID", c => c.Int(identity: true));
        }
        
        public override void Down()
        {
            DropColumn("Contacts", "ID");
        }
    }
}

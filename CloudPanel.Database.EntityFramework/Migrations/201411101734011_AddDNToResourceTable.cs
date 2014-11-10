namespace CloudPanel.Database.EntityFramework.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddDNToResourceTable : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ResourceMailboxes", "DistinguishedName", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.ResourceMailboxes", "DistinguishedName");
        }
    }
}

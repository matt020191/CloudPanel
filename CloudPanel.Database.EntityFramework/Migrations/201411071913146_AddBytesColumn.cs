namespace CloudPanel.Database.EntityFramework.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddBytesColumn : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.SvcMailboxDatabaseSizes", "DatabaseSizeInBytes", c => c.Long(nullable: false));
            AddColumn("dbo.SvcMailboxSizes", "TotalItemSizeInBytes", c => c.Long(nullable: false));
            AddColumn("dbo.SvcMailboxSizes", "TotalDeletedItemSizeInBytes", c => c.Long(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.SvcMailboxSizes", "TotalDeletedItemSizeInBytes");
            DropColumn("dbo.SvcMailboxSizes", "TotalItemSizeInBytes");
            DropColumn("dbo.SvcMailboxDatabaseSizes", "DatabaseSizeInBytes");
        }
    }
}

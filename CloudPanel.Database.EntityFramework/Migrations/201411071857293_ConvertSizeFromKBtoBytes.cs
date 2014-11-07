namespace CloudPanel.Database.EntityFramework.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ConvertSizeFromKBtoBytes : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.SvcMailboxSizes", "TotalItemSize", c => c.String(nullable: false, maxLength: 255));
            AddColumn("dbo.SvcMailboxSizes", "TotalDeletedItemSize", c => c.String(nullable: false, maxLength: 255));
            DropColumn("dbo.SvcMailboxSizes", "TotalItemSizeInKB");
            DropColumn("dbo.SvcMailboxSizes", "TotalDeletedItemSizeInKB");
        }
        
        public override void Down()
        {
            AddColumn("dbo.SvcMailboxSizes", "TotalDeletedItemSizeInKB", c => c.String(nullable: false, maxLength: 255, unicode: false));
            AddColumn("dbo.SvcMailboxSizes", "TotalItemSizeInKB", c => c.String(nullable: false, maxLength: 255, unicode: false));
            DropColumn("dbo.SvcMailboxSizes", "TotalDeletedItemSize");
            DropColumn("dbo.SvcMailboxSizes", "TotalItemSize");
        }
    }
}

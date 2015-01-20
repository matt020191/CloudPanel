namespace CloudPanel.Database.EntityFramework.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateMailboxDatabaseTable : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "dbo.SvcMailboxDatabaseSizes", newName: "StatMailboxDatabaseSizes");
            AddColumn("dbo.StatMailboxDatabaseSizes", "DatabaseSizeInBytes", c => c.Long(nullable: false));
            AlterColumn("dbo.StatMailboxDatabaseSizes", "Retrieved", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.StatMailboxDatabaseSizes", "DatabaseSizeInBytes");
            RenameTable(name: "dbo.StatMailboxDatabaseSizes", newName: "SvcMailboxDatabaseSizes");
        }
    }
}

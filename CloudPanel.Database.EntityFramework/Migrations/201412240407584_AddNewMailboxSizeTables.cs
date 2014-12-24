namespace CloudPanel.Database.EntityFramework.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddNewMailboxSizeTables : DbMigration
    {
        public override void Up()
        {
            DropTable("dbo.SvcMailboxSizes");
            CreateTable(
                "dbo.StatMailboxArchiveSizes",
                c => new
                {
                    ID = c.Int(nullable: false, identity: true),
                    UserGuid = c.Guid(nullable: false),
                    UserPrincipalName = c.String(nullable: false, maxLength: 64),
                    MailboxDatabase = c.String(nullable: false, maxLength: 255),
                    TotalItemSize = c.String(nullable: false, maxLength: 255),
                    TotalItemSizeInBytes = c.Long(nullable: false),
                    TotalDeletedItemSize = c.String(nullable: false, maxLength: 255),
                    TotalDeletedItemSizeInBytes = c.Long(nullable: false),
                    ItemCount = c.Int(nullable: false),
                    DeletedItemCount = c.Int(nullable: false),
                    Retrieved = c.DateTime(nullable: false),
                })
                .PrimaryKey(t => t.ID);
            CreateTable(
                "dbo.StatMailboxSizes",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        UserGuid = c.Guid(nullable: false),
                        UserPrincipalName = c.String(nullable: false, maxLength: 64),
                        MailboxDatabase = c.String(nullable: false, maxLength: 255),
                        TotalItemSize = c.String(nullable: false, maxLength: 255),
                        TotalItemSizeInBytes = c.Long(nullable: false),
                        TotalDeletedItemSize = c.String(nullable: false, maxLength: 255),
                        TotalDeletedItemSizeInBytes = c.Long(nullable: false),
                        ItemCount = c.Int(nullable: false),
                        DeletedItemCount = c.Int(nullable: false),
                        Retrieved = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.ID);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.StatMailboxSizes");
            DropTable("dbo.StatMailboxArchiveSizes");
        }
    }
}

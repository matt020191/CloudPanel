namespace CloudPanel.Database.EntityFramework.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AdjustBrandingOptions : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Brandings", "MenuType", c => c.Int(nullable: false));
            AlterColumn("dbo.Brandings", "Theme", c => c.String(nullable: false));
            DropColumn("dbo.Brandings", "TopMenu");
            DropColumn("dbo.Brandings", "TopMenuLargeIcons");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Brandings", "TopMenuLargeIcons", c => c.Boolean(nullable: false));
            AddColumn("dbo.Brandings", "TopMenu", c => c.Boolean(nullable: false));
            AlterColumn("dbo.Brandings", "Theme", c => c.String());
            DropColumn("dbo.Brandings", "MenuType");
        }
    }
}

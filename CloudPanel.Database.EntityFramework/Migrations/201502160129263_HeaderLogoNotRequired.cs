namespace CloudPanel.Database.EntityFramework.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class HeaderLogoNotRequired : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Brandings", "HeaderLogo", c => c.String());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Brandings", "HeaderLogo", c => c.String(nullable: false));
        }
    }
}

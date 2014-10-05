namespace CloudPanel.Database.EntityFramework.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddBrandingsTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Brandings",
                c => new
                    {
                        BrandingID = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                        HostName = c.String(nullable: false),
                        Email = c.String(nullable: false),
                        LoginLogo = c.String(nullable: false),
                        HeaderLogo = c.String(nullable: false),
                        Theme = c.String(),
                    })
                .PrimaryKey(t => t.BrandingID);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Brandings");
        }
    }
}

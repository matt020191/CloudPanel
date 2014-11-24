namespace CloudPanel.Database.EntityFramework.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddNewColumnsToRoles : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.UserRoles", "cDomains", c => c.Boolean(nullable: false));
            AddColumn("dbo.UserRoles", "cUsers", c => c.Boolean(nullable: false));
            AddColumn("dbo.UserRoles", "cExchangeContacts", c => c.Boolean(nullable: false));
            AddColumn("dbo.UserRoles", "cExchangeGroups", c => c.Boolean(nullable: false));
            AddColumn("dbo.UserRoles", "cExchangeResources", c => c.Boolean(nullable: false));
            AddColumn("dbo.UserRoles", "cExchangePublicFolders", c => c.Boolean(nullable: false));
            AddColumn("dbo.UserRoles", "cCitrix", c => c.Boolean(nullable: false));
            AddColumn("dbo.UserRoles", "cLync", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.UserRoles", "cLync");
            DropColumn("dbo.UserRoles", "cCitrix");
            DropColumn("dbo.UserRoles", "cExchangePublicFolders");
            DropColumn("dbo.UserRoles", "cExchangeResources");
            DropColumn("dbo.UserRoles", "cExchangeGroups");
            DropColumn("dbo.UserRoles", "cExchangeContacts");
            DropColumn("dbo.UserRoles", "cUsers");
            DropColumn("dbo.UserRoles", "cDomains");
        }
    }
}

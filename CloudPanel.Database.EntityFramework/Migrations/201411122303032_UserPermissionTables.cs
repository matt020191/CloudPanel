namespace CloudPanel.Database.EntityFramework.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UserPermissionTables : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.UserPermission",
                c => new
                    {
                        UserID = c.Int(nullable: false),
                        RoleID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.UserID);
            
            CreateTable(
                "dbo.UserRole",
                c => new
                    {
                        RoleID = c.Int(nullable: false, identity: true),
                        DisplayName = c.String(),
                        CompanyCode = c.String(),
                        vDomains = c.Boolean(nullable: false),
                        vUsers = c.Boolean(nullable: false),
                        vUsersEdit = c.Boolean(nullable: false),
                        vExchangeContacts = c.Boolean(nullable: false),
                        vExchangeGroups = c.Boolean(nullable: false),
                        vExchangeResources = c.Boolean(nullable: false),
                        vExchangePublicFolders = c.Boolean(nullable: false),
                        vCitrix = c.Boolean(nullable: false),
                        vLync = c.Boolean(nullable: false),
                        eDomains = c.Boolean(nullable: false),
                        eUsers = c.Boolean(nullable: false),
                        eExchangeContacts = c.Boolean(nullable: false),
                        eExchangeGroups = c.Boolean(nullable: false),
                        eExchangeResources = c.Boolean(nullable: false),
                        eExchangePublicFolders = c.Boolean(nullable: false),
                        eCitrix = c.Boolean(nullable: false),
                        eLync = c.Boolean(nullable: false),
                        ePermissions = c.Boolean(nullable: false),
                        dDomains = c.Boolean(nullable: false),
                        dUsers = c.Boolean(nullable: false),
                        dExchangeContacts = c.Boolean(nullable: false),
                        dExchangeGroups = c.Boolean(nullable: false),
                        dExchangeResources = c.Boolean(nullable: false),
                        dExchangePublicFolders = c.Boolean(nullable: false),
                        dCitrix = c.Boolean(nullable: false),
                        dLync = c.Boolean(nullable: false),
                        dPermissions = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.RoleID);
            
            DropTable("dbo.UserPermissions");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.UserPermissions",
                c => new
                    {
                        UserID = c.Int(nullable: false),
                        EnableExchange = c.Boolean(nullable: false),
                        DisableExchange = c.Boolean(nullable: false),
                        AddDomain = c.Boolean(nullable: false),
                        DeleteDomain = c.Boolean(nullable: false),
                        ModifyAcceptedDomain = c.Boolean(nullable: false),
                        ImportUsers = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.UserID);
            
            DropTable("dbo.UserRole");
            DropTable("dbo.UserPermission");
        }
    }
}

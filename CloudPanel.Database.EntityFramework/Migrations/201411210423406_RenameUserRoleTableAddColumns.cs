namespace CloudPanel.Database.EntityFramework.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RenameUserRoleTableAddColumns : DbMigration
    {
        public override void Up()
        {
            RenameTable("UserRole", "UserRoles");
        }
        
        public override void Down()
        {
        }
    }
}

namespace CloudPanel.Database.EntityFramework.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddRoleIDToUsersTable : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Users", "RoleID", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Users", "RoleID");
        }
    }
}

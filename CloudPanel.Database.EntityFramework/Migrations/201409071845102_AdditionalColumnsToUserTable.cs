namespace CloudPanel.Database.EntityFramework.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AdditionalColumnsToUserTable : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Users", "Street", c => c.String());
            AddColumn("dbo.Users", "City", c => c.String());
            AddColumn("dbo.Users", "State", c => c.String());
            AddColumn("dbo.Users", "PostalCode", c => c.String());
            AddColumn("dbo.Users", "Country", c => c.String());
            AddColumn("dbo.Users", "Company", c => c.String());
            AddColumn("dbo.Users", "JobTitle", c => c.String());
            AddColumn("dbo.Users", "TelephoneNumber", c => c.String());
            AddColumn("dbo.Users", "Fax", c => c.String());
            AddColumn("dbo.Users", "HomePhone", c => c.String());
            AddColumn("dbo.Users", "MobilePhone", c => c.String());
            AddColumn("dbo.Users", "Notes", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Users", "Notes");
            DropColumn("dbo.Users", "MobilePhone");
            DropColumn("dbo.Users", "HomePhone");
            DropColumn("dbo.Users", "Fax");
            DropColumn("dbo.Users", "TelephoneNumber");
            DropColumn("dbo.Users", "JobTitle");
            DropColumn("dbo.Users", "Company");
            DropColumn("dbo.Users", "Country");
            DropColumn("dbo.Users", "PostalCode");
            DropColumn("dbo.Users", "State");
            DropColumn("dbo.Users", "City");
            DropColumn("dbo.Users", "Street");
        }
    }
}

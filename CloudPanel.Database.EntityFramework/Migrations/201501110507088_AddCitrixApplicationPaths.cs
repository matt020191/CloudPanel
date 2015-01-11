namespace CloudPanel.Database.EntityFramework.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddCitrixApplicationPaths : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.CitrixApplications", "CommandLineExecutable", c => c.String());
            AddColumn("dbo.CitrixApplications", "CommandLineArguments", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.CitrixApplications", "CommandLineArguments");
            DropColumn("dbo.CitrixApplications", "CommandLineExecutable");
        }
    }
}

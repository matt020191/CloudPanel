namespace CloudPanel.Database.EntityFramework.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddUsernameAndIPToAuditTrace : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AuditTraces", "Username", c => c.String());
            AddColumn("dbo.AuditTraces", "IPAddress", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.AuditTraces", "IPAddress");
            DropColumn("dbo.AuditTraces", "Username");
        }
    }
}

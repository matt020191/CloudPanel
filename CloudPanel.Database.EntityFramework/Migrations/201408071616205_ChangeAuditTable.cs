namespace CloudPanel.Database.EntityFramework.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ChangeAuditTable : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.Contacts", new[] { "DistinguishedName" });
            AddColumn("dbo.Audit", "Timestamp", c => c.DateTime(nullable: false));
            AddColumn("dbo.Audit", "Method", c => c.String(nullable: false, maxLength: 16));
            AddColumn("dbo.Audit", "Path", c => c.String(nullable: false, maxLength: 512));
            AddColumn("dbo.Audit", "IdentifyingInfo", c => c.String(maxLength: 255));
            AddColumn("dbo.Audit", "UserHostAddress", c => c.String(nullable: false, maxLength: 64));
            DropColumn("dbo.Audit", "Date");
            DropColumn("dbo.Audit", "ActionID");
            DropColumn("dbo.Audit", "Variable1");
            DropColumn("dbo.Audit", "Variable2");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Audit", "Variable2", c => c.String());
            AddColumn("dbo.Audit", "Variable1", c => c.String(nullable: false));
            AddColumn("dbo.Audit", "ActionID", c => c.Int(nullable: false));
            AddColumn("dbo.Audit", "Date", c => c.DateTime(nullable: false));
            DropColumn("dbo.Audit", "UserHostAddress");
            DropColumn("dbo.Audit", "IdentifyingInfo");
            DropColumn("dbo.Audit", "Path");
            DropColumn("dbo.Audit", "Method");
            DropColumn("dbo.Audit", "Timestamp");
            CreateIndex("dbo.Contacts", "DistinguishedName", unique: true);
        }
    }
}

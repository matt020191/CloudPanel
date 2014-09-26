namespace CloudPanel.Database.EntityFramework.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ChangeKeyInGroupsTable : DbMigration
    {
        public override void Up()
        {
            DropPrimaryKey("DistributionGroups");
            AlterColumn("DistributionGroups", "DistinguishedName", c => c.String(maxLength: 255, unicode: false));
            AddPrimaryKey("DistributionGroups", "ID");
        }
        
        public override void Down()
        {
            DropPrimaryKey("DistributionGroups");
            AlterColumn("DistributionGroups", "DistinguishedName", c => c.String(nullable: false, maxLength: 255, unicode: false));
            AddPrimaryKey("DistributionGroups", "DistinguishedName");
        }
    }
}

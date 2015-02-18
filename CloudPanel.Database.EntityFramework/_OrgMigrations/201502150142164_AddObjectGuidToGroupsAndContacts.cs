namespace CloudPanel.Database.EntityFramework.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddObjectGuidToGroupsAndContacts
    {/*
        public override void Up()
        {
            AddColumn("dbo.Contacts", "ObjectGuid", c => c.Guid(nullable: false));
            AddColumn("dbo.DistributionGroups", "ObjectGuid", c => c.Guid(nullable: false));
            AddColumn("dbo.DistributionGroups", "IsSecurityGroup", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.DistributionGroups", "IsSecurityGroup");
            DropColumn("dbo.DistributionGroups", "ObjectGuid");
            DropColumn("dbo.Contacts", "ObjectGuid");
        }*/
    }
}

namespace CloudPanel.Database.EntityFramework.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddMissingColumns
    {/*
        public override void Up()
        {
            // THIS ALREADY EXISTED IN 3.0.906
            //AddColumn("dbo.Plans_Organization", "MaxExchangeActivesyncPolicies", c => c.Int(nullable: true));
            
            AddColumn("dbo.ResourceMailboxes", "ResourceGuid", c => c.Guid());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Plans_Organization", "MaxExchangeActivesyncPolicies");
        }*/
    }
}

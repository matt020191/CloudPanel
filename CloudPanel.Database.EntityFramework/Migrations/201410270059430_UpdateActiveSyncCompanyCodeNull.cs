namespace CloudPanel.Database.EntityFramework.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateActiveSyncCompanyCodeNull : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Plans_ExchangeActiveSync", "CompanyCode", c => c.String(maxLength: 255, unicode: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Plans_ExchangeActiveSync", "CompanyCode", c => c.String(nullable: false, maxLength: 255, unicode: false));
        }
    }
}

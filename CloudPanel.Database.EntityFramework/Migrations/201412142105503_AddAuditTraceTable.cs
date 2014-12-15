namespace CloudPanel.Database.EntityFramework.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddAuditTraceTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.AuditTraces",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Method = c.String(),
                        Route = c.String(),
                        Parameters = c.String(),
                    })
                .PrimaryKey(t => t.ID);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.AuditTraces");
        }
    }
}

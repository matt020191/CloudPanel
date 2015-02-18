namespace CloudPanel.Database.EntityFramework.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class FixUserPermissionTableAndDropOtherTables
    {/*
        public override void Up()
        {
            CreateIndex("dbo.Users", "RoleID");
            AddForeignKey("dbo.Users", "RoleID", "dbo.UserRoles", "RoleID");
            DropTable("dbo.UserPlans");
            DropTable("dbo.UserPlansCitrix");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.UserPlansCitrix",
                c => new
                    {
                        UPCID = c.Int(nullable: false, identity: true),
                        UserID = c.Int(nullable: false),
                        CitrixPlanID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.UPCID);
            
            CreateTable(
                "dbo.UserPlans",
                c => new
                    {
                        UserGuid = c.Guid(nullable: false),
                        ProductID = c.Int(nullable: false),
                        PlanID = c.Int(nullable: false),
                        CompanyCode = c.String(maxLength: 255),
                    })
                .PrimaryKey(t => new { t.UserGuid, t.ProductID, t.PlanID });
            
            DropForeignKey("dbo.Users", "RoleID", "dbo.UserRoles");
            DropIndex("dbo.Users", new[] { "RoleID" });
        }*/
    }
}

namespace CloudPanel.Database.EntityFramework.Migrations
{
    using System;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Migrations;
    
    public partial class AddScalarFunctions : DbMigration
    {
        public override void Up()
        {
            this.Sql(@"CREATE FUNCTION [dbo].[GetCitrixPlanPrice]
                        (
	                        @CompanyCode varchar(255),
	                        @CitrixPlanID int
                        )
                        RETURNS varchar(25)
                        AS
                        BEGIN
	                        DECLARE @price varchar(25);
	
	                        SET @price = (SELECT Price FROM PriceOverride WHERE CompanyCode=@CompanyCode AND PlanID=@CitrixPlanID AND Product='Citrix');
	
	                        IF @price IS NULL
		                        SET @price = (SELECT Price FROM Plans_Citrix WHERE CitrixPlanID=@CitrixPlanID);
		
	                        RETURN @price;
                        END");
        }
        
        public override void Down()
        {
            this.Sql(@"DROP FUNCTION GetCitrixPlanPrice");
        }
    }
}

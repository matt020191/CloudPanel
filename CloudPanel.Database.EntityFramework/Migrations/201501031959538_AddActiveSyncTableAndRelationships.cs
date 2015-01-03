namespace CloudPanel.Database.EntityFramework.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddActiveSyncTableAndRelationships : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.UserActiveSyncDevices",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        DeviceGuid = c.Guid(nullable: false),
                        FirstSyncTime = c.DateTime(),
                        LastPolicyUpdateTime = c.DateTime(),
                        LastSyncAttemptTime = c.DateTime(),
                        LastSuccessSync = c.DateTime(),
                        DeviceWipeSentTime = c.DateTime(),
                        DeviceWipeRequestTime = c.DateTime(),
                        DeviceWipeAckTime = c.DateTime(),
                        LastPingHeartbeat = c.Int(nullable: false),
                        Identity = c.String(),
                        DeviceType = c.String(),
                        DeviceID = c.String(),
                        DeviceUserAgent = c.String(),
                        DeviceModel = c.String(),
                        DeviceImei = c.String(),
                        DeviceFriendlyName = c.String(),
                        DeviceOS = c.String(),
                        DevicePhoneNumber = c.String(),
                        Status = c.String(),
                        StatusNote = c.String(),
                        DevicePolicyApplied = c.String(),
                        DevicePolicyApplicationStatus = c.String(),
                        DeviceActiveSyncVersion = c.String(),
                        NumberOfFoldersSynced = c.String(),
                        UserGuid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.Users", t => t.UserGuid, cascadeDelete: true)
                .Index(t => t.UserGuid);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.UserActiveSyncDevices", "UserGuid", "dbo.Users");
            DropIndex("dbo.UserActiveSyncDevices", new[] { "UserGuid" });
            DropTable("dbo.UserActiveSyncDevices");
        }
    }
}

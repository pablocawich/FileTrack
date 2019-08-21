namespace FileTracking.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveRequestFromNotifAndAddNewFields : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Notifications", "RequestId", "dbo.Requests");
            DropIndex("dbo.Notifications", new[] { "RequestId" });
            AddColumn("dbo.Notifications", "FileVolumeId", c => c.Int());
            AddColumn("dbo.Notifications", "RecipientBranchId", c => c.Byte());
            AddColumn("dbo.Notifications", "SenderBranchId", c => c.Byte());
            CreateIndex("dbo.Notifications", "FileVolumeId");
            CreateIndex("dbo.Notifications", "RecipientBranchId");
            CreateIndex("dbo.Notifications", "SenderBranchId");
            AddForeignKey("dbo.Notifications", "FileVolumeId", "dbo.FileVolumes", "Id");
            AddForeignKey("dbo.Notifications", "RecipientBranchId", "dbo.Branches", "Id");
            AddForeignKey("dbo.Notifications", "SenderBranchId", "dbo.Branches", "Id");
            DropColumn("dbo.Notifications", "RequestId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Notifications", "RequestId", c => c.Int());
            DropForeignKey("dbo.Notifications", "SenderBranchId", "dbo.Branches");
            DropForeignKey("dbo.Notifications", "RecipientBranchId", "dbo.Branches");
            DropForeignKey("dbo.Notifications", "FileVolumeId", "dbo.FileVolumes");
            DropIndex("dbo.Notifications", new[] { "SenderBranchId" });
            DropIndex("dbo.Notifications", new[] { "RecipientBranchId" });
            DropIndex("dbo.Notifications", new[] { "FileVolumeId" });
            DropColumn("dbo.Notifications", "SenderBranchId");
            DropColumn("dbo.Notifications", "RecipientBranchId");
            DropColumn("dbo.Notifications", "FileVolumeId");
            CreateIndex("dbo.Notifications", "RequestId");
            AddForeignKey("dbo.Notifications", "RequestId", "dbo.Requests", "Id");
        }
    }
}

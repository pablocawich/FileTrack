namespace FileTracking.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RequestAndNotificationModifications : DbMigration
    {
        public override void Up()
        {
            RenameColumn(table: "dbo.Requests", name: "BranchesId", newName: "CurrentFileBranchId");
            RenameIndex(table: "dbo.Requests", name: "IX_BranchesId", newName: "IX_CurrentFileBranchId");
            AddColumn("dbo.Notifications", "SenderUserId", c => c.Int());
            AddColumn("dbo.Requests", "OriginBranchId", c => c.Byte(nullable: false));
            AddColumn("dbo.Requests", "AcceptedById", c => c.Int());
            AddColumn("dbo.Requests", "ReturnAcceptById", c => c.Int());
            CreateIndex("dbo.Notifications", "SenderUserId");
            CreateIndex("dbo.Requests", "OriginBranchId");
            CreateIndex("dbo.Requests", "AcceptedById");
            CreateIndex("dbo.Requests", "ReturnAcceptById");
            AddForeignKey("dbo.Requests", "AcceptedById", "dbo.AdUsers", "Id");
            AddForeignKey("dbo.Requests", "OriginBranchId", "dbo.Branches", "Id", cascadeDelete: false);
            AddForeignKey("dbo.Requests", "ReturnAcceptById", "dbo.AdUsers", "Id");
            AddForeignKey("dbo.Notifications", "SenderUserId", "dbo.AdUsers", "Id");
            DropColumn("dbo.Notifications", "SenderUser");
            DropColumn("dbo.Requests", "RequesteeBranch");
            DropColumn("dbo.Requests", "AcceptedBy");
            DropColumn("dbo.Requests", "ReturnAcceptBy");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Requests", "ReturnAcceptBy", c => c.String());
            AddColumn("dbo.Requests", "AcceptedBy", c => c.String());
            AddColumn("dbo.Requests", "RequesteeBranch", c => c.Byte(nullable: false));
            AddColumn("dbo.Notifications", "SenderUser", c => c.String(maxLength: 32));
            DropForeignKey("dbo.Notifications", "SenderUserId", "dbo.AdUsers");
            DropForeignKey("dbo.Requests", "ReturnAcceptById", "dbo.AdUsers");
            DropForeignKey("dbo.Requests", "OriginBranchId", "dbo.Branches");
            DropForeignKey("dbo.Requests", "AcceptedById", "dbo.AdUsers");
            DropIndex("dbo.Requests", new[] { "ReturnAcceptById" });
            DropIndex("dbo.Requests", new[] { "AcceptedById" });
            DropIndex("dbo.Requests", new[] { "OriginBranchId" });
            DropIndex("dbo.Notifications", new[] { "SenderUserId" });
            DropColumn("dbo.Requests", "ReturnAcceptById");
            DropColumn("dbo.Requests", "AcceptedById");
            DropColumn("dbo.Requests", "OriginBranchId");
            DropColumn("dbo.Notifications", "SenderUserId");
            RenameIndex(table: "dbo.Requests", name: "IX_CurrentFileBranchId", newName: "IX_BranchesId");
            RenameColumn(table: "dbo.Requests", name: "CurrentFileBranchId", newName: "BranchesId");
        }
    }
}

namespace FileTracking.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveUnecessaryFieldsFromRejTable : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.RejectedRequests", "ReturnAcceptById", "dbo.AdUsers");
            DropIndex("dbo.RejectedRequests", new[] { "ReturnAcceptById" });
            RenameColumn(table: "dbo.RejectedRequests", name: "RegistryUserAcceptId", newName: "RegistryUserRejectId");
            RenameIndex(table: "dbo.RejectedRequests", name: "IX_RegistryUserAcceptId", newName: "IX_RegistryUserRejectId");
            AddColumn("dbo.RejectedRequests", "RegRejectedDate", c => c.DateTime());
            DropColumn("dbo.RejectedRequests", "RegAcceptedDate");
            DropColumn("dbo.RejectedRequests", "ReturnAcceptById");
            DropColumn("dbo.RejectedRequests", "ReturnDate");
        }
        
        public override void Down()
        {
            AddColumn("dbo.RejectedRequests", "ReturnDate", c => c.DateTime());
            AddColumn("dbo.RejectedRequests", "ReturnAcceptById", c => c.Int());
            AddColumn("dbo.RejectedRequests", "RegAcceptedDate", c => c.DateTime());
            DropColumn("dbo.RejectedRequests", "RegRejectedDate");
            RenameIndex(table: "dbo.RejectedRequests", name: "IX_RegistryUserRejectId", newName: "IX_RegistryUserAcceptId");
            RenameColumn(table: "dbo.RejectedRequests", name: "RegistryUserRejectId", newName: "RegistryUserAcceptId");
            CreateIndex("dbo.RejectedRequests", "ReturnAcceptById");
            AddForeignKey("dbo.RejectedRequests", "ReturnAcceptById", "dbo.AdUsers", "Id");
        }
    }
}

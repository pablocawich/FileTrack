namespace FileTracking.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddCompletedRequestTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.CompletedRequests",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        FileVolumeId = c.Int(nullable: false),
                        RequesterUserId = c.Int(nullable: false),
                        RequesterBranchId = c.Byte(nullable: false),
                        FileBranchId = c.Byte(nullable: false),
                        RequestDate = c.DateTime(nullable: false),
                        RegistryUserAcceptId = c.Int(),
                        RegAcceptedDate = c.DateTime(),
                        ReturnAcceptById = c.Int(),
                        ReturnDate = c.DateTime(),
                        UserTransferFromId = c.Int(),
                        TransferType = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Branches", t => t.FileBranchId, cascadeDelete: true)
                .ForeignKey("dbo.FileVolumes", t => t.FileVolumeId, cascadeDelete: false)
                .ForeignKey("dbo.AdUsers", t => t.RegistryUserAcceptId)
                .ForeignKey("dbo.Branches", t => t.RequesterBranchId, cascadeDelete: false)
                .ForeignKey("dbo.AdUsers", t => t.RequesterUserId, cascadeDelete: false)
                .ForeignKey("dbo.AdUsers", t => t.ReturnAcceptById)
                .ForeignKey("dbo.AdUsers", t => t.UserTransferFromId)
                .Index(t => t.FileVolumeId)
                .Index(t => t.RequesterUserId)
                .Index(t => t.RequesterBranchId)
                .Index(t => t.FileBranchId)
                .Index(t => t.RegistryUserAcceptId)
                .Index(t => t.ReturnAcceptById)
                .Index(t => t.UserTransferFromId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.CompletedRequests", "UserTransferFromId", "dbo.AdUsers");
            DropForeignKey("dbo.CompletedRequests", "ReturnAcceptById", "dbo.AdUsers");
            DropForeignKey("dbo.CompletedRequests", "RequesterUserId", "dbo.AdUsers");
            DropForeignKey("dbo.CompletedRequests", "RequesterBranchId", "dbo.Branches");
            DropForeignKey("dbo.CompletedRequests", "RegistryUserAcceptId", "dbo.AdUsers");
            DropForeignKey("dbo.CompletedRequests", "FileVolumeId", "dbo.FileVolumes");
            DropForeignKey("dbo.CompletedRequests", "FileBranchId", "dbo.Branches");
            DropIndex("dbo.CompletedRequests", new[] { "UserTransferFromId" });
            DropIndex("dbo.CompletedRequests", new[] { "ReturnAcceptById" });
            DropIndex("dbo.CompletedRequests", new[] { "RegistryUserAcceptId" });
            DropIndex("dbo.CompletedRequests", new[] { "FileBranchId" });
            DropIndex("dbo.CompletedRequests", new[] { "RequesterBranchId" });
            DropIndex("dbo.CompletedRequests", new[] { "RequesterUserId" });
            DropIndex("dbo.CompletedRequests", new[] { "FileVolumeId" });
            DropTable("dbo.CompletedRequests");
        }
    }
}

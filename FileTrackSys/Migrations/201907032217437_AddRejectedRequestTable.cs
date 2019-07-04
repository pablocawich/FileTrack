namespace FileTracking.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddRejectedRequestTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.RejectedRequests",
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
            DropForeignKey("dbo.RejectedRequests", "UserTransferFromId", "dbo.AdUsers");
            DropForeignKey("dbo.RejectedRequests", "ReturnAcceptById", "dbo.AdUsers");
            DropForeignKey("dbo.RejectedRequests", "RequesterUserId", "dbo.AdUsers");
            DropForeignKey("dbo.RejectedRequests", "RequesterBranchId", "dbo.Branches");
            DropForeignKey("dbo.RejectedRequests", "RegistryUserAcceptId", "dbo.AdUsers");
            DropForeignKey("dbo.RejectedRequests", "FileVolumeId", "dbo.FileVolumes");
            DropForeignKey("dbo.RejectedRequests", "FileBranchId", "dbo.Branches");
            DropIndex("dbo.RejectedRequests", new[] { "UserTransferFromId" });
            DropIndex("dbo.RejectedRequests", new[] { "ReturnAcceptById" });
            DropIndex("dbo.RejectedRequests", new[] { "RegistryUserAcceptId" });
            DropIndex("dbo.RejectedRequests", new[] { "FileBranchId" });
            DropIndex("dbo.RejectedRequests", new[] { "RequesterBranchId" });
            DropIndex("dbo.RejectedRequests", new[] { "RequesterUserId" });
            DropIndex("dbo.RejectedRequests", new[] { "FileVolumeId" });
            DropTable("dbo.RejectedRequests");
        }
    }
}

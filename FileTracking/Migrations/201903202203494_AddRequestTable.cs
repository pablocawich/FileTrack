namespace FileTracking.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddRequestTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Requests",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        FileVolumesId = c.Int(nullable: false),
                        UserId = c.Int(nullable: false),
                        FileId = c.Int(nullable: false),
                        BranchesId = c.Byte(nullable: false),
                        RequestStatusId = c.Byte(nullable: false),
                        RequestDate = c.DateTime(nullable: false),
                        AcceptedDate = c.DateTime(),
                        AcceptedBy = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Branches", t => t.BranchesId, cascadeDelete: true)
                .ForeignKey("dbo.Files", t => t.FileId, cascadeDelete: false)
                .ForeignKey("dbo.FileVolumes", t => t.FileVolumesId, cascadeDelete: false)
                .ForeignKey("dbo.RequestStatus", t => t.RequestStatusId, cascadeDelete: true)
                .ForeignKey("dbo.AdUsers", t => t.UserId, cascadeDelete: false)
                .Index(t => t.FileVolumesId)
                .Index(t => t.UserId)
                .Index(t => t.FileId)
                .Index(t => t.BranchesId)
                .Index(t => t.RequestStatusId);
            
            CreateTable(
                "dbo.RequestStatus",
                c => new
                    {
                        Id = c.Byte(nullable: false),
                        Status = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Requests", "UserId", "dbo.AdUsers");
            DropForeignKey("dbo.Requests", "RequestStatusId", "dbo.RequestStatus");
            DropForeignKey("dbo.Requests", "FileVolumesId", "dbo.FileVolumes");
            DropForeignKey("dbo.Requests", "FileId", "dbo.Files");
            DropForeignKey("dbo.Requests", "BranchesId", "dbo.Branches");
            DropIndex("dbo.Requests", new[] { "RequestStatusId" });
            DropIndex("dbo.Requests", new[] { "BranchesId" });
            DropIndex("dbo.Requests", new[] { "FileId" });
            DropIndex("dbo.Requests", new[] { "UserId" });
            DropIndex("dbo.Requests", new[] { "FileVolumesId" });
            DropTable("dbo.RequestStatus");
            DropTable("dbo.Requests");
        }
    }
}

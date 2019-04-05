namespace FileTracking.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DeleteVolumeTables : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.FileVolumes", "FileId", "dbo.Files");
            DropForeignKey("dbo.Volumes", "FileVolumeId", "dbo.FileVolumes");
            DropIndex("dbo.FileVolumes", new[] { "FileId" });
            DropIndex("dbo.Volumes", new[] { "FileVolumeId" });
            DropTable("dbo.FileVolumes");
            DropTable("dbo.Volumes");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.Volumes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        FileVolumeId = c.Int(nullable: false),
                        VolumeForFile = c.Byte(nullable: false),
                        Comments = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.FileVolumes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        FileId = c.Int(nullable: false),
                        VolumeCount = c.Byte(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateIndex("dbo.Volumes", "FileVolumeId");
            CreateIndex("dbo.FileVolumes", "FileId");
            AddForeignKey("dbo.Volumes", "FileVolumeId", "dbo.FileVolumes", "Id", cascadeDelete: true);
            AddForeignKey("dbo.FileVolumes", "FileId", "dbo.Files", "Id", cascadeDelete: true);
        }
    }
}

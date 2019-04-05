namespace FileTracking.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddVolumeTables : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.FileVolumes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        FileId = c.Int(nullable: false),
                        VolumeCount = c.Byte(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Files", t => t.FileId, cascadeDelete: true)
                .Index(t => t.FileId);
            
            CreateTable(
                "dbo.Volumes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        FileVolumeId = c.Int(nullable: false),
                        VolumeForFile = c.Byte(nullable: false),
                        Comments = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.FileVolumes", t => t.FileVolumeId, cascadeDelete: true)
                .Index(t => t.FileVolumeId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Volumes", "FileVolumeId", "dbo.FileVolumes");
            DropForeignKey("dbo.FileVolumes", "FileId", "dbo.Files");
            DropIndex("dbo.Volumes", new[] { "FileVolumeId" });
            DropIndex("dbo.FileVolumes", new[] { "FileId" });
            DropTable("dbo.Volumes");
            DropTable("dbo.FileVolumes");
        }
    }
}

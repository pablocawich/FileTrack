namespace FileTracking.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddFileVolumesToFile : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.FileVolumes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        FileId = c.Int(nullable: false),
                        Volume = c.Byte(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Files", t => t.FileId, cascadeDelete: true)
                .Index(t => t.FileId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.FileVolumes", "FileId", "dbo.Files");
            DropIndex("dbo.FileVolumes", new[] { "FileId" });
            DropTable("dbo.FileVolumes");
        }
    }
}

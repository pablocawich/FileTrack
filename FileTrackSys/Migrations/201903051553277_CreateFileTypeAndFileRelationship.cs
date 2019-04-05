namespace FileTracking.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CreateFileTypeAndFileRelationship : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Files", "FileTypeId", c => c.Byte(nullable: false));
            CreateIndex("dbo.Files", "FileTypeId");
            AddForeignKey("dbo.Files", "FileTypeId", "dbo.FileTypes", "Id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Files", "FileTypeId", "dbo.FileTypes");
            DropIndex("dbo.Files", new[] { "FileTypeId" });
            DropColumn("dbo.Files", "FileTypeId");
        }
    }
}

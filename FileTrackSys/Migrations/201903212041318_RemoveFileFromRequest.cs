namespace FileTracking.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveFileFromRequest : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Requests", "FileId", "dbo.Files");
            DropIndex("dbo.Requests", new[] { "FileId" });
            DropColumn("dbo.Requests", "FileId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Requests", "FileId", c => c.Int(nullable: false));
            CreateIndex("dbo.Requests", "FileId");
            AddForeignKey("dbo.Requests", "FileId", "dbo.Files", "Id", cascadeDelete: true);
        }
    }
}

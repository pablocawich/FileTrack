namespace FileTracking.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddFileStatusToFile : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.FileStatus",
                c => new
                    {
                        Id = c.Byte(nullable: false),
                        Status = c.String(nullable: false, maxLength: 32),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.Files", "FileStatusId", c => c.Byte(nullable: false));
            CreateIndex("dbo.Files", "FileStatusId");
            AddForeignKey("dbo.Files", "FileStatusId", "dbo.FileStatus", "Id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Files", "FileStatusId", "dbo.FileStatus");
            DropIndex("dbo.Files", new[] { "FileStatusId" });
            DropColumn("dbo.Files", "FileStatusId");
            DropTable("dbo.FileStatus");
        }
    }
}

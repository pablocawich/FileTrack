namespace FileTracking.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddNewFiledsToVolumes : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.States",
                c => new
                    {
                        Id = c.Byte(nullable: false),
                        State = c.String(maxLength: 32),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.FileVolumes", "Comment", c => c.String(maxLength: 255));
            AddColumn("dbo.FileVolumes", "StatesId", c => c.Byte(nullable: false));
            CreateIndex("dbo.FileVolumes", "StatesId");
            AddForeignKey("dbo.FileVolumes", "StatesId", "dbo.States", "Id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.FileVolumes", "StatesId", "dbo.States");
            DropIndex("dbo.FileVolumes", new[] { "StatesId" });
            DropColumn("dbo.FileVolumes", "StatesId");
            DropColumn("dbo.FileVolumes", "Comment");
            DropTable("dbo.States");
        }
    }
}

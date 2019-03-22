namespace FileTracking.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddBranchToVolumes : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.FileVolumes", "BranchesId", c => c.Byte(nullable: false));
            CreateIndex("dbo.FileVolumes", "BranchesId");
            AddForeignKey("dbo.FileVolumes", "BranchesId", "dbo.Branches", "Id", cascadeDelete: false);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.FileVolumes", "BranchesId", "dbo.Branches");
            DropIndex("dbo.FileVolumes", new[] { "BranchesId" });
            DropColumn("dbo.FileVolumes", "BranchesId");
        }
    }
}

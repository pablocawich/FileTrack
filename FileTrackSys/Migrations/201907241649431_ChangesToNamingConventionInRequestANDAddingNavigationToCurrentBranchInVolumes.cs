namespace FileTracking.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ChangesToNamingConventionInRequestANDAddingNavigationToCurrentBranchInVolumes : DbMigration
    {
        public override void Up()
        {
            RenameColumn(table: "dbo.Requests", name: "CurrentFileBranchId", newName: "RecipientBranchId");
            RenameIndex(table: "dbo.Requests", name: "IX_CurrentFileBranchId", newName: "IX_RecipientBranchId");
            AddColumn("dbo.FileVolumes", "CurrentLocationId", c => c.Byte(nullable: false));
            CreateIndex("dbo.FileVolumes", "CurrentLocationId");
            AddForeignKey("dbo.FileVolumes", "CurrentLocationId", "dbo.Branches", "Id", cascadeDelete: false);
            DropColumn("dbo.FileVolumes", "CurrentLocation");
        }
        
        public override void Down()
        {
            AddColumn("dbo.FileVolumes", "CurrentLocation", c => c.Byte(nullable: false));
            DropForeignKey("dbo.FileVolumes", "CurrentLocationId", "dbo.Branches");
            DropIndex("dbo.FileVolumes", new[] { "CurrentLocationId" });
            DropColumn("dbo.FileVolumes", "CurrentLocationId");
            RenameIndex(table: "dbo.Requests", name: "IX_RecipientBranchId", newName: "IX_CurrentFileBranchId");
            RenameColumn(table: "dbo.Requests", name: "RecipientBranchId", newName: "CurrentFileBranchId");
        }
    }
}

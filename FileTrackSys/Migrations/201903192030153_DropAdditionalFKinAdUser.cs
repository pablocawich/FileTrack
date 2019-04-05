namespace FileTracking.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DropAdditionalFKinAdUser : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.AdUsers", "Branches_Id", "dbo.Branches");
            DropIndex("dbo.AdUsers", new[] { "Branches_Id" });
            RenameColumn(table: "dbo.AdUsers", name: "Branches_Id", newName: "BranchesId");
            AlterColumn("dbo.AdUsers", "BranchesId", c => c.Byte(nullable: false));
            CreateIndex("dbo.AdUsers", "BranchesId");
            AddForeignKey("dbo.AdUsers", "BranchesId", "dbo.Branches", "Id", cascadeDelete: true);
            DropColumn("dbo.AdUsers", "BranchId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.AdUsers", "BranchId", c => c.Byte(nullable: false));
            DropForeignKey("dbo.AdUsers", "BranchesId", "dbo.Branches");
            DropIndex("dbo.AdUsers", new[] { "BranchesId" });
            AlterColumn("dbo.AdUsers", "BranchesId", c => c.Byte());
            RenameColumn(table: "dbo.AdUsers", name: "BranchesId", newName: "Branches_Id");
            CreateIndex("dbo.AdUsers", "Branches_Id");
            AddForeignKey("dbo.AdUsers", "Branches_Id", "dbo.Branches", "Id");
        }
    }
}

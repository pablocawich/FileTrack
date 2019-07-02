namespace FileTracking.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RenameOriginBranchToRequesterbranchInRequest : DbMigration
    {
        public override void Up()
        {
            RenameColumn(table: "dbo.Requests", name: "OriginBranchId", newName: "RequesterBranchId");
            RenameIndex(table: "dbo.Requests", name: "IX_OriginBranchId", newName: "IX_RequesterBranchId");
        }
        
        public override void Down()
        {
            RenameIndex(table: "dbo.Requests", name: "IX_RequesterBranchId", newName: "IX_OriginBranchId");
            RenameColumn(table: "dbo.Requests", name: "RequesterBranchId", newName: "OriginBranchId");
        }
    }
}

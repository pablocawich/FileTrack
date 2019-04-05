namespace FileTracking.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddBranchesTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Branches",
                c => new
                    {
                        Id = c.Byte(nullable: false),
                        Branch = c.String(),
                        DistrictsId = c.Byte(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Districts", t => t.DistrictsId, cascadeDelete: true)
                .Index(t => t.DistrictsId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Branches", "DistrictsId", "dbo.Districts");
            DropIndex("dbo.Branches", new[] { "DistrictsId" });
            DropTable("dbo.Branches");
        }
    }
}

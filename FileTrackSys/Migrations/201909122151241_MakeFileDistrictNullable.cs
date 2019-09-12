namespace FileTracking.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class MakeFileDistrictNullable : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Files", "DistrictsId", "dbo.Districts");
            DropIndex("dbo.Files", new[] { "DistrictsId" });
            AlterColumn("dbo.Files", "DistrictsId", c => c.Byte());
            CreateIndex("dbo.Files", "DistrictsId");
            AddForeignKey("dbo.Files", "DistrictsId", "dbo.Districts", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Files", "DistrictsId", "dbo.Districts");
            DropIndex("dbo.Files", new[] { "DistrictsId" });
            AlterColumn("dbo.Files", "DistrictsId", c => c.Byte(nullable: false));
            CreateIndex("dbo.Files", "DistrictsId");
            AddForeignKey("dbo.Files", "DistrictsId", "dbo.Districts", "Id", cascadeDelete: true);
        }
    }
}

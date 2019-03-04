namespace FileTracking.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddSpecificLocationsToFile : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Files", "DistrictsId", "dbo.Districts");
            DropIndex("dbo.Files", new[] { "DistrictsId" });
            DropPrimaryKey("dbo.Districts");
            AlterColumn("dbo.Districts", "Id", c => c.Byte(nullable: false));
            AlterColumn("dbo.Files", "DistrictsId", c => c.Byte(nullable: false));
            AddPrimaryKey("dbo.Districts", "Id");
            CreateIndex("dbo.Files", "DistrictsId");
            AddForeignKey("dbo.Files", "DistrictsId", "dbo.Districts", "Id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Files", "DistrictsId", "dbo.Districts");
            DropIndex("dbo.Files", new[] { "DistrictsId" });
            DropPrimaryKey("dbo.Districts");
            AlterColumn("dbo.Files", "DistrictsId", c => c.Int(nullable: false));
            AlterColumn("dbo.Districts", "Id", c => c.Int(nullable: false, identity: true));
            AddPrimaryKey("dbo.Districts", "Id");
            CreateIndex("dbo.Files", "DistrictsId");
            AddForeignKey("dbo.Files", "DistrictsId", "dbo.Districts", "Id", cascadeDelete: true);
        }
    }
}

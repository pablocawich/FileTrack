namespace FileTracking.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddLocationTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Locations",
                c => new
                    {
                        LocationId = c.String(nullable: false, maxLength: 128),
                        DistrictsId = c.Byte(nullable: false),
                        Name = c.String(maxLength: 64),
                    })
                .PrimaryKey(t => t.LocationId)
                .ForeignKey("dbo.Districts", t => t.DistrictsId, cascadeDelete: true)
                .Index(t => t.DistrictsId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Locations", "DistrictsId", "dbo.Districts");
            DropIndex("dbo.Locations", new[] { "DistrictsId" });
            DropTable("dbo.Locations");
        }
    }
}

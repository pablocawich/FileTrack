namespace FileTracking.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddLocationToFiles : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Files", "LocationId", c => c.String(nullable: false, maxLength: 128));
            CreateIndex("dbo.Files", "LocationId");
            AddForeignKey("dbo.Files", "LocationId", "dbo.Locations", "LocationId", cascadeDelete: false);
            DropColumn("dbo.Files", "CityOrTown");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Files", "CityOrTown", c => c.String(nullable: false, maxLength: 82));
            DropForeignKey("dbo.Files", "LocationId", "dbo.Locations");
            DropIndex("dbo.Files", new[] { "LocationId" });
            DropColumn("dbo.Files", "LocationId");
        }
    }
}

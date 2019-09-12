namespace FileTracking.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemovingRequiredfromFileFields : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Files", "LocationId", "dbo.Locations");
            DropIndex("dbo.Files", new[] { "LocationId" });
            AlterColumn("dbo.Files", "LocationId", c => c.String(maxLength: 128));
            CreateIndex("dbo.Files", "LocationId");
            AddForeignKey("dbo.Files", "LocationId", "dbo.Locations", "LocationId");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Files", "LocationId", "dbo.Locations");
            DropIndex("dbo.Files", new[] { "LocationId" });
            AlterColumn("dbo.Files", "LocationId", c => c.String(nullable: false, maxLength: 128));
            CreateIndex("dbo.Files", "LocationId");
            AddForeignKey("dbo.Files", "LocationId", "dbo.Locations", "LocationId", cascadeDelete: true);
        }
    }
}

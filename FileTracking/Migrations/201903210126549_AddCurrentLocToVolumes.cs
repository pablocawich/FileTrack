namespace FileTracking.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddCurrentLocToVolumes : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.FileVolumes", "CurrentLocation", c => c.Byte(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.FileVolumes", "CurrentLocation");
        }
    }
}

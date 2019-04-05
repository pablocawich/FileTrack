namespace FileTracking.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddDefaultValueToVolume : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Files", "Volume", c => c.Byte(nullable: false, defaultValue: 1));
        }
        
        public override void Down()
        {
        }
    }
}

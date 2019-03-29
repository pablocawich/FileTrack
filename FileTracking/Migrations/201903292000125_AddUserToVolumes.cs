namespace FileTracking.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddUserToVolumes : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.FileVolumes", "AdUserId", c => c.Int());
            CreateIndex("dbo.FileVolumes", "AdUserId");
            AddForeignKey("dbo.FileVolumes", "AdUserId", "dbo.AdUsers", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.FileVolumes", "AdUserId", "dbo.AdUsers");
            DropIndex("dbo.FileVolumes", new[] { "AdUserId" });
            DropColumn("dbo.FileVolumes", "AdUserId");
        }
    }
}

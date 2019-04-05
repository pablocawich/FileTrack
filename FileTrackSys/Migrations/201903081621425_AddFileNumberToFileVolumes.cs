namespace FileTracking.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddFileNumberToFileVolumes : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.FileVolumes", "FileNumber", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.FileVolumes", "FileNumber");
        }
    }
}

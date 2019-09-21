namespace FileTracking.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateFileStreetLength : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Files", "Street", c => c.String(nullable: false, maxLength: 250));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Files", "Street", c => c.String(nullable: false, maxLength: 64));
        }
    }
}

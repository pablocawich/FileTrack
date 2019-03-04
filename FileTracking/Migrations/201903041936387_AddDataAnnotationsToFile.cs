namespace FileTracking.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddDataAnnotationsToFile : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Files", "FirstName", c => c.String(nullable: false, maxLength: 54));
            AlterColumn("dbo.Files", "LastName", c => c.String(nullable: false, maxLength: 54));
            AlterColumn("dbo.Files", "MiddleName", c => c.String(nullable: false, maxLength: 54));
            AlterColumn("dbo.Files", "Street", c => c.String(nullable: false, maxLength: 64));
            AlterColumn("dbo.Files", "CityOrTown", c => c.String(nullable: false, maxLength: 82));
            AlterColumn("dbo.Districts", "District", c => c.String(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Districts", "District", c => c.String());
            AlterColumn("dbo.Files", "CityOrTown", c => c.String());
            AlterColumn("dbo.Files", "Street", c => c.String());
            AlterColumn("dbo.Files", "MiddleName", c => c.String());
            AlterColumn("dbo.Files", "LastName", c => c.String());
            AlterColumn("dbo.Files", "FirstName", c => c.String());
        }
    }
}

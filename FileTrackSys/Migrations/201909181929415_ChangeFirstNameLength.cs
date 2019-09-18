namespace FileTracking.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ChangeFirstNameLength : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Files", "FirstName", c => c.String(nullable: false, maxLength: 100));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Files", "FirstName", c => c.String(nullable: false, maxLength: 54));
        }
    }
}

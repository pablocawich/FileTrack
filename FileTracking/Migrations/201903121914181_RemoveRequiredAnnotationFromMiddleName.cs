namespace FileTracking.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveRequiredAnnotationFromMiddleName : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Files", "MiddleName", c => c.String(maxLength: 54));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Files", "MiddleName", c => c.String(nullable: false, maxLength: 54));
        }
    }
}

namespace FileTracking.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddFullNameToFilesTable : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Files", "FullName", c => c.String(maxLength: 100));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Files", "FullName");
        }
    }
}

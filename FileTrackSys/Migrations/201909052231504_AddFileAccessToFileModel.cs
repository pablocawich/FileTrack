namespace FileTracking.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddFileAccessToFileModel : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Files", "FileAccess", c => c.String(maxLength: 200));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Files", "FileAccess");
        }
    }
}

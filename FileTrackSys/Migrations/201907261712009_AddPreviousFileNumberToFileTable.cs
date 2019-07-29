namespace FileTracking.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddPreviousFileNumberToFileTable : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Files", "PreviousFileNumber", c => c.String(maxLength: 94));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Files", "PreviousFileNumber");
        }
    }
}

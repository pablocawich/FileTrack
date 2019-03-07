namespace FileTracking.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveCompositeKeyFromFileNumber : DbMigration
    {
        public override void Up()
        {
            DropPrimaryKey("dbo.Files");
            AlterColumn("dbo.Files", "Id", c => c.Int(nullable: false, identity: true));
            AddPrimaryKey("dbo.Files", "Id");
        }
        
        public override void Down()
        {
            DropPrimaryKey("dbo.Files");
            AlterColumn("dbo.Files", "Id", c => c.Int(nullable: false));
            AddPrimaryKey("dbo.Files", new[] { "Id", "FileNumber" });
        }
    }
}

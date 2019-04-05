namespace FileTracking.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddCompositeKeyAndVolumeColumn : DbMigration
    {
        public override void Up()
        {
            DropPrimaryKey("dbo.Files");
            AddColumn("dbo.Files", "FileNumber", c => c.Int(nullable: false));
            AddColumn("dbo.Files", "Volume", c => c.Byte(nullable: false));
            AlterColumn("dbo.Files", "Id", c => c.Int(nullable: false));
            AddPrimaryKey("dbo.Files", new[] { "Id", "FileNumber" });
        }
        
        public override void Down()
        {
            DropPrimaryKey("dbo.Files");
            AlterColumn("dbo.Files", "Id", c => c.Int(nullable: false, identity: true));
            DropColumn("dbo.Files", "Volume");
            DropColumn("dbo.Files", "FileNumber");
            AddPrimaryKey("dbo.Files", "Id");
        }
    }
}

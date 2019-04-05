namespace FileTracking.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveUselessFileRules : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Files", "Comments", c => c.String(maxLength: 255));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Files", "Comments", c => c.String());
        }
    }
}

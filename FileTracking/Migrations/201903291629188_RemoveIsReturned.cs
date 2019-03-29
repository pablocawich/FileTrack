namespace FileTracking.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveIsReturned : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.Requests", "IsReturned");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Requests", "IsReturned", c => c.Boolean(nullable: false));
        }
    }
}

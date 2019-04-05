namespace FileTracking.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddIsReturnedToRequest : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Requests", "IsReturned", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Requests", "IsReturned");
        }
    }
}

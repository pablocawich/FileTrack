namespace FileTracking.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddIsConfirmedToRequest : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Requests", "IsConfirmed", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Requests", "IsConfirmed");
        }
    }
}

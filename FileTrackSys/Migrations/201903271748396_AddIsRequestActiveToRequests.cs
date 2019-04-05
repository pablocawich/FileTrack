namespace FileTracking.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddIsRequestActiveToRequests : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Requests", "IsRequestActive", c => c.Boolean(nullable: false, defaultValue: true));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Requests", "IsRequestActive");
        }
    }
}

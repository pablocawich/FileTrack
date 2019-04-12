namespace FileTracking.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddRequestTypeToRequest : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Requests", "RequestTypeId", c => c.String(maxLength: 32));
            CreateIndex("dbo.Requests", "RequestTypeId");
            AddForeignKey("dbo.Requests", "RequestTypeId", "dbo.RequestTypes", "RequestTypeId");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Requests", "RequestTypeId", "dbo.RequestTypes");
            DropIndex("dbo.Requests", new[] { "RequestTypeId" });
            DropColumn("dbo.Requests", "RequestTypeId");
        }
    }
}

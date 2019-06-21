namespace FileTracking.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddRequestedFromUserColumnToRequest : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Requests", "UserRequestedFromId", c => c.Int());
            CreateIndex("dbo.Requests", "UserRequestedFromId");
            AddForeignKey("dbo.Requests", "UserRequestedFromId", "dbo.AdUsers", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Requests", "UserRequestedFromId", "dbo.AdUsers");
            DropIndex("dbo.Requests", new[] { "UserRequestedFromId" });
            DropColumn("dbo.Requests", "UserRequestedFromId");
        }
    }
}

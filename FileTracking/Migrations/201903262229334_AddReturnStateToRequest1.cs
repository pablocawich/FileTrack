namespace FileTracking.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddReturnStateToRequest1 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Requests", "ReturnStateId", c => c.Byte(nullable: false));
            CreateIndex("dbo.Requests", "ReturnStateId");
            AddForeignKey("dbo.Requests", "ReturnStateId", "dbo.ReturnStates", "Id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Requests", "ReturnStateId", "dbo.ReturnStates");
            DropIndex("dbo.Requests", new[] { "ReturnStateId" });
            DropColumn("dbo.Requests", "ReturnStateId");
        }
    }
}

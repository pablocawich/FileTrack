namespace FileTracking.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddRequestBinderField : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Requests", "RequestBinder", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Requests", "RequestBinder");
        }
    }
}

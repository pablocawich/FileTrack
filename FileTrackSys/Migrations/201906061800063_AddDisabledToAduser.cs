namespace FileTracking.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddDisabledToAduser : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AdUsers", "IsDisabled", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.AdUsers", "IsDisabled");
        }
    }
}

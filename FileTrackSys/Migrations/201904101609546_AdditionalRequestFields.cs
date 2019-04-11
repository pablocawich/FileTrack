namespace FileTracking.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AdditionalRequestFields : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Requests", "RequesteeBranch", c => c.Byte(nullable: false));
            AddColumn("dbo.Requests", "ReturnedDate", c => c.DateTime());
            AddColumn("dbo.Requests", "ReturnAcceptBy", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Requests", "ReturnAcceptBy");
            DropColumn("dbo.Requests", "ReturnedDate");
            DropColumn("dbo.Requests", "RequesteeBranch");
        }
    }
}

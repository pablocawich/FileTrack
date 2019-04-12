namespace FileTracking.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddOtherStateToRequestStatus : DbMigration
    {
        public override void Up()
        {
            Sql("INSERT INTO RequestStatus(Id, Status) VALUES(4, 'Never Received')");
        }
        
        public override void Down()
        {
        }
    }
}

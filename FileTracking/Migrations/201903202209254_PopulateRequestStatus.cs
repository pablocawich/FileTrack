namespace FileTracking.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class PopulateRequestStatus : DbMigration
    {
        public override void Up()
        {
            Sql("INSERT INTO RequestStatus(Id, Status) VALUES(1,'Pending')");
            Sql("INSERT INTO RequestStatus(Id, Status) VALUES(2,'Accepted')");
            Sql("INSERT INTO RequestStatus(Id, Status) VALUES(3,'Denied')");
        }
        
        public override void Down()
        {
        }
    }
}

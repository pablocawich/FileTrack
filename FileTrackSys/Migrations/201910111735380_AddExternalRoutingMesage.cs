namespace FileTracking.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddExternalRoutingMesage : DbMigration
    {
        public override void Up()
        {
            Sql("INSERT INTO Messages(Id,MessageType,MessageText) VALUES('EXROUTE','External Routing','An external branch has checked out a file to a user. Kindly route file to user, upon arrival.')");
        }
        
        public override void Down()
        {
        }
    }
}

namespace FileTracking.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class PopulateRequestType : DbMigration
    {
        public override void Up()
        {
            Sql("INSERT INTO RequestTypes(RequestTypeId, Type) VALUES('INREQ','Internal Request')");
            Sql("INSERT INTO RequestTypes(RequestTypeId, Type) VALUES('EXREQ','External Request')");
        }
        
        public override void Down()
        {
        }
    }
}

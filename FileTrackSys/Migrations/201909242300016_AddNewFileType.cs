namespace FileTracking.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddNewFileType : DbMigration
    {
        public override void Up()
        {
            Sql("INSERT INTO RequestTypes(RequestTypeId, Type) VALUES('DITFR','Direct Transfer')");
        }
        
        public override void Down()
        {
        }
    }
}

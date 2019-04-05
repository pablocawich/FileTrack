namespace FileTracking.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class PopulateFileStatus : DbMigration
    {
        public override void Up()
        {
            Sql("INSERT INTO FileStatus (Id,Status) VALUES (1, 'Active')");
            Sql("INSERT INTO FileStatus (Id,Status) VALUES (2, 'Repaid')");
            Sql("INSERT INTO FileStatus (Id,Status) VALUES (3, 'Closed')");
            Sql("INSERT INTO FileStatus (Id,Status) VALUES (4, 'Cancelled')");
            Sql("INSERT INTO FileStatus (Id,Status) VALUES (5, 'Not taken up')");
        }
        
        public override void Down()
        {
        }
    }
}

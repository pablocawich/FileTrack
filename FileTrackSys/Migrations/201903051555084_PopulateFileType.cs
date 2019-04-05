namespace FileTracking.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class PopulateFileType : DbMigration
    {
        public override void Up()
        {
            Sql("INSERT INTO FileTypes(Id, Type) VALUES (1, 'Regular Loan')");
            Sql("INSERT INTO FileTypes(Id, Type) VALUES (2, 'Staff Loan')");
            Sql("INSERT INTO FileTypes(Id, Type) VALUES (3, 'Managers Only')");
        }
        
        public override void Down()
        {
        }
    }
}

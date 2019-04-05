namespace FileTracking.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class PopulateDistricts : DbMigration
    {
        public override void Up()
        {
            Sql("INSERT INTO Districts ( District) VALUES ('Corozal')");
            Sql("INSERT INTO Districts ( District) VALUES ('Orange Walk')");
            Sql("INSERT INTO Districts ( District) VALUES ('Belize')");
            Sql("INSERT INTO Districts ( District) VALUES ('Cayo')");
            Sql("INSERT INTO Districts ( District) VALUES ('Stann Creek')");
            Sql("INSERT INTO Districts (District) VALUES ('Toledo')");
        }
        
        public override void Down()
        {
        }
    }
}

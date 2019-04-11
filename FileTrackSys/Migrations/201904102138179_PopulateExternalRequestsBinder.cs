namespace FileTracking.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class PopulateExternalRequestsBinder : DbMigration
    {
        public override void Up()
        {
            Sql("INSERT INTO ExternalRequestsBinders(Id, CurrentNumberBinder) VALUES (1, 10)");
        }
        
        public override void Down()
        {
        }
    }
}

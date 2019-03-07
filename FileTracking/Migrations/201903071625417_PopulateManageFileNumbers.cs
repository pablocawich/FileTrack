namespace FileTracking.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class PopulateManageFileNumbers : DbMigration
    {
        public override void Up()
        {
            Sql("INSERT INTO ManageFileNumbers(Id, CurrentFileNumber) VALUES (1, 9999)");
        }
        
        public override void Down()
        {
        }
    }
}

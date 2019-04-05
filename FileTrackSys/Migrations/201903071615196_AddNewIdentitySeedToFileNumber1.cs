namespace FileTracking.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddNewIdentitySeedToFileNumber1 : DbMigration
    {
        public override void Up()
        {
            Sql("DBCC CHECKIDENT(Files, RESEED, 1)");
        }
        
        public override void Down()
        {
        }
    }
}

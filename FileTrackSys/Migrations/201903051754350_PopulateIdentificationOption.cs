namespace FileTracking.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class PopulateIdentificationOption : DbMigration
    {
        public override void Up()
        {
            Sql("INSERT INTO IdentificationOptions(Id, Identification) VALUES(1, 'Social-Security')");
            Sql("INSERT INTO IdentificationOptions(Id, Identification) VALUES(2, 'Passport')");
            Sql("INSERT INTO IdentificationOptions(Id, Identification) VALUES(3, 'Voter ID')");
            Sql("INSERT INTO IdentificationOptions(Id, Identification) VALUES(4, 'Driver License')");
        }
        
        public override void Down()
        {
        }
    }
}

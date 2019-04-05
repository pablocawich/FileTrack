namespace FileTracking.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class PopulateStatesTable : DbMigration
    {
        public override void Up()
        {
            Sql("INSERT INTO States(Id, State) VALUES(1, 'Stored')");
            Sql("INSERT INTO States(Id, State) VALUES(2, 'Requested')");
            Sql("INSERT INTO States(Id, State) VALUES(3, 'Ready')");
            Sql("INSERT INTO States(Id, State) VALUES(4, 'Transfer')");
            Sql("INSERT INTO States(Id, State) VALUES(5, 'Checked Out')");
        }
        
        public override void Down()
        {

        }
    }
}

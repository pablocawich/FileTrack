namespace FileTracking.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class PopulateReturnState : DbMigration
    {
        public override void Up()
        {
            Sql("INSERT INTO ReturnStates(Id, State) VALUES(1,'Return Idle')");
            Sql("INSERT INTO ReturnStates(Id, State) VALUES(2,'Return Sent')");
            Sql("INSERT INTO ReturnStates(Id, State) VALUES(3,'Return Accepted')");
        }
        
        public override void Down()
        {
            
        }
    }
}

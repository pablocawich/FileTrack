namespace FileTracking.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddExtRequestMessageToMssgDb : DbMigration
    {
        public override void Up()
        {
            Sql("INSERT INTO Messages(Id,MessageType,MessageText) VALUES('XPENDING','External Request','An external user is requesting a file. Kindly address.')");
        }
        
        public override void Down()
        {
        }
    }
}

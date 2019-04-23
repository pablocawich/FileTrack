namespace FileTracking.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class PopulateMessageTable : DbMigration
    {
        public override void Up()
        {
            Sql("INSERT INTO Messages(Id,MessageType,MessageText) VALUES('REJ','Denial','File request has been rejected')");
            Sql("INSERT INTO Messages(Id,MessageType,MessageText) VALUES('ACC','Accepted','File request has been accepted. Further confirm checkout.')");
            Sql("INSERT INTO Messages(Id,MessageType,MessageText) VALUES('RET','Returning','User is returning file. Kindly Check in when the file arrives')");
            Sql("INSERT INTO Messages(Id,MessageType,MessageText) VALUES('RET_ACC','Accept Return','Registry confirmed you check in (return)')");
            Sql("INSERT INTO Messages(Id,MessageType,MessageText) VALUES('EX_ACC','External Accept','Your external file request has been accepted by its respective branch registry')");
            Sql("INSERT INTO Messages(Id,MessageType,MessageText) VALUES('EX_REJ','External Denial','Your external file request has been rejected by its respective branch registry')");
            Sql("INSERT INTO Messages(Id,MessageType,MessageText) VALUES('EX_RET','External Return','External registry is returning file. Kindly accept when package arrives')");
            Sql("INSERT INTO Messages(Id,MessageType,MessageText) VALUES('ExRetAcc','External Return Approval','Your external return has been accepted by registry')");
        }
        
        public override void Down()
        {
        }
    }
}

namespace FileTracking.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddNewMessageData : DbMigration
    {
        public override void Up()
        {
            Sql("INSERT INTO Messages(Id,MessageType,MessageText) VALUES('TRANREQ','Transfer Request','A user is requesting a file transfer')");
            Sql("INSERT INTO Messages(Id,MessageType,MessageText) VALUES('TRANACC','Transfer Accepted','Your transfer request has been accepted. Kindly retrieve file.')");
            Sql("INSERT INTO Messages(Id,MessageType,MessageText) VALUES('TRANREJ','Transfer Denied','Your transfer request has been rejected.')");
            //Direct Transfer Messaage
            Sql("INSERT INTO Messages(Id,MessageType,MessageText) VALUES('DIRTREQ','Direct Request','A User wants to transfer a file to you. Kindly confirm.')");
            Sql("INSERT INTO Messages(Id,MessageType,MessageText) VALUES('DIRTACC','Direct Accepted','The user has accepted the direct transfer.')");
            Sql("INSERT INTO Messages(Id,MessageType,MessageText) VALUES('DIRTREJ','Direct Denied','The user has declined your direct transfer. Return file to registry.')");
            //for pending files
            Sql("INSERT INTO Messages(Id,MessageType,MessageText) VALUES('PENDING','Check out request','you have a file pending a check out approval. Kindly look into.')");
        }
        
        public override void Down()
        {
        }
    }
}

namespace FileTracking.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddRegistryTransferMessage : DbMigration
    {
        public override void Up()
        {
            Sql("INSERT INTO Messages (Id, MessageType, MessageText) VALUES ('REG_TRAN','Registry Transfer','Registry is transferring a file to you. Kindly Confirm')");
        }
        
        public override void Down()
        {
        }
    }
}

namespace FileTracking.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddNotificationTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Notifications",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        SenderUserId = c.Int(),
                        RecipientUserId = c.Int(),
                        MessageId = c.String(maxLength: 8),
                        RequestId = c.Int(),
                        DateTriggered = c.DateTime(),
                        Read = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Messages", t => t.MessageId)
                .ForeignKey("dbo.AdUsers", t => t.RecipientUserId)
                .ForeignKey("dbo.Requests", t => t.RequestId)
                .ForeignKey("dbo.AdUsers", t => t.SenderUserId)
                .Index(t => t.SenderUserId)
                .Index(t => t.RecipientUserId)
                .Index(t => t.MessageId)
                .Index(t => t.RequestId);
            
            CreateTable(
                "dbo.Messages",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 8),
                        MessageType = c.String(maxLength: 24),
                        MessageText = c.String(maxLength: 255),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Notifications", "SenderUserId", "dbo.AdUsers");
            DropForeignKey("dbo.Notifications", "RequestId", "dbo.Requests");
            DropForeignKey("dbo.Notifications", "RecipientUserId", "dbo.AdUsers");
            DropForeignKey("dbo.Notifications", "MessageId", "dbo.Messages");
            DropIndex("dbo.Notifications", new[] { "RequestId" });
            DropIndex("dbo.Notifications", new[] { "MessageId" });
            DropIndex("dbo.Notifications", new[] { "RecipientUserId" });
            DropIndex("dbo.Notifications", new[] { "SenderUserId" });
            DropTable("dbo.Messages");
            DropTable("dbo.Notifications");
        }
    }
}

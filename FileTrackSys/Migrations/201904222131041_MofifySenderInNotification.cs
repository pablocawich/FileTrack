namespace FileTracking.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class MofifySenderInNotification : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Notifications", "SenderUserId", "dbo.AdUsers");
            DropIndex("dbo.Notifications", new[] { "SenderUserId" });
            AddColumn("dbo.Notifications", "SenderUser", c => c.String(maxLength: 32));
            DropColumn("dbo.Notifications", "SenderUserId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Notifications", "SenderUserId", c => c.Int());
            DropColumn("dbo.Notifications", "SenderUser");
            CreateIndex("dbo.Notifications", "SenderUserId");
            AddForeignKey("dbo.Notifications", "SenderUserId", "dbo.AdUsers", "Id");
        }
    }
}

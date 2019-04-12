namespace FileTracking.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddRequestTypeTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.RequestTypes",
                c => new
                    {
                        RequestTypeId = c.String(nullable: false, maxLength: 32),
                        Type = c.String(nullable: false, maxLength: 32),
                    })
                .PrimaryKey(t => t.RequestTypeId);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.RequestTypes");
        }
    }
}

namespace FileTracking.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddExternalRequestsBinder : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ExternalRequestsBinders",
                c => new
                    {
                        Id = c.Byte(nullable: false),
                        CurrentNumberBinder = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.ExternalRequestsBinders");
        }
    }
}

namespace FileTracking.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddManageFileNumbers : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ManageFileNumbers",
                c => new
                    {
                        Id = c.Byte(nullable: false),
                        CurrentFileNumber = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.ManageFileNumbers");
        }
    }
}

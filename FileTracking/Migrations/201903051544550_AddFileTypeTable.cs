namespace FileTracking.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddFileTypeTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.FileTypes",
                c => new
                    {
                        Id = c.Byte(nullable: false),
                        Type = c.String(nullable: false, maxLength: 24),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.FileTypes");
        }
    }
}

namespace FileTracking.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddAdUser : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.AdUsers",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Email = c.String(),
                        Username = c.String(),
                        Role = c.String(),
                        BranchId = c.Byte(nullable: false),
                        Branches_Id = c.Byte(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Branches", t => t.Branches_Id)
                .Index(t => t.Branches_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AdUsers", "Branches_Id", "dbo.Branches");
            DropIndex("dbo.AdUsers", new[] { "Branches_Id" });
            DropTable("dbo.AdUsers");
        }
    }
}

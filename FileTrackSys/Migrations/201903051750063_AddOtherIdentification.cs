namespace FileTracking.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddOtherIdentification : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.OtherIdentifications",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        IdentificationOptionId = c.Byte(nullable: false),
                        IdNumber = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.IdentificationOptions", t => t.IdentificationOptionId, cascadeDelete: true)
                .Index(t => t.IdentificationOptionId);
            
            CreateTable(
                "dbo.IdentificationOptions",
                c => new
                    {
                        Id = c.Byte(nullable: false),
                        Identification = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.Files", "OtherIdentificationId", c => c.Int(nullable: false));
            CreateIndex("dbo.Files", "OtherIdentificationId");
            AddForeignKey("dbo.Files", "OtherIdentificationId", "dbo.OtherIdentifications", "Id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Files", "OtherIdentificationId", "dbo.OtherIdentifications");
            DropForeignKey("dbo.OtherIdentifications", "IdentificationOptionId", "dbo.IdentificationOptions");
            DropIndex("dbo.OtherIdentifications", new[] { "IdentificationOptionId" });
            DropIndex("dbo.Files", new[] { "OtherIdentificationId" });
            DropColumn("dbo.Files", "OtherIdentificationId");
            DropTable("dbo.IdentificationOptions");
            DropTable("dbo.OtherIdentifications");
        }
    }
}

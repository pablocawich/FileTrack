namespace FileTracking.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddReturnStateToRequest : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ReturnStates",
                c => new
                    {
                        Id = c.Byte(nullable: false),
                        State = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.ReturnStates");
        }
    }
}

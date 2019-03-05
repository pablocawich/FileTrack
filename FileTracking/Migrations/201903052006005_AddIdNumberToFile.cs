namespace FileTracking.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddIdNumberToFile : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Files", "IdentificationNumber", c => c.String(maxLength: 64));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Files", "IdentificationNumber");
        }
    }
}

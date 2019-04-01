namespace FileTracking.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddLoanNumber : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Files", "LoanNumber", c => c.String(maxLength: 164));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Files", "LoanNumber");
        }
    }
}

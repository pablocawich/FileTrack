namespace FileTracking.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class PopulateBranchesTable : DbMigration
    {
        public override void Up()
        {
            Sql("INSERT INTO Branches(Id,Branch,DistrictsId) VALUES (1, 'Corozal', 1)");
            Sql("INSERT INTO Branches(Id,Branch,DistrictsId) VALUES (2, 'Orange Walk', 2)");
            Sql("INSERT INTO Branches(Id,Branch,DistrictsId) VALUES (3, 'Belize City', 3)");
            Sql("INSERT INTO Branches(Id,Branch,DistrictsId) VALUES (4, 'San Pedro', 3)");
            Sql("INSERT INTO Branches(Id,Branch,DistrictsId) VALUES (5, 'Belmopan', 4)");
            Sql("INSERT INTO Branches(Id,Branch,DistrictsId) VALUES (6, 'Dangriga', 5)");
        }
        
        public override void Down()
        {
        }
    }
}

namespace ePatria.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddWalktroughIDToRCM : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.RiskControlMatrices", "WalktroughID", c => c.Int(nullable: false));
            CreateIndex("dbo.RiskControlMatrices", "WalktroughID");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.RiskControlMatrices", "WalktroughID", "dbo.Walktroughs");
            DropIndex("dbo.RiskControlMatrices", new[] { "WalktroughID" });
            DropColumn("dbo.RiskControlMatrices", "WalktroughID");
        }
    }
}

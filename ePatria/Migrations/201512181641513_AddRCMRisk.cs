namespace ePatria.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddRCMRisk : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.RCMRisks",
                c => new
                {
                    RCMRiskID = c.Int(nullable: false, identity: true),
                    RiskControlMatrixID = c.Int(),
                    Nama = c.String(),
                    Keterangan = c.String(),
                })
                .PrimaryKey(t => t.RCMRiskID)
                .ForeignKey("dbo.RiskControlMatrices", t => t.RiskControlMatrixID)
                .Index(t => t.RCMRiskID)
                .Index(t => t.RiskControlMatrixID);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.RCMRisks", "RiskControlMatrixID", "dbo.RiskControlMatrices");
            DropIndex("dbo.RCMRisks", new[] { "RCMRiskID" });
            DropIndex("dbo.RCMRisks", new[] { "RiskControlMatrixID" });
            DropTable("dbo.RCMRisks");
        }
    }
}

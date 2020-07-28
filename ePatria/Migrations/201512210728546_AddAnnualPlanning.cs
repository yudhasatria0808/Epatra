namespace ePatria.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddAnnualPlanning : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.AnnualPlannings", "ReviewRelationMasterID", "dbo.ReviewRelationMasters");
            DropIndex("dbo.AnnualPlannings", new[] { "ReviewRelationMasterID" });
            AddColumn("dbo.AnnualPlannings", "Type", c => c.String());
            DropColumn("dbo.AnnualPlannings", "ReviewRelationMasterID");
     
        }
        
        public override void Down()
        {
            AddColumn("dbo.AnnualPlannings", "ReviewRelationMasterID", c => c.Int());
            DropColumn("dbo.AnnualPlannings", "Type");
            CreateIndex("dbo.AnnualPlannings", "ReviewRelationMasterID");
            AddForeignKey("dbo.AnnualPlannings", "ReviewRelationMasterID", "dbo.ReviewRelationMasters", "ReviewRelationMasterID");
            
        }

      
    }
}

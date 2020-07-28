namespace ePatria.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateEngagementActivity : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.EngagementActivities", "Date_Start", c => c.String());
            AddColumn("dbo.EngagementActivities", "Date_End", c => c.String());
            AddColumn("dbo.EngagementActivities", "PICID", c => c.Int());
            AddColumn("dbo.EngagementActivities", "SupervisorID", c => c.Int());
            AddColumn("dbo.EngagementActivities", "TeamLeaderID", c => c.Int());
            AddColumn("dbo.EngagementActivities", "MemberID", c => c.Int());
            DropColumn("dbo.AnnualPlannings", "PICID");
            DropColumn("dbo.AnnualPlannings", "SupervisorID");
            DropColumn("dbo.AnnualPlannings", "TeamLeaderID");
            DropColumn("dbo.AnnualPlannings", "MemberID");
        }
        
        public override void Down()
        {
            AddColumn("dbo.AnnualPlannings", "MemberID", c => c.Int(nullable: false));
            AddColumn("dbo.AnnualPlannings", "TeamLeaderID", c => c.Int(nullable: false));
            AddColumn("dbo.AnnualPlannings", "SupervisorID", c => c.Int(nullable: false));
            AddColumn("dbo.AnnualPlannings", "PICID", c => c.Int(nullable: false));
            DropColumn("dbo.EngagementActivities", "MemberID");
            DropColumn("dbo.EngagementActivities", "TeamLeaderID");
            DropColumn("dbo.EngagementActivities", "SupervisorID");
            DropColumn("dbo.EngagementActivities", "PICID");
            DropColumn("dbo.EngagementActivities", "Date_End");
            DropColumn("dbo.EngagementActivities", "Date_Start");
        }
    }
}

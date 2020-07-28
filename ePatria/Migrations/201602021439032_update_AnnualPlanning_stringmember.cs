namespace ePatria.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class update_AnnualPlanning_stringmember : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.EngagementActivities", "PICID", c => c.String());
            AlterColumn("dbo.EngagementActivities", "SupervisorID", c => c.String());
            AlterColumn("dbo.EngagementActivities", "TeamLeaderID", c => c.String());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.EngagementActivities", "TeamLeaderID", c => c.Int());
            AlterColumn("dbo.EngagementActivities", "SupervisorID", c => c.Int());
            AlterColumn("dbo.EngagementActivities", "PICID", c => c.Int());
        }
    }
}

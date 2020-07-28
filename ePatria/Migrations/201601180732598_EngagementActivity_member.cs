namespace ePatria.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class EngagementActivity_member : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.EngagementActivities", "MemberID", c => c.String());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.EngagementActivities", "MemberID", c => c.Int());
        }
    }
}

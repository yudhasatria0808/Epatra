namespace ePatria.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Add : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Preliminaries", "EngagementID", c => c.Int());
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Preliminaries", "EngagementID", "dbo.EngagementActivities");
            DropIndex("dbo.Preliminaries", new[] { "EngagementID" });
            DropColumn("dbo.Preliminaries", "EngagementID");
        }
    }
}

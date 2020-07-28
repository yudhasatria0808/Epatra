namespace ePatria.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Add_ExitMeeting : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ExitMeetings",
                c => new
                    {
                        ExitMeetingID = c.Int(nullable: false, identity: true),
                        ActivityID = c.Int(nullable: false),
                        EngagementID = c.Int(nullable: false),
                        Date_Start = c.DateTime(nullable: false),
                        Date_End = c.DateTime(nullable: false),
                        Status = c.String(),
                        OrganizationID = c.Int(nullable: false),
                        LetterOfCommandID = c.Int(nullable: false),
                        Reviewer1 = c.String(),
                        Reviewer2 = c.String(),
                    })
                .PrimaryKey(t => t.ExitMeetingID);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ExitMeetings", "OrganizationID", "dbo.Organizations");
            DropForeignKey("dbo.ExitMeetings", "EngagementID", "dbo.EngagementActivities");
            DropForeignKey("dbo.ExitMeetings", "ActivityID", "dbo.Activities");
            DropIndex("dbo.ExitMeetings", new[] { "OrganizationID" });
            DropIndex("dbo.ExitMeetings", new[] { "EngagementID" });
            DropIndex("dbo.ExitMeetings", new[] { "ActivityID" });
            DropTable("dbo.ExitMeetings");
        }
    }
}

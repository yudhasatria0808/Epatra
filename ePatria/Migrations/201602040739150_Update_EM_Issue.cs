namespace ePatria.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Update_EM_Issue : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.FieldWorks", "ExitMeeting_ExitMeetingID", c => c.Int());
            AddColumn("dbo.RCMDetailRiskControlIssues", "Signification", c => c.String());
            AddColumn("dbo.RCMDetailRiskControlIssues", "Due_Date", c => c.String());
            AddColumn("dbo.RCMDetailRiskControlIssues", "PICID", c => c.String());
            CreateIndex("dbo.FieldWorks", "ExitMeeting_ExitMeetingID");
            AddForeignKey("dbo.FieldWorks", "ExitMeeting_ExitMeetingID", "dbo.ExitMeetings", "ExitMeetingID");
            DropColumn("dbo.ExitMeetings", "Date_End");
        }
        
        public override void Down()
        {
            AddColumn("dbo.ExitMeetings", "Date_End", c => c.DateTime(nullable: false));
            DropForeignKey("dbo.FieldWorks", "ExitMeeting_ExitMeetingID", "dbo.ExitMeetings");
            DropIndex("dbo.FieldWorks", new[] { "ExitMeeting_ExitMeetingID" });
            DropColumn("dbo.RCMDetailRiskControlIssues", "PICID");
            DropColumn("dbo.RCMDetailRiskControlIssues", "Due_Date");
            DropColumn("dbo.RCMDetailRiskControlIssues", "Signification");
            DropColumn("dbo.FieldWorks", "ExitMeeting_ExitMeetingID");
        }
    }
}

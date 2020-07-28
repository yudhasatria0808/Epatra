namespace ePatria.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class APM_updatedate : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.AuditPlanningMemorandums", "EntryMeetingDateEnd");
            DropColumn("dbo.AuditPlanningMemorandums", "ExitMeetingDateEnd");
            DropColumn("dbo.AuditPlanningMemorandums", "LHADateEnd");
        }
        
        public override void Down()
        {
            AddColumn("dbo.AuditPlanningMemorandums", "LHADateEnd", c => c.DateTime(nullable: false));
            AddColumn("dbo.AuditPlanningMemorandums", "ExitMeetingDateEnd", c => c.DateTime(nullable: false));
            AddColumn("dbo.AuditPlanningMemorandums", "EntryMeetingDateEnd", c => c.DateTime(nullable: false));
        }
    }
}

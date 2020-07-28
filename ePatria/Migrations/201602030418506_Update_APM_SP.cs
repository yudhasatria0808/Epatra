namespace ePatria.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Update_APM_SP : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.AuditPlanningMemorandums", "PICID", c => c.String());
            AlterColumn("dbo.AuditPlanningMemorandums", "SupervisorID", c => c.String());
            AlterColumn("dbo.AuditPlanningMemorandums", "TeamLeaderID", c => c.String());
            AlterColumn("dbo.LetterOfCommands", "PICID", c => c.String());
            AlterColumn("dbo.LetterOfCommands", "SupervisorID", c => c.String());
            AlterColumn("dbo.LetterOfCommands", "TeamLeaderID", c => c.String());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.LetterOfCommands", "TeamLeaderID", c => c.Int(nullable: false));
            AlterColumn("dbo.LetterOfCommands", "SupervisorID", c => c.Int(nullable: false));
            AlterColumn("dbo.LetterOfCommands", "PICID", c => c.Int(nullable: false));
            AlterColumn("dbo.AuditPlanningMemorandums", "TeamLeaderID", c => c.Int(nullable: false));
            AlterColumn("dbo.AuditPlanningMemorandums", "SupervisorID", c => c.Int(nullable: false));
            AlterColumn("dbo.AuditPlanningMemorandums", "PICID", c => c.Int(nullable: false));
        }
    }
}

namespace ePatria.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class APM_member : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.AuditPlanningMemorandums", "MemberID", c => c.String());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.AuditPlanningMemorandums", "MemberID", c => c.Int(nullable: false));
        }
    }
}

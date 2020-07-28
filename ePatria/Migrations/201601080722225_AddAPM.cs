namespace ePatria.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddAPM : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AuditPlanningMemorandums", "Kesimpulan", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.AuditPlanningMemorandums", "Kesimpulan");
        }
    }
}

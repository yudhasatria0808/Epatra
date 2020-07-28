namespace ePatria.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateAnnualPlanning : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Employees", "Type", c => c.String(nullable: false));
            AlterColumn("dbo.Employees", "Name", c => c.String(nullable: false));
            AlterColumn("dbo.Employees", "NoPEK", c => c.String(nullable: false));
            AlterColumn("dbo.Employees", "Email", c => c.String(nullable: false));
            AlterColumn("dbo.Employees", "Status", c => c.String(nullable: false));
            AlterColumn("dbo.Organizations", "Name", c => c.String(nullable: false));
            AlterColumn("dbo.Positions", "PositionName", c => c.String(nullable: false));
            //DropColumn("dbo.AnnualPlannings", "Date_Start");
            //DropColumn("dbo.AnnualPlannings", "Date_End");
        }
        
        public override void Down()
        {
            //AddColumn("dbo.AnnualPlannings", "Date_End", c => c.DateTime(nullable: false));
            //AddColumn("dbo.AnnualPlannings", "Date_Start", c => c.DateTime(nullable: false));
            AlterColumn("dbo.Positions", "PositionName", c => c.String());
            AlterColumn("dbo.Organizations", "Name", c => c.String());
            AlterColumn("dbo.Employees", "Status", c => c.String());
            AlterColumn("dbo.Employees", "Email", c => c.String());
            AlterColumn("dbo.Employees", "NoPEK", c => c.String());
            AlterColumn("dbo.Employees", "Name", c => c.String());
            AlterColumn("dbo.Employees", "Type", c => c.String());
        }
    }
}

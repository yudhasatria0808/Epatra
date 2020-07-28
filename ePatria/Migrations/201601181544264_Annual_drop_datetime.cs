namespace ePatria.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Annual_drop_datetime : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.AnnualPlannings", "Date_Start");
            DropColumn("dbo.AnnualPlannings", "Date_End");
        }
        
        public override void Down()
        {
            AddColumn("dbo.AnnualPlannings", "Date_End", c => c.DateTime(nullable: false));
            AddColumn("dbo.AnnualPlannings", "Date_Start", c => c.DateTime(nullable: false));
        }
    }
}

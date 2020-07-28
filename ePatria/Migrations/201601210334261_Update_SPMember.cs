namespace ePatria.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Update_SPMember : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.LetterOfCommands", "MemberID", c => c.String());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.LetterOfCommands", "MemberID", c => c.Int(nullable: false));
        }
    }
}

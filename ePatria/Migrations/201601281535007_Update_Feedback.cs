namespace ePatria.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Update_Feedback : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.FeedbackQuestionDetails", "LetterofCommandID", c => c.Int(nullable: false));
            AddColumn("dbo.FeedbackQuestionDetails", "Questioner_QuestionerID", c => c.Int());
            AlterColumn("dbo.FeedbackQuestionDetails", "QuestionerID", c => c.String());
            AlterColumn("dbo.FeedbackQuestionDetails", "PICID", c => c.String());
            CreateIndex("dbo.FeedbackQuestionDetails", "LetterofCommandID");
            CreateIndex("dbo.FeedbackQuestionDetails", "Questioner_QuestionerID");
            AddForeignKey("dbo.FeedbackQuestionDetails", "LetterofCommandID", "dbo.LetterOfCommands", "LetterOfCommandID", cascadeDelete: true);
            AddForeignKey("dbo.FeedbackQuestionDetails", "Questioner_QuestionerID", "dbo.Questioners", "QuestionerID");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.FeedbackQuestionDetails", "Questioner_QuestionerID", "dbo.Questioners");
            DropForeignKey("dbo.FeedbackQuestionDetails", "LetterofCommandID", "dbo.LetterOfCommands");
            DropIndex("dbo.FeedbackQuestionDetails", new[] { "Questioner_QuestionerID" });
            DropIndex("dbo.FeedbackQuestionDetails", new[] { "LetterofCommandID" });
            AlterColumn("dbo.FeedbackQuestionDetails", "PICID", c => c.Int(nullable: false));
            AlterColumn("dbo.FeedbackQuestionDetails", "QuestionerID", c => c.Int(nullable: false));
            DropColumn("dbo.FeedbackQuestionDetails", "Questioner_QuestionerID");
            DropColumn("dbo.FeedbackQuestionDetails", "LetterofCommandID");
        }
    }
}

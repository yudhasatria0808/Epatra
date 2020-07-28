using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity.Infrastructure;
using System.Web.Mvc;
using System.Text.RegularExpressions;
using System.ComponentModel.DataAnnotations.Schema;

namespace ePatria.Models
{
    public class ePatriaDefault : DbContext
    {
        public DbSet<Organization> Organizations { get; set; }
        public DbSet<Position> Positions { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Activity> Activities { get; set; }
        public DbSet<AnnualPlanning> AnnualPlannings { get; set; }
        public DbSet<Preliminary> Preliminaries { get; set; }
        public DbSet<Questioner> Questioners { get; set; }
        public DbSet<LetterOfCommand> LetterOfCommands { get; set; }
        public DbSet<LetterOfCommandDetailDasar> LetterOfCommandDetailDasars { get; set; }
        public DbSet<LetterOfCommandDetailUntuk> LetterOfCommandDetailUntuks { get; set; }
        public DbSet<LetterOfCommandDetailTembusan> LetterOfCommandDetailTembusans { get; set; }
        public DbSet<AuditPlanningMemorandum> AuditPlanningMemorandums { get; set; }
        public DbSet<Walktrough> Walktroughs { get; set; }
        public DbSet<BusinessProces> BusinessProcess { get; set; }
        public DbSet<NotulenEntryMeeting> NotulenEntryMeetings { get; set; }
        public DbSet<RiskControlMatrix> RiskControlMatrixs { get; set; }
        public DbSet<RCMDetailRisk> RCMDetailRisks { get; set; }
        public DbSet<RCMDetailRiskControl> RCMDetailRiskControls { get; set; }
        public DbSet<RCMDetailControlAuditStep> RCMDetailControlAuditSteps { get; set; }
        public DbSet<FieldWork> FieldWorks { get; set; }
        public DbSet<Reporting> Reportings { get; set; }
        public DbSet<Feedback> Feedbacks { get; set; }
        public DbSet<FeedbackQuestionDetail> FeedbackQuestionDetails { get; set; }
        public DbSet<EmailForm> EmailForms { get; set; }
        public DbSet<PostComment> PostComments { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<ReviewRelationMaster> ReviewRelationMasters { get; set; }
        public DbSet<ReviewRelationDetail> ReviewRelationDetails { get; set; }
        public DbSet<ConsultingRequest> ConsultingRequests { get; set; }
        public DbSet<ConsultingLetterOfCommand> ConsultingLetterOfCommands { get; set; }
        public DbSet<ConsultingLetterOfCommandDetailDasar> ConsultingLetterOfCommandDetailDasars { get; set; }
        public DbSet<ConsultingLetterOfCommandDetailTembusan> ConsultingLetterOfCommandDetailTembusans { get; set; }
        public DbSet<ConsultingLetterOfCommandDetailUntuk> ConsultingLetterOfCommandDetailUntuks { get; set; }
        public DbSet<ConsultingLetterOfCommandMember> ConsultingLetterOfCommandMembers { get; set; }
        public DbSet<ConsultingBusinessProcess> ConsultingBusinessProcess { get; set; }
        public DbSet<ConsultingFeedback> ConsultingFeedbacks { get; set; }
        public DbSet<ConsultingFeedbackQuestionDetail> ConsultingFeedbackQuestionDetails { get; set; }
        public DbSet<ConsultingFieldWork> ConsultingFieldWork { get; set; }
        public DbSet<ConsultingRCMDetailControlAuditStep> ConsultingRCMDetailControlAuditSteps { get; set; }
        public DbSet<ConsultingRCMDetailRisk> ConsultingRCMDetailRisks { get; set; }
        public DbSet<ConsultingRCMDetailRiskControl> ConsultingRCMDetailRiskControls { get; set; }
        public DbSet<ConsultingReporting> ConsultingReportings { get; set; }
        public DbSet<ConsultingRiskControlMatrix> ConsultingRiskControlMatrixs { get; set; }
        public DbSet<ConsultingWalktrough> ConsultingWalktroughs { get; set; }
        public DbSet<BPM> BPMs { get; set; }
        public DbSet<ConsultingDraftAgreement> ConsultingDraftAgreements { get; set; }
        public DbSet<EngagementActivity> EngagementActivities { get; set; }
        public DbSet<RCMDetailRiskControlDetail> RCMDetailRiskControlDetail { get; set; }
        public DbSet<AuditQuery> AuditQuery { get; set; }
        public DbSet<RCMDetailRiskControlIssue> RCMDetailRiskControlIssue { get; set; }
        public DbSet<ExitMeeting> ExitMeetings { get; set; }
        public DbSet<UserLoginDetail> UserLoginDetail { get; set; }
        public DbSet<QuestionerAnswers> QuestionerAnswers { get; set; }
        public DbSet<ConsultingRCMDetailRiskControlDetail> ConsultingRCMDetailRiskControlDetail { get; set; }
        public DbSet<ConsultingRCMDetailRiskControlIssue> ConsultingRCMDetailRiskControlIssue { get; set; }
        public DbSet<ConsultingAuditQuery> ConsultingAuditQuery { get; set; }
        public DbSet<AuditTrails> AuditTrails { get; set; }
        public DbSet<MonitoringTindakLanjut> MonitoringTindakLanjut { get; set; }
        public DbSet<Permissions> Permissions { get; set; }
        public DbSet<PermissionRoles> PermissionRoles { get; set; }
        public DbSet<ReportingBabModel> ReportingBabModel { get; set; }
        public DbSet<ListFeedbackSended> ListFeedbackSended { get; set; }
        public DbSet<ListFeedbackSendedConsulting> ListFeedbackSendedConsulting { get; set; }
    }
}
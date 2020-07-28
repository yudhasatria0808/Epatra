namespace ePatria.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class First : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Activities",
                c => new
                {
                    ActivityID = c.Int(nullable: false, identity: true),
                    ActivityParentID = c.Int(),
                    Name = c.String(),
                    Status = c.String(),
                    Status_Active = c.String(),
                    Tahun = c.String(),
                    Description = c.String(),
                    DepartementID = c.Int(),
                })
                .PrimaryKey(t => t.ActivityID);

            CreateTable(
                "dbo.AnnualPlannings",
                c => new
                {
                    AnnualPlanningID = c.Int(nullable: false, identity: true),
                    Date_Start = c.DateTime(nullable: false),
                    Date_End = c.DateTime(nullable: false),
                    Status = c.String(),
                    Approval_Status = c.String(),
                    Tahun = c.String(),
                    ActivityID = c.Int(nullable: false),
                    PICID = c.Int(nullable: false),
                    SupervisorID = c.Int(nullable: false),
                    TeamLeaderID = c.Int(nullable: false),
                    MemberID = c.Int(nullable: false),
                    ReviewRelationMasterID = c.Int(),
                })
                .PrimaryKey(t => t.AnnualPlanningID);

            CreateTable(
                "dbo.ReviewRelationMasters",
                c => new
                {
                    ReviewRelationMasterID = c.Int(nullable: false, identity: true),
                    Description = c.String(),
                })
                .PrimaryKey(t => t.ReviewRelationMasterID);

            CreateTable(
                "dbo.ReviewRelationDetails",
                c => new
                {
                    ReviewRelationDetailID = c.Int(nullable: false, identity: true),
                    ReviewRelationMasterID = c.Int(),
                    PostId = c.Int(),
                })
                .PrimaryKey(t => t.ReviewRelationDetailID)
                .ForeignKey("dbo.Posts", t => t.PostId)
                .ForeignKey("dbo.ReviewRelationMasters", t => t.ReviewRelationMasterID)
                .Index(t => t.ReviewRelationMasterID)
                .Index(t => t.PostId);

            CreateTable(
                "dbo.Posts",
                c => new
                {
                    PostId = c.Int(nullable: false, identity: true),
                    Message = c.String(),
                    PostedBy = c.String(),
                    PostedDate = c.DateTime(nullable: false),
                })
                .PrimaryKey(t => t.PostId);

            CreateTable(
                "dbo.PostComments",
                c => new
                {
                    PostCommentId = c.Int(nullable: false, identity: true),
                    PostId = c.Int(nullable: false),
                    Message = c.String(),
                    CommentedBy = c.String(),
                    CommentedDate = c.DateTime(nullable: false),
                })
                .PrimaryKey(t => t.PostCommentId)
                .ForeignKey("dbo.Posts", t => t.PostId, cascadeDelete: true)
                .Index(t => t.PostId);

            CreateTable(
                "dbo.AuditPlanningMemorandums",
                c => new
                {
                    AuditPlanningMemorandumID = c.Int(nullable: false, identity: true),
                    NomerAPM = c.String(),
                    PreliminaryID = c.Int(nullable: false),
                    Date_Start = c.DateTime(nullable: false),
                    Date_End = c.DateTime(nullable: false),
                    Status = c.String(),
                    ActivityID = c.Int(nullable: false),
                    TujuanAudit = c.String(),
                    RuangLingkupAudit = c.String(),
                    MetodologiAudit = c.String(),
                    DataDanDokumen = c.String(),
                    EntryMeetingDateStart = c.DateTime(nullable: false),
                    EntryMeetingDateEnd = c.DateTime(nullable: false),
                    WalktroughDateStart = c.DateTime(nullable: false),
                    WalktroughDateEnd = c.DateTime(nullable: false),
                    FieldWorkDateStart = c.DateTime(nullable: false),
                    FieldWorkDateEnd = c.DateTime(nullable: false),
                    ExitMeetingDateStart = c.DateTime(nullable: false),
                    ExitMeetingDateEnd = c.DateTime(nullable: false),
                    LHADateStart = c.DateTime(nullable: false),
                    LHADateEnd = c.DateTime(nullable: false),
                    PICID = c.Int(nullable: false),
                    SupervisorID = c.Int(nullable: false),
                    TeamLeaderID = c.Int(nullable: false),
                    MemberID = c.Int(nullable: false),
                    ReviewRelationMasterID = c.Int(),
                })
                .PrimaryKey(t => t.AuditPlanningMemorandumID);

            CreateTable(
                "dbo.Preliminaries",
                c => new
                {
                    PreliminaryID = c.Int(nullable: false, identity: true),
                    Type = c.String(),
                    Status = c.String(),
                    NomorPreliminarySurvey = c.String(),
                    Date_Start = c.DateTime(nullable: false),
                    Date_End = c.DateTime(nullable: false),
                    ActivityID = c.Int(nullable: false),
                    EmployeeID = c.Int(nullable: false),
                    Position_PositionID = c.Int(),
                })
                .PrimaryKey(t => t.PreliminaryID);

            CreateTable(
                "dbo.Employees",
                c => new
                {
                    EmployeeID = c.Int(nullable: false, identity: true),
                    Type = c.String(),
                    Name = c.String(),
                    NoPEK = c.String(),
                    Email = c.String(),
                    PhoneNumber = c.String(),
                    Status = c.String(),
                    OrganizationID = c.Int(nullable: false),
                    PositionID = c.Int(nullable: false),
                })
                .PrimaryKey(t => t.EmployeeID)
                .ForeignKey("dbo.Organizations", t => t.OrganizationID, cascadeDelete: true)
                .ForeignKey("dbo.Positions", t => t.PositionID, cascadeDelete: true)
                .Index(t => t.OrganizationID)
                .Index(t => t.PositionID);

            CreateTable(
                "dbo.Organizations",
                c => new
                {
                    OrganizationID = c.Int(nullable: false, identity: true),
                    OrganizationParentID = c.Int(),
                    Name = c.String(),
                    Description = c.String(),
                })
                .PrimaryKey(t => t.OrganizationID);

            CreateTable(
                "dbo.Positions",
                c => new
                {
                    PositionID = c.Int(nullable: false, identity: true),
                    PositionName = c.String(),
                    JobDesc = c.String(),
                })
                .PrimaryKey(t => t.PositionID);

            CreateTable(
                "dbo.BPMs",
                c => new
                {
                    BPMID = c.Int(nullable: false, identity: true),
                    WalktroughID = c.Int(nullable: false),
                    Name = c.String(),
                    Status = c.String(),
                })
                .PrimaryKey(t => t.BPMID)
                .ForeignKey("dbo.Walktroughs", t => t.WalktroughID, cascadeDelete: true)
                .Index(t => t.WalktroughID);

            CreateTable(
                "dbo.Walktroughs",
                c => new
                {
                    WalktroughID = c.Int(nullable: false, identity: true),
                    LetterOfCommandID = c.Int(nullable: false),
                    ActivityID = c.Int(nullable: false),
                    Date_Start = c.DateTime(nullable: false),
                    Date_End = c.DateTime(nullable: false),
                    Remarks = c.String(),
                    Status = c.String(),
                })
                .PrimaryKey(t => t.WalktroughID);

            CreateTable(
                "dbo.BusinessProces",
                c => new
                {
                    BusinessProcesID = c.Int(nullable: false, identity: true),
                    BPMID = c.Int(nullable: false),
                    WalktroughID = c.Int(),
                    DocumentNo = c.String(),
                    DocumentName = c.String(),
                    FolderName = c.String(),
                })
                .PrimaryKey(t => t.BusinessProcesID);

            CreateTable(
                "dbo.LetterOfCommands",
                c => new
                {
                    LetterOfCommandID = c.Int(nullable: false, identity: true),
                    NomorSP = c.String(),
                    Status = c.String(),
                    Remarks = c.String(),
                    AssuranceType = c.String(),
                    Date_Start = c.DateTime(nullable: false),
                    Date_End = c.DateTime(nullable: false),
                    Menimbang = c.String(),
                    Penutup = c.String(),
                    PreliminaryID = c.Int(nullable: false),
                    ActivityID = c.Int(nullable: false),
                    PICID = c.Int(nullable: false),
                    SupervisorID = c.Int(nullable: false),
                    TeamLeaderID = c.Int(nullable: false),
                    MemberID = c.Int(nullable: false),
                })
                .PrimaryKey(t => t.LetterOfCommandID);

            CreateTable(
                "dbo.ConsultingBusinessProcesses",
                c => new
                {
                    ConsultingBPMID = c.Int(nullable: false, identity: true),
                    ConsultingWalktroughID = c.Int(nullable: false),
                    DocumentNo = c.String(),
                    DocumentName = c.String(),
                    FolderName = c.String(),
                })
                .PrimaryKey(t => t.ConsultingBPMID)
                .ForeignKey("dbo.ConsultingWalktroughs", t => t.ConsultingWalktroughID, cascadeDelete: true)
                .Index(t => t.ConsultingWalktroughID);

            CreateTable(
                "dbo.ConsultingRiskControlMatrices",
                c => new
                {
                    ConsultingRCMID = c.Int(nullable: false, identity: true),
                    ConsultingBPMID = c.Int(nullable: false),
                    SubBusinessProcess = c.String(),
                    Objectives = c.String(),
                })
                .PrimaryKey(t => t.ConsultingRCMID)
                .ForeignKey("dbo.ConsultingBusinessProcesses", t => t.ConsultingBPMID, cascadeDelete: true)
                .Index(t => t.ConsultingBPMID);

            CreateTable(
                "dbo.ConsultingWalktroughs",
                c => new
                {
                    ConsultingWalktroughID = c.Int(nullable: false, identity: true),
                    ConsultingSuratPerintahID = c.Int(nullable: false),
                    ActivityID = c.Int(nullable: false),
                    PeriodStartDate = c.DateTime(nullable: false),
                    PeriodEndDate = c.DateTime(nullable: false),
                    Remarks = c.String(),
                    Status = c.String(),
                })
                .PrimaryKey(t => t.ConsultingWalktroughID)
                .ForeignKey("dbo.ConsultingLetterOfCommands", t => t.ConsultingSuratPerintahID, cascadeDelete: true)
                .Index(t => t.ConsultingSuratPerintahID);

            CreateTable(
                "dbo.ConsultingLetterOfCommands",
                c => new
                {
                    ConsultingSuratPerintahID = c.Int(nullable: false, identity: true),
                    NomorSP = c.String(),
                    ConsultingRequestID = c.Int(nullable: false),
                    EngagementName = c.String(),
                    Status = c.String(),
                    StartDate = c.DateTime(nullable: false),
                    EndDate = c.DateTime(nullable: false),
                    PicID = c.Int(nullable: false),
                    SupervisorID = c.Int(nullable: false),
                    TeamLeaderID = c.Int(nullable: false),
                    Menimbang = c.String(),
                    Penutup = c.String(),
                })
                .PrimaryKey(t => t.ConsultingSuratPerintahID)
                .ForeignKey("dbo.ConsultingRequests", t => t.ConsultingRequestID, cascadeDelete: true)
                .Index(t => t.ConsultingRequestID);

            CreateTable(
                "dbo.ConsultingRequests",
                c => new
                {
                    ConsultingRequestID = c.Int(nullable: false, identity: true),
                    NoSurat = c.String(),
                    NoRequest = c.String(),
                    RequesterID = c.Int(nullable: false),
                    Date_Start = c.DateTime(nullable: false),
                    Date_End = c.DateTime(nullable: false),
                    EvaluationResult = c.String(),
                    Status = c.String(),
                    ActivityID = c.Int(nullable: false),
                })
                .PrimaryKey(t => t.ConsultingRequestID)
                .ForeignKey("dbo.Activities", t => t.ActivityID, cascadeDelete: true)
                .Index(t => t.ActivityID);

            CreateTable(
                "dbo.ConsultingDraftAgreements",
                c => new
                {
                    ConsultingDraftAgreementID = c.Int(nullable: false, identity: true),
                    NoRequest = c.String(),
                    NoSurat = c.String(),
                    RequesterID = c.Int(nullable: false),
                    Date_Start = c.DateTime(nullable: false),
                    Date_End = c.DateTime(nullable: false),
                    ActivityID = c.Int(nullable: false),
                    Tujuan = c.String(),
                    RuangLingkup = c.String(),
                    Peran = c.String(),
                    Status = c.String(),
                })
                .PrimaryKey(t => t.ConsultingDraftAgreementID)
                .ForeignKey("dbo.Activities", t => t.ActivityID, cascadeDelete: true)
                .Index(t => t.ActivityID);

            CreateTable(
                "dbo.ConsultingFeedbackQuestionDetails",
                c => new
                {
                    ConsultingFeedbackQuestionDetailID = c.Int(nullable: false, identity: true),
                    ConsultingFeedbackID = c.Int(nullable: false),
                    QuestionID = c.Int(nullable: false),
                    PicID = c.Int(nullable: false),
                })
                .PrimaryKey(t => t.ConsultingFeedbackQuestionDetailID)
                .ForeignKey("dbo.ConsultingFeedbacks", t => t.ConsultingFeedbackID, cascadeDelete: true)
                .Index(t => t.ConsultingFeedbackID);

            CreateTable(
                "dbo.ConsultingFeedbacks",
                c => new
                {
                    ConsultingFeedbackID = c.Int(nullable: false, identity: true),
                    ConsultingSuratPerintahID = c.Int(nullable: false),
                })
                .PrimaryKey(t => t.ConsultingFeedbackID)
                .ForeignKey("dbo.ConsultingLetterOfCommands", t => t.ConsultingSuratPerintahID, cascadeDelete: true)
                .Index(t => t.ConsultingSuratPerintahID);

            CreateTable(
                "dbo.ConsultingFieldWorks",
                c => new
                {
                    ConsultingFieldWorkID = c.Int(nullable: false, identity: true),
                    ConsultingSuratPerintahID = c.Int(nullable: false),
                    Desc = c.String(),
                })
                .PrimaryKey(t => t.ConsultingFieldWorkID)
                .ForeignKey("dbo.ConsultingLetterOfCommands", t => t.ConsultingSuratPerintahID, cascadeDelete: true)
                .Index(t => t.ConsultingSuratPerintahID);

            CreateTable(
                "dbo.ConsultingLetterOfCommandDetailDasars",
                c => new
                {
                    ConsultingLetterOfCommandDetailDasarID = c.Int(nullable: false, identity: true),
                    ConsultingSuratPerintahID = c.Int(nullable: false),
                    Dasar = c.String(),
                })
                .PrimaryKey(t => t.ConsultingLetterOfCommandDetailDasarID)
                .ForeignKey("dbo.ConsultingLetterOfCommands", t => t.ConsultingSuratPerintahID, cascadeDelete: true)
                .Index(t => t.ConsultingSuratPerintahID);

            CreateTable(
                "dbo.ConsultingLetterOfCommandDetailTembusans",
                c => new
                {
                    ConsultingLetterOfCommandDetailTembusanID = c.Int(nullable: false, identity: true),
                    ConsultingSuratPerintahID = c.Int(nullable: false),
                    Tembusan = c.String(),
                })
                .PrimaryKey(t => t.ConsultingLetterOfCommandDetailTembusanID)
                .ForeignKey("dbo.ConsultingLetterOfCommands", t => t.ConsultingSuratPerintahID, cascadeDelete: true)
                .Index(t => t.ConsultingSuratPerintahID);

            CreateTable(
                "dbo.ConsultingLetterOfCommandDetailUntuks",
                c => new
                {
                    ConsultingLetterOfCommandDetailUntukID = c.Int(nullable: false, identity: true),
                    ConsultingSuratPerintahID = c.Int(nullable: false),
                    Untuk = c.String(),
                })
                .PrimaryKey(t => t.ConsultingLetterOfCommandDetailUntukID)
                .ForeignKey("dbo.ConsultingLetterOfCommands", t => t.ConsultingSuratPerintahID, cascadeDelete: true)
                .Index(t => t.ConsultingSuratPerintahID);

            CreateTable(
                "dbo.ConsultingLetterOfCommandMembers",
                c => new
                {
                    ConsultingLetterOfCommandMemberID = c.Int(nullable: false, identity: true),
                    ConsultingSuratPerintahID = c.Int(nullable: false),
                    MemberID = c.Int(nullable: false),
                })
                .PrimaryKey(t => t.ConsultingLetterOfCommandMemberID)
                .ForeignKey("dbo.ConsultingLetterOfCommands", t => t.ConsultingSuratPerintahID, cascadeDelete: true)
                .Index(t => t.ConsultingSuratPerintahID);

            CreateTable(
                "dbo.ConsultingRCMDetailControlAuditSteps",
                c => new
                {
                    ConsultingRCMDetailControlAuditStepID = c.Int(nullable: false, identity: true),
                    ConsultingRCMDetailRiskControlID = c.Int(nullable: false),
                    AuditStepName = c.String(),
                    Status = c.String(),
                    WorkDoneDescription = c.String(),
                    WorkDoneResult = c.String(),
                })
                .PrimaryKey(t => t.ConsultingRCMDetailControlAuditStepID)
                .ForeignKey("dbo.ConsultingRCMDetailRiskControls", t => t.ConsultingRCMDetailRiskControlID, cascadeDelete: true)
                .Index(t => t.ConsultingRCMDetailRiskControlID);

            CreateTable(
                "dbo.ConsultingRCMDetailRiskControls",
                c => new
                {
                    ConsultingRCMDetailRiskControlID = c.Int(nullable: false, identity: true),
                    ConsultingRCMDetailRiskID = c.Int(nullable: false),
                    ControlName = c.String(),
                    Status = c.String(),
                    RefNo = c.String(),
                    Title = c.String(),
                    Criteria = c.String(),
                    Impact = c.String(),
                    ImpactValue = c.String(),
                    Cause = c.String(),
                    Recommendation = c.String(),
                    ManagementResponse = c.String(),
                    PICID = c.Int(nullable: false),
                    FindingType = c.String(),
                    DueDate = c.DateTime(nullable: false),
                    CaseCloseBool = c.String(),
                    CloseDate = c.DateTime(nullable: false),
                })
                .PrimaryKey(t => t.ConsultingRCMDetailRiskControlID)
                .ForeignKey("dbo.ConsultingRCMDetailRisks", t => t.ConsultingRCMDetailRiskID, cascadeDelete: true)
                .Index(t => t.ConsultingRCMDetailRiskID);

            CreateTable(
                "dbo.ConsultingRCMDetailRisks",
                c => new
                {
                    ConsultingRCMDetailRiskID = c.Int(nullable: false, identity: true),
                    ConsultingRCMID = c.Int(nullable: false),
                    RiskName = c.String(),
                    Status = c.String(),
                })
                .PrimaryKey(t => t.ConsultingRCMDetailRiskID)
                .ForeignKey("dbo.ConsultingRiskControlMatrices", t => t.ConsultingRCMID, cascadeDelete: true)
                .Index(t => t.ConsultingRCMID);

            CreateTable(
                "dbo.ConsultingReportings",
                c => new
                {
                    ConsultingReportingID = c.Int(nullable: false, identity: true),
                    ConsultingSuratPerintahID = c.Int(nullable: false),
                    ActivityID = c.Int(nullable: false),
                    NoLaporan = c.String(),
                    Kepada = c.String(),
                    Dari = c.String(),
                    Lampiran = c.String(),
                    Perihal = c.String(),
                    Hasil = c.String(),
                })
                .PrimaryKey(t => t.ConsultingReportingID)
                .ForeignKey("dbo.ConsultingLetterOfCommands", t => t.ConsultingSuratPerintahID, cascadeDelete: true)
                .Index(t => t.ConsultingSuratPerintahID);

            CreateTable(
                "dbo.EmailForms",
                c => new
                {
                    ID = c.Int(nullable: false, identity: true),
                    FromName = c.String(nullable: false),
                    FromEmail = c.String(nullable: false),
                    Message = c.String(nullable: false),
                })
                .PrimaryKey(t => t.ID);

            CreateTable(
                "dbo.FeedbackQuestionDetails",
                c => new
                {
                    FeedbackQuestionDetailID = c.Int(nullable: false, identity: true),
                    FeedbackID = c.Int(nullable: false),
                    QuestionerID = c.Int(nullable: false),
                    PICID = c.Int(nullable: false),
                })
                .PrimaryKey(t => t.FeedbackQuestionDetailID)
                .ForeignKey("dbo.Feedbacks", t => t.FeedbackID, cascadeDelete: true)
                .Index(t => t.FeedbackID);

            CreateTable(
                "dbo.Feedbacks",
                c => new
                {
                    FeedbackID = c.Int(nullable: false, identity: true),
                    AuditPlanningMemorandumID = c.Int(nullable: false),
                })
                .PrimaryKey(t => t.FeedbackID)
                .ForeignKey("dbo.AuditPlanningMemorandums", t => t.AuditPlanningMemorandumID, cascadeDelete: true)
                .Index(t => t.AuditPlanningMemorandumID);

            CreateTable(
                "dbo.FieldWorks",
                c => new
                {
                    FieldWorkID = c.Int(nullable: false, identity: true),
                    LetterOfCommandID = c.Int(nullable: false),
                    RiskControlMatrixID = c.Int(nullable: false),
                    Status = c.String(),
                })
                .PrimaryKey(t => t.FieldWorkID);

            CreateTable(
                "dbo.RiskControlMatrices",
                c => new
                {
                    RiskControlMatrixID = c.Int(nullable: false, identity: true),
                    BusinessProcesID = c.Int(nullable: false),
                    SubBusinessProcess = c.Int(),
                    Objectives = c.String(),
                })
                .PrimaryKey(t => t.RiskControlMatrixID);

            CreateTable(
                "dbo.LetterOfCommandDetailDasars",
                c => new
                {
                    ID = c.Int(nullable: false, identity: true),
                    LetterOfCommandID = c.Int(nullable: false),
                    Dasar = c.String(),
                })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.LetterOfCommands", t => t.LetterOfCommandID, cascadeDelete: true)
                .Index(t => t.LetterOfCommandID);

            CreateTable(
                "dbo.LetterOfCommandDetailTembusans",
                c => new
                {
                    ID = c.Int(nullable: false, identity: true),
                    LetterOfCommandID = c.Int(nullable: false),
                    Tembusan = c.String(),
                })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.LetterOfCommands", t => t.LetterOfCommandID, cascadeDelete: true)
                .Index(t => t.LetterOfCommandID);

            CreateTable(
                "dbo.LetterOfCommandDetailUntuks",
                c => new
                {
                    ID = c.Int(nullable: false, identity: true),
                    LetterOfCommandID = c.Int(nullable: false),
                    Untuk = c.String(),
                })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.LetterOfCommands", t => t.LetterOfCommandID, cascadeDelete: true)
                .Index(t => t.LetterOfCommandID);

            CreateTable(
                "dbo.NotulenEntryMeetings",
                c => new
                {
                    NotulenEntryMeetingID = c.Int(nullable: false, identity: true),
                    AuditPlanningMemorandumID = c.Int(nullable: false),
                    Tujuan = c.String(),
                    TimeConsumableStartDate = c.DateTime(nullable: false),
                    TimeConsumableEndDate = c.DateTime(nullable: false),
                    Place = c.String(),
                    EmployeeChairmanID = c.Int(nullable: false),
                    EmployeeModeratorID = c.Int(nullable: false),
                    EmployeeReporterID = c.Int(nullable: false),
                    EmployeeMemberID = c.Int(nullable: false),
                    Opening = c.String(),
                    ExposurePlan = c.String(),
                })
                .PrimaryKey(t => t.NotulenEntryMeetingID);

            CreateTable(
                "dbo.Questioners",
                c => new
                {
                    QuestionerID = c.Int(nullable: false, identity: true),
                    Name = c.String(),
                    Type = c.String(),
                    Value = c.String(),
                    Status = c.String(),
                })
                .PrimaryKey(t => t.QuestionerID);

            CreateTable(
                "dbo.RCMDetailControlAuditSteps",
                c => new
                {
                    RCMDetailControlAuditStepID = c.Int(nullable: false, identity: true),
                    RCMDetailRiskControlID = c.Int(nullable: false),
                    AuditStepName = c.String(),
                    Status = c.String(),
                    WorkDoneDescription = c.String(),
                    WorkDoneResult = c.String(),
                })
                .PrimaryKey(t => t.RCMDetailControlAuditStepID)
                .ForeignKey("dbo.RCMDetailRiskControls", t => t.RCMDetailRiskControlID, cascadeDelete: true)
                .Index(t => t.RCMDetailRiskControlID);

            CreateTable(
                "dbo.RCMDetailRiskControls",
                c => new
                {
                    RCMDetailRiskControlID = c.Int(nullable: false, identity: true),
                    RCMDetailRiskID = c.Int(nullable: false),
                    ControlName = c.String(),
                    Status = c.String(),
                    ReviewMasterID = c.Int(nullable: false),
                    RefNo = c.String(),
                    Title = c.String(),
                    Createria = c.String(),
                    Impact = c.String(),
                    ImpactValue = c.String(),
                    Cause = c.String(),
                    Recommendation = c.String(),
                    ManagementResponse = c.String(),
                    PIC = c.String(),
                    FindingType = c.String(),
                    DueDate = c.DateTime(nullable: false),
                    CaseCloseBool = c.String(),
                    CloseDate = c.DateTime(nullable: false),
                })
                .PrimaryKey(t => t.RCMDetailRiskControlID)
                .ForeignKey("dbo.RCMDetailRisks", t => t.RCMDetailRiskID, cascadeDelete: true)
                .Index(t => t.RCMDetailRiskID);

            CreateTable(
                "dbo.RCMDetailRisks",
                c => new
                {
                    RCMDetailRiskID = c.Int(nullable: false, identity: true),
                    RiskControlMatrixID = c.Int(nullable: false),
                    RiskName = c.String(),
                    Status = c.String(),
                })
                .PrimaryKey(t => t.RCMDetailRiskID)
                .ForeignKey("dbo.RiskControlMatrices", t => t.RiskControlMatrixID, cascadeDelete: true)
                .Index(t => t.RiskControlMatrixID);

            CreateTable(
                "dbo.Reportings",
                c => new
                {
                    ReportingID = c.Int(nullable: false, identity: true),
                    LetterOfCommandID = c.Int(nullable: false),
                    NomorLaporan = c.String(),
                    MemoDescription = c.String(),
                    SummaryDescription = c.String(),
                    Bab01Title = c.String(),
                    Bab01SubTitle = c.String(),
                    Bab01Description = c.String(),
                    Bab02Title = c.String(),
                    Bab02SubTitle = c.String(),
                    Bab02Description = c.String(),
                })
                .PrimaryKey(t => t.ReportingID);

            CreateTable(
                   "dbo.EngagementActivities",
                   c => new
                   {
                       EngagementID = c.Int(nullable: false, identity: true),
                       ActivityID = c.Int(nullable: false),
                       Name = c.String(),
                   })
                   .PrimaryKey(t => t.EngagementID)
                   .ForeignKey("dbo.Activities", t => t.ActivityID, cascadeDelete: true)
                   .Index(t => t.ActivityID);

        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Reportings", "LetterOfCommandID", "dbo.LetterOfCommands");
            DropForeignKey("dbo.RCMDetailControlAuditSteps", "RCMDetailRiskControlID", "dbo.RCMDetailRiskControls");
            DropForeignKey("dbo.RCMDetailRiskControls", "RCMDetailRiskID", "dbo.RCMDetailRisks");
            DropForeignKey("dbo.RCMDetailRisks", "RiskControlMatrixID", "dbo.RiskControlMatrices");
            DropForeignKey("dbo.NotulenEntryMeetings", "AuditPlanningMemorandumID", "dbo.AuditPlanningMemorandums");
            DropForeignKey("dbo.LetterOfCommandDetailUntuks", "LetterOfCommandID", "dbo.LetterOfCommands");
            DropForeignKey("dbo.LetterOfCommandDetailTembusans", "LetterOfCommandID", "dbo.LetterOfCommands");
            DropForeignKey("dbo.LetterOfCommandDetailDasars", "LetterOfCommandID", "dbo.LetterOfCommands");
            DropForeignKey("dbo.FieldWorks", "RiskControlMatrixID", "dbo.RiskControlMatrices");
            DropForeignKey("dbo.RiskControlMatrices", "BusinessProcesID", "dbo.BusinessProces");
            DropForeignKey("dbo.FieldWorks", "LetterOfCommandID", "dbo.LetterOfCommands");
            DropForeignKey("dbo.FeedbackQuestionDetails", "FeedbackID", "dbo.Feedbacks");
            DropForeignKey("dbo.Feedbacks", "AuditPlanningMemorandumID", "dbo.AuditPlanningMemorandums");
            DropForeignKey("dbo.EngagementActivities", "ActivityID", "dbo.Activities");
            DropForeignKey("dbo.ConsultingReportings", "ConsultingSuratPerintahID", "dbo.ConsultingLetterOfCommands");
            DropForeignKey("dbo.ConsultingRCMDetailControlAuditSteps", "ConsultingRCMDetailRiskControlID", "dbo.ConsultingRCMDetailRiskControls");
            DropForeignKey("dbo.ConsultingRCMDetailRiskControls", "ConsultingRCMDetailRiskID", "dbo.ConsultingRCMDetailRisks");
            DropForeignKey("dbo.ConsultingRCMDetailRisks", "ConsultingRCMID", "dbo.ConsultingRiskControlMatrices");
            DropForeignKey("dbo.ConsultingLetterOfCommandMembers", "ConsultingSuratPerintahID", "dbo.ConsultingLetterOfCommands");
            DropForeignKey("dbo.ConsultingLetterOfCommandDetailUntuks", "ConsultingSuratPerintahID", "dbo.ConsultingLetterOfCommands");
            DropForeignKey("dbo.ConsultingLetterOfCommandDetailTembusans", "ConsultingSuratPerintahID", "dbo.ConsultingLetterOfCommands");
            DropForeignKey("dbo.ConsultingLetterOfCommandDetailDasars", "ConsultingSuratPerintahID", "dbo.ConsultingLetterOfCommands");
            DropForeignKey("dbo.ConsultingFieldWorks", "ConsultingSuratPerintahID", "dbo.ConsultingLetterOfCommands");
            DropForeignKey("dbo.ConsultingFeedbackQuestionDetails", "ConsultingFeedbackID", "dbo.ConsultingFeedbacks");
            DropForeignKey("dbo.ConsultingFeedbacks", "ConsultingSuratPerintahID", "dbo.ConsultingLetterOfCommands");
            DropForeignKey("dbo.ConsultingDraftAgreements", "ActivityID", "dbo.Activities");
            DropForeignKey("dbo.ConsultingBusinessProcesses", "ConsultingWalktroughID", "dbo.ConsultingWalktroughs");
            DropForeignKey("dbo.ConsultingWalktroughs", "ConsultingSuratPerintahID", "dbo.ConsultingLetterOfCommands");
            DropForeignKey("dbo.ConsultingLetterOfCommands", "ConsultingRequestID", "dbo.ConsultingRequests");
            DropForeignKey("dbo.ConsultingRequests", "ActivityID", "dbo.Activities");
            DropForeignKey("dbo.ConsultingRiskControlMatrices", "ConsultingBPMID", "dbo.ConsultingBusinessProcesses");
            DropForeignKey("dbo.Walktroughs", "LetterOfCommandID", "dbo.LetterOfCommands");
            DropForeignKey("dbo.LetterOfCommands", "PreliminaryID", "dbo.Preliminaries");
            DropForeignKey("dbo.LetterOfCommands", "ActivityID", "dbo.Activities");
            DropForeignKey("dbo.BusinessProces", "WalktroughID", "dbo.Walktroughs");
            DropForeignKey("dbo.BusinessProces", "BPMID", "dbo.BPMs");
            DropForeignKey("dbo.BPMs", "WalktroughID", "dbo.Walktroughs");
            DropForeignKey("dbo.Walktroughs", "ActivityID", "dbo.Activities");
            DropForeignKey("dbo.AuditPlanningMemorandums", "ReviewRelationMasterID", "dbo.ReviewRelationMasters");
            DropForeignKey("dbo.AuditPlanningMemorandums", "PreliminaryID", "dbo.Preliminaries");
            DropForeignKey("dbo.Preliminaries", "EmployeeID", "dbo.Employees");
            DropForeignKey("dbo.Preliminaries", "Position_PositionID", "dbo.Positions");
            DropForeignKey("dbo.Employees", "PositionID", "dbo.Positions");
            DropForeignKey("dbo.Employees", "OrganizationID", "dbo.Organizations");
            DropForeignKey("dbo.Preliminaries", "ActivityID", "dbo.Activities");
            DropForeignKey("dbo.AuditPlanningMemorandums", "ActivityID", "dbo.Activities");
            DropForeignKey("dbo.AnnualPlannings", "ReviewRelationMasterID", "dbo.ReviewRelationMasters");
            DropForeignKey("dbo.ReviewRelationDetails", "ReviewRelationMasterID", "dbo.ReviewRelationMasters");
            DropForeignKey("dbo.ReviewRelationDetails", "PostId", "dbo.Posts");
            DropForeignKey("dbo.PostComments", "PostId", "dbo.Posts");
            DropForeignKey("dbo.AnnualPlannings", "ActivityID", "dbo.Activities");
            DropIndex("dbo.Reportings", new[] { "LetterOfCommandID" });
            DropIndex("dbo.RCMDetailRisks", new[] { "RiskControlMatrixID" });
            DropIndex("dbo.RCMDetailRiskControls", new[] { "RCMDetailRiskID" });
            DropIndex("dbo.RCMDetailControlAuditSteps", new[] { "RCMDetailRiskControlID" });
            DropIndex("dbo.NotulenEntryMeetings", new[] { "AuditPlanningMemorandumID" });
            DropIndex("dbo.LetterOfCommandDetailUntuks", new[] { "LetterOfCommandID" });
            DropIndex("dbo.LetterOfCommandDetailTembusans", new[] { "LetterOfCommandID" });
            DropIndex("dbo.LetterOfCommandDetailDasars", new[] { "LetterOfCommandID" });
            DropIndex("dbo.RiskControlMatrices", new[] { "BusinessProcesID" });
            DropIndex("dbo.FieldWorks", new[] { "RiskControlMatrixID" });
            DropIndex("dbo.FieldWorks", new[] { "LetterOfCommandID" });
            DropIndex("dbo.Feedbacks", new[] { "AuditPlanningMemorandumID" });
            DropIndex("dbo.FeedbackQuestionDetails", new[] { "FeedbackID" });
            DropIndex("dbo.EngagementActivities", new[] { "ActivityID" });
            DropIndex("dbo.ConsultingReportings", new[] { "ConsultingSuratPerintahID" });
            DropIndex("dbo.ConsultingRCMDetailRisks", new[] { "ConsultingRCMID" });
            DropIndex("dbo.ConsultingRCMDetailRiskControls", new[] { "ConsultingRCMDetailRiskID" });
            DropIndex("dbo.ConsultingRCMDetailControlAuditSteps", new[] { "ConsultingRCMDetailRiskControlID" });
            DropIndex("dbo.ConsultingLetterOfCommandMembers", new[] { "ConsultingSuratPerintahID" });
            DropIndex("dbo.ConsultingLetterOfCommandDetailUntuks", new[] { "ConsultingSuratPerintahID" });
            DropIndex("dbo.ConsultingLetterOfCommandDetailTembusans", new[] { "ConsultingSuratPerintahID" });
            DropIndex("dbo.ConsultingLetterOfCommandDetailDasars", new[] { "ConsultingSuratPerintahID" });
            DropIndex("dbo.ConsultingFieldWorks", new[] { "ConsultingSuratPerintahID" });
            DropIndex("dbo.ConsultingFeedbacks", new[] { "ConsultingSuratPerintahID" });
            DropIndex("dbo.ConsultingFeedbackQuestionDetails", new[] { "ConsultingFeedbackID" });
            DropIndex("dbo.ConsultingDraftAgreements", new[] { "ActivityID" });
            DropIndex("dbo.ConsultingRequests", new[] { "ActivityID" });
            DropIndex("dbo.ConsultingLetterOfCommands", new[] { "ConsultingRequestID" });
            DropIndex("dbo.ConsultingWalktroughs", new[] { "ConsultingSuratPerintahID" });
            DropIndex("dbo.ConsultingRiskControlMatrices", new[] { "ConsultingBPMID" });
            DropIndex("dbo.ConsultingBusinessProcesses", new[] { "ConsultingWalktroughID" });
            DropIndex("dbo.LetterOfCommands", new[] { "ActivityID" });
            DropIndex("dbo.LetterOfCommands", new[] { "PreliminaryID" });
            DropIndex("dbo.BusinessProces", new[] { "WalktroughID" });
            DropIndex("dbo.BusinessProces", new[] { "BPMID" });
            DropIndex("dbo.Walktroughs", new[] { "ActivityID" });
            DropIndex("dbo.Walktroughs", new[] { "LetterOfCommandID" });
            DropIndex("dbo.BPMs", new[] { "WalktroughID" });
            DropIndex("dbo.Employees", new[] { "PositionID" });
            DropIndex("dbo.Employees", new[] { "OrganizationID" });
            DropIndex("dbo.Preliminaries", new[] { "Position_PositionID" });
            DropIndex("dbo.Preliminaries", new[] { "EmployeeID" });
            DropIndex("dbo.Preliminaries", new[] { "ActivityID" });
            DropIndex("dbo.AuditPlanningMemorandums", new[] { "ReviewRelationMasterID" });
            DropIndex("dbo.AuditPlanningMemorandums", new[] { "ActivityID" });
            DropIndex("dbo.AuditPlanningMemorandums", new[] { "PreliminaryID" });
            DropIndex("dbo.PostComments", new[] { "PostId" });
            DropIndex("dbo.ReviewRelationDetails", new[] { "PostId" });
            DropIndex("dbo.ReviewRelationDetails", new[] { "ReviewRelationMasterID" });
            DropIndex("dbo.AnnualPlannings", new[] { "ReviewRelationMasterID" });
            DropIndex("dbo.AnnualPlannings", new[] { "ActivityID" });
            DropTable("dbo.Reportings");
            DropTable("dbo.RCMDetailRisks");
            DropTable("dbo.RCMDetailRiskControls");
            DropTable("dbo.RCMDetailControlAuditSteps");
            DropTable("dbo.Questioners");
            DropTable("dbo.NotulenEntryMeetings");
            DropTable("dbo.LetterOfCommandDetailUntuks");
            DropTable("dbo.LetterOfCommandDetailTembusans");
            DropTable("dbo.LetterOfCommandDetailDasars");
            DropTable("dbo.RiskControlMatrices");
            DropTable("dbo.FieldWorks");
            DropTable("dbo.Feedbacks");
            DropTable("dbo.FeedbackQuestionDetails");
            DropTable("dbo.EngagementActivities");
            DropTable("dbo.EmailForms");
            DropTable("dbo.ConsultingReportings");
            DropTable("dbo.ConsultingRCMDetailRisks");
            DropTable("dbo.ConsultingRCMDetailRiskControls");
            DropTable("dbo.ConsultingRCMDetailControlAuditSteps");
            DropTable("dbo.ConsultingLetterOfCommandMembers");
            DropTable("dbo.ConsultingLetterOfCommandDetailUntuks");
            DropTable("dbo.ConsultingLetterOfCommandDetailTembusans");
            DropTable("dbo.ConsultingLetterOfCommandDetailDasars");
            DropTable("dbo.ConsultingFieldWorks");
            DropTable("dbo.ConsultingFeedbacks");
            DropTable("dbo.ConsultingFeedbackQuestionDetails");
            DropTable("dbo.ConsultingDraftAgreements");
            DropTable("dbo.ConsultingRequests");
            DropTable("dbo.ConsultingLetterOfCommands");
            DropTable("dbo.ConsultingWalktroughs");
            DropTable("dbo.ConsultingRiskControlMatrices");
            DropTable("dbo.ConsultingBusinessProcesses");
            DropTable("dbo.LetterOfCommands");
            DropTable("dbo.BusinessProces");
            DropTable("dbo.Walktroughs");
            DropTable("dbo.BPMs");
            DropTable("dbo.Positions");
            DropTable("dbo.Organizations");
            DropTable("dbo.Employees");
            DropTable("dbo.Preliminaries");
            DropTable("dbo.AuditPlanningMemorandums");
            DropTable("dbo.PostComments");
            DropTable("dbo.Posts");
            DropTable("dbo.ReviewRelationDetails");
            DropTable("dbo.ReviewRelationMasters");
            DropTable("dbo.AnnualPlannings");
            DropTable("dbo.Activities");
        }
    }
}

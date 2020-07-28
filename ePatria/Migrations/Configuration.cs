namespace ePatria.Migrations
{
    using ePatria.Models;
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<ePatria.Models.ePatriaDefault>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            AutomaticMigrationDataLossAllowed = true;
        }

        protected override void Seed(ePatria.Models.ePatriaDefault context)
        {
            var manager = new UserManager<ApplicationUser>(
                new UserStore<ApplicationUser>(
                    new ApplicationDbContext()));

            context.Organizations.AddOrUpdate(x => x.OrganizationID,
                new Models.Organization()
                {
                    OrganizationID = 1,
                    OrganizationParentID = null,
                    Name = "Direktur Utama",
                    Description = "-"
                }
                );

           // context.Organizations.AddOrUpdate(x => x.OrganizationID,
           //     new Models.Organization()
           //     {
           //         OrganizationID = 2,
           //         OrganizationParentID = 1,
           //         Name = "Sub Finance and G&A",
           //         Description = "-"
           //     }
           //     );

           // context.Organizations.AddOrUpdate(x => x.OrganizationID,
           //    new Models.Organization()
           //    {
           //        OrganizationID = 3,
           //        OrganizationParentID = 1,
           //        Name = "Sub Sub Finance and G&A",
           //        Description = "-"
           //    }
           //    );

            context.Positions.AddOrUpdate(x => x.PositionID,
                new Models.Position()
                {
                    PositionID = 1,
                    PositionName = "Direktur Utama",
                    JobDesc = "-"
                }
                );

           // context.Positions.AddOrUpdate(x => x.PositionID,
           //     new Models.Position()
           //     {
           //         PositionID = 2,
           //         PositionName = "VP ABC",
           //         JobDesc = "-"
           //     }
           //         );

           // context.Employees.AddOrUpdate(x => x.EmployeeID,
           //     new Models.Employee()
           //     {
           //         EmployeeID = 1,
           //         Type = "Auditor",
           //         Name = "John Doe",
           //         NoPEK = "1123",
           //         Email = "John.Doe@gmail.com",
           //         PhoneNumber = "+628777912398",
           //         Status = "PWTT",
           //         OrganizationID = 1,
           //         PositionID = 2
           //     }
           //     );

           // context.Activities.AddOrUpdate(x => x.ActivityID,
           //     new Models.Activity()
           //     {
           //         ActivityID = 1,
           //         ActivityParentID = null,
           //         Name = "Expenditure",
           //         Status = "Pending for Approval",
           //         Tahun = "2011, 2012, 2013",
           //         DepartementID = 1,

           //     }
           //     );

           // context.Activities.AddOrUpdate(x => x.ActivityID,
           //     new Models.Activity()
           //     {
           //         ActivityID = 2,
           //         ActivityParentID = 1,
           //         Name = "Sub Expenditure 1",
           //         Status = "Pending for Approval",
           //         Tahun = "2011, 2012, 2013",
           //         DepartementID = 1,

           //     }
           //     );
           // context.Activities.AddOrUpdate(x => x.ActivityID,
           //     new Models.Activity()
           //     {
           //         ActivityID = 3,
           //         ActivityParentID = 1,
           //         Name = "Sub Expenditure 2",
           //         Status = "Approve",
           //         Tahun = "2011, 2012, 2013",
           //         DepartementID = 1,

           //     }
           //     );
           // context.AnnualPlannings.AddOrUpdate(x => x.AnnualPlanningID,
           //     new Models.AnnualPlanning()
           //     {
           //         AnnualPlanningID = 1,
           //         ActivityID = 3,
           //         Status = "On Progress",
           //         //Date_Start = DateTime.Parse("2004-02-12"),
           //         //Date_End = DateTime.Parse("2004-02-12"),
           //         Tahun = "2011",
           //         Approval_Status = "Approve",
           //         //PICID = 1,
           //         //SupervisorID = 1,
           //         //TeamLeaderID = 1,
           //         //MemberID = 1
           //     }
           //     );

           // context.EngagementActivities.AddOrUpdate(x => x.EngagementID,
           //     new Models.EngagementActivity()
           //     {
           //         EngagementID = 1,
           //         ActivityID = 3,
           //         Name = "Test Engagement",
           //     }
           //     );

           // context.Preliminaries.AddOrUpdate(x => x.PreliminaryID,
           //     new Models.Preliminary()
           //     {
           //         PreliminaryID = 1,
           //         Type = "Planned",
           //         NomorPreliminarySurvey = "Preliminary/1",
           //         Date_Start = DateTime.Parse("2015-02-12"),
           //         Date_End = DateTime.Parse("2015-02-12"),
           //         Status = "Approve",
           //         ActivityID = 3,
           //         EmployeeID = 1,
           //     }
           //     );

          

           // context.Questioners.AddOrUpdate(x => x.QuestionerID,
           //     new Models.Questioner()
           //     {
           //         QuestionerID = 1,
           //         Type = "Text Box",
           //         Value = null,
           //         Name = "Siapa Nama Anda",
           //         Status = "Active"
           //     }
           //     );

           // context.LetterOfCommands.AddOrUpdate(x => x.LetterOfCommandID,
           //     new Models.LetterOfCommand()
           //     {
           //         LetterOfCommandID = 1,
           //         ActivityID = 3,
           //         NomorSP = "SP/1",
           //         Status = "Approve",
           //         Remarks = "Ada perpanjangan SP dengan no SP 123",
           //         AssuranceType = "Operational",
           //         Date_Start = DateTime.Parse("2015-09-1"),
           //         Date_End = DateTime.Parse("2015-10-1"),
           //         PreliminaryID = 1,
           //         PICID = 1,
           //         SupervisorID = 1,
           //         TeamLeaderID = 1,
           //         MemberID = "1",
           //         Menimbang = null,
           //         Penutup = null
           //     }
           //     );

           // context.LetterOfCommandDetailDasars.AddOrUpdate(x => x.ID,
           //     new Models.LetterOfCommandDetailDasar()
           //     {
           //         ID = 1,
           //         LetterOfCommandID = 1,
           //         Dasar = "Dasar"
           //     }
           //     );

           // context.LetterOfCommandDetailTembusans.AddOrUpdate(x => x.ID,
           //     new Models.LetterOfCommandDetailTembusan()
           //     {
           //         ID = 1,
           //         LetterOfCommandID = 1,
           //         Tembusan = "Tembusan"
           //     }
           //     );

           // context.LetterOfCommandDetailUntuks.AddOrUpdate(x => x.ID,
           //    new Models.LetterOfCommandDetailUntuk()
           //    {
           //        ID = 1,
           //        LetterOfCommandID = 1,
           //        Untuk = "John"
           //    }
           //    );



           // context.AuditPlanningMemorandums.AddOrUpdate(x => x.AuditPlanningMemorandumID,
           //     new Models.AuditPlanningMemorandum()
           //     {
           //         AuditPlanningMemorandumID = 1,
           //         NomerAPM = "APM/1",
           //         PreliminaryID = 1,
           //         Date_Start = DateTime.Parse("2015-09-1"),
           //         Date_End = DateTime.Parse("2015-10-1"),
           //         Status = "Pending",
           //         ActivityID = 3,
           //         TujuanAudit = "Testing",
           //         RuangLingkupAudit = "Test",
           //         MetodologiAudit = "Test",
           //         DataDanDokumen = "Test",
           //         EntryMeetingDateStart = DateTime.Parse("2015-09-1"),
           //         EntryMeetingDateEnd = DateTime.Parse("2015-10-1"),
           //         WalktroughDateStart = DateTime.Parse("2015-09-1"),
           //         WalktroughDateEnd = DateTime.Parse("2015-10-1"),
           //         FieldWorkDateStart = DateTime.Parse("2015-09-1"),
           //         FieldWorkDateEnd = DateTime.Parse("2015-10-1"),
           //         ExitMeetingDateStart = DateTime.Parse("2015-09-1"),
           //         ExitMeetingDateEnd = DateTime.Parse("2015-10-1"),
           //         LHADateStart = DateTime.Parse("2015-09-1"),
           //         LHADateEnd = DateTime.Parse("2015-10-1"),
           //         PICID = 1,
           //         SupervisorID = 1,
           //         TeamLeaderID = 1,
           //         MemberID = "1"
           //     }
           //     );

           // context.NotulenEntryMeetings.AddOrUpdate(x => x.NotulenEntryMeetingID,
           //    new Models.NotulenEntryMeeting()
           //    {
           //        NotulenEntryMeetingID = 1,
           //        AuditPlanningMemorandumID = 1,
           //        Tujuan = "Testing",
           //        TimeConsumableStartDate = DateTime.Parse("2015-09-1"),
           //        TimeConsumableEndDate = DateTime.Parse("2015-10-1"),
           //        Place = "Pending",
           //        EmployeeChairmanID = 2,
           //        EmployeeModeratorID = 2,
           //        EmployeeReporterID = 2,
           //        EmployeeMemberID = 2,
           //        Opening = "Test",
           //        ExposurePlan = "Test"
           //    }
           //    );

           // context.Walktroughs.AddOrUpdate(x => x.WalktroughID,
           //    new Models.Walktrough()
           //    {
           //        WalktroughID = 1,
           //        LetterOfCommandID = 1,
           //        ActivityID = 1,
           //        Date_Start = DateTime.Parse("2015-09-1"),
           //        Date_End = DateTime.Parse("2015-10-1"),
           //        Status = "Approve",
           //        Remarks = "Perpanjangan SP 123asd"
           //    }
           //    );

           // context.BPMs.AddOrUpdate(x => x.BPMID,
           //     new Models.BPM()
           //     {
           //         BPMID = 1,
           //         WalktroughID = 1,
           //         Name = "BPM1",
           //         Status = "Approve"
           //     }
           //     );

           // context.BusinessProcess.AddOrUpdate(x => x.BusinessProcesID,
           //    new Models.BusinessProces()
           //    {
           //        BusinessProcesID = 1,
           //        WalktroughID = 1,
           //        BPMID = 1,
           //        DocumentNo = "12345",
           //        DocumentName = "ABCDEF",
           //        FolderName = "Preparation",
           //    }
           //    );

           // context.RiskControlMatrixs.AddOrUpdate(x => x.RiskControlMatrixID,
           //    new Models.RiskControlMatrix()
           //    {
           //        RiskControlMatrixID = 1,
           //        BusinessProcesID = 1,
           //        SubBusinessProcess = 1,
           //        Objectives = "Test"
           //    }
           //    );

           // context.RCMDetailRisks.AddOrUpdate(x => x.RCMDetailRiskID,
           //    new Models.RCMDetailRisk()
           //    {
           //        RCMDetailRiskID = 1,
           //        RiskControlMatrixID = 1,
           //        RiskName = "Risk 1",
           //        Status = "Pending"
           //    }
           //    );

           // context.RCMDetailRiskControls.AddOrUpdate(x => x.RCMDetailRiskControlID,
           //    new Models.RCMDetailRiskControl()
           //    {
           //        RCMDetailRiskControlID = 1,
           //        RCMDetailRiskID = 1,
           //        ControlName = "Control 1",
           //        Status = "Approve",
           //        RefNo = "12345",
           //        Title = "Test",
           //        Createria = "Test",
           //        Impact = "Test",
           //        ImpactValue = "Test",
           //        Cause = "Test",
           //        Recommendation = "Test",
           //        ManagementResponse = "Test",
           //        PIC = "Test",
           //        FindingType = "Test",
           //        DueDate = DateTime.Parse("2015-10-10"),
           //        CaseCloseBool = "Test",
           //        CloseDate = DateTime.Parse("2016-10-10")
           //    }
           //    );


           // context.RCMDetailControlAuditSteps.AddOrUpdate(x => x.RCMDetailControlAuditStepID,
           //    new Models.RCMDetailControlAuditStep()
           //    {
           //        RCMDetailControlAuditStepID = 1,
           //        RCMDetailRiskControlID = 1,
           //        AuditStepName = " Step 1",
           //        Status = "Approve",
           //        WorkDoneDescription = "Done",
           //        WorkDoneResult = "Test"
           //    }
           //    );

           // context.FieldWorks.AddOrUpdate(x => x.FieldWorkID,
           //    new Models.FieldWork()
           //    {
           //        FieldWorkID = 1,
           //        LetterOfCommandID = 1,
           //        RiskControlMatrixID = 1,
           //        Status = "Approve"
           //    }
           //    );

           // context.Reportings.AddOrUpdate(x => x.ReportingID,
           //   new Models.Reporting()
           //   {
           //       ReportingID = 1,
           //       LetterOfCommandID = 1,
           //       NomorLaporan = "12345",
           //       MemoDescription = "Test",
           //       SummaryDescription = "Test",
           //       Bab01Title = "Test",
           //       Bab01SubTitle = "Test",
           //       Bab01Description = "Test",
           //       Bab02Title = "Test",
           //       Bab02SubTitle = "Test",
           //       Bab02Description = "Test"
           //   }
           //   );

           // context.Feedbacks.AddOrUpdate(x => x.FeedbackID,
           //    new Models.Feedback()
           //    {
           //        FeedbackID = 1,
           //        AuditPlanningMemorandumID = 1
           //    }
           //    );


           // context.FeedbackQuestionDetails.AddOrUpdate(x => x.FeedbackQuestionDetailID,
           //    new Models.FeedbackQuestionDetail()
           //    {
           //        FeedbackQuestionDetailID = 1,
           //        FeedbackID = 1,
           //        QuestionerID = 1,
           //        PICID = 1
           //    }
           //    );

           // context.ReviewRelationMasters.AddOrUpdate(x => x.ReviewRelationMasterID,
           //   new Models.ReviewRelationMaster()
           //   {
           //       ReviewRelationMasterID = 1,
           //       Description = "prelim1",
           //   }
           //   );

           // context.ReviewRelationMasters.AddOrUpdate(x => x.ReviewRelationMasterID,
           //   new Models.ReviewRelationMaster()
           //   {
           //       ReviewRelationMasterID = 2,
           //       Description = "apm1",
           //   }
           //   );

           // context.ReviewRelationMasters.AddOrUpdate(x => x.ReviewRelationMasterID,
           //   new Models.ReviewRelationMaster()
           //   {
           //       ReviewRelationMasterID = 3,
           //       Description = "letter1",
           //   }
           //   );

           // context.ReviewRelationMasters.AddOrUpdate(x => x.ReviewRelationMasterID,
           //   new Models.ReviewRelationMaster()
           //   {
           //       ReviewRelationMasterID = 4,
           //       Description = "bpm1",
           //   }
           //   );

           // context.ReviewRelationMasters.AddOrUpdate(x => x.ReviewRelationMasterID,
           //   new Models.ReviewRelationMaster()
           //   {
           //       ReviewRelationMasterID = 5,
           //       Description = "rcm1",
           //   }
           //   );

           // //context.ReviewRelationDetails.AddOrUpdate(x => x.ReviewRelationDetailID,
           // //  new Models.ReviewRelationDetail()
           // //  {
           // //      ReviewRelationDetailID = 1,
           // //      ReviewRelationMasterID = 1,
           // //      PostId = 1,
           // //  }
           // //  );


           // context.ConsultingRequests.AddOrUpdate(x => x.ConsultingRequestID,
           //   new Models.ConsultingRequest()
           //   {
           //       ConsultingRequestID = 1,
           //       NoRequest = "123",
           //       NoSurat = "1123",
           //       RequesterID = 1,
           //       Date_Start = DateTime.Parse("2016-10-10"),
           //       Date_End = DateTime.Parse("2016-10-10"),
           //       EvaluationResult = "Test",
           //       ActivityID = 3,
           //   }
           //   );

           // context.ConsultingDraftAgreements.AddOrUpdate(x => x.ConsultingDraftAgreementID,
           //   new Models.ConsultingDraftAgreement()
           //   {
           //       ConsultingDraftAgreementID = 1,
           //       NoRequest = "123",
           //       NoSurat = "1123",
           //       RequesterID = 1,
           //       Date_Start = DateTime.Parse("2016-10-10"),
           //       Date_End = DateTime.Parse("2016-10-10"),
           //       ActivityID = 3,
           //   }
           //   );


           // context.ConsultingLetterOfCommands.AddOrUpdate(x => x.ConsultingSuratPerintahID,
           //     new Models.ConsultingLetterOfCommand()
           //     {
           //         ConsultingSuratPerintahID = 1,
           //         NomorSP = "1123",
           //         Status = "Pending",
           //         StartDate = DateTime.Parse("2015-09-1"),
           //         EndDate = DateTime.Parse("2015-10-1"),
           //         ConsultingRequestID = 1,
           //         EngagementName = "Test",
           //         PicID = 1,
           //         SupervisorID = 1,
           //         TeamLeaderID = 1,
           //         Menimbang = null,
           //         Penutup = null
           //     }
           //     );

           // context.ConsultingLetterOfCommandMembers.AddOrUpdate(x => x.ConsultingLetterOfCommandMemberID,
           //  new Models.ConsultingLetterOfCommandMember()
           //  {
           //      ConsultingLetterOfCommandMemberID = 1,
           //      ConsultingSuratPerintahID = 1,
           //      MemberID = 1,
           //  }
           //  );


           // context.ConsultingLetterOfCommandDetailUntuks.AddOrUpdate(x => x.ConsultingLetterOfCommandDetailUntukID,
           //     new Models.ConsultingLetterOfCommandDetailUntuk()
           //     {
           //         ConsultingLetterOfCommandDetailUntukID = 1,
           //         ConsultingSuratPerintahID = 1,
           //         Untuk = "Untuk"
           //     }
           //     );

           // context.ConsultingLetterOfCommandDetailTembusans.AddOrUpdate(x => x.ConsultingLetterOfCommandDetailTembusanID,
           //    new Models.ConsultingLetterOfCommandDetailTembusan()
           //    {
           //        ConsultingLetterOfCommandDetailTembusanID = 1,
           //        ConsultingSuratPerintahID = 1,
           //        Tembusan = "Tembusan"
           //    }
           //    );


           // context.ConsultingWalktroughs.AddOrUpdate(x => x.ConsultingWalktroughID,
           //   new Models.ConsultingWalktrough()
           //   {
           //       ConsultingWalktroughID = 1,
           //       ConsultingSuratPerintahID = 1,
           //       ActivityID = 3,
           //       PeriodStartDate = DateTime.Parse("2015-10-1"),
           //       PeriodEndDate = DateTime.Parse("2015-11-1"),
           //       Remarks = "Test",
           //       Status = null,
           //   }
           //   );


           // context.ConsultingBusinessProcess.AddOrUpdate(x => x.ConsultingBPMID,
           //   new Models.ConsultingBusinessProcess()
           //   {
           //       ConsultingBPMID = 1,
           //       ConsultingWalktroughID = 1,
           //       DocumentNo = "12345",
           //       DocumentName = "ABCDEF",
           //       FolderName = "Preparation",
           //   }
           //   );

           // context.ConsultingRiskControlMatrixs.AddOrUpdate(x => x.ConsultingRCMID,
           //   new Models.ConsultingRiskControlMatrix()
           //   {
           //       ConsultingRCMID = 1,
           //       ConsultingBPMID = 1,
           //       SubBusinessProcess = "1",
           //       Objectives = "Test"
           //   }
           //   );




           // context.ConsultingRCMDetailRisks.AddOrUpdate(x => x.ConsultingRCMDetailRiskID,
           //   new Models.ConsultingRCMDetailRisk()
           //   {
           //       ConsultingRCMDetailRiskID = 1,
           //       ConsultingRCMID = 1,
           //       RiskName = "Risk 1",
           //       Status = "Pending"
           //   }
           //   );

           // context.ConsultingRCMDetailRiskControls.AddOrUpdate(x => x.ConsultingRCMDetailRiskControlID,
           //    new Models.ConsultingRCMDetailRiskControl()
           //    {
           //        ConsultingRCMDetailRiskControlID = 1,
           //        ConsultingRCMDetailRiskID = 1,
           //        ControlName = "Control 1",
           //        Status = "Approve",
           //        RefNo = "12345",
           //        Title = "Test",
           //        Criteria = "Test",
           //        Impact = "Test",
           //        ImpactValue = "Test",
           //        Cause = "Test",
           //        Recommendation = "Test",
           //        ManagementResponse = "Test",
           //        PICID = 1,
           //        FindingType = "Test",
           //        DueDate = DateTime.Parse("2015-10-10"),
           //        CaseCloseBool = "Test",
           //        CloseDate = DateTime.Parse("2016-10-10")
           //    }
           //    );


           // context.ConsultingRCMDetailControlAuditSteps.AddOrUpdate(x => x.ConsultingRCMDetailControlAuditStepID,
           //    new Models.ConsultingRCMDetailControlAuditStep()
           //    {
           //        ConsultingRCMDetailControlAuditStepID = 1,
           //        ConsultingRCMDetailRiskControlID = 1,
           //        AuditStepName = " Step 1",
           //        Status = "Approve",
           //        WorkDoneDescription = "Done",
           //        WorkDoneResult = "Test"
           //    }
           //    );


           // context.ConsultingFieldWork.AddOrUpdate(x => x.ConsultingFieldWorkID,
           //   new Models.ConsultingFieldWork()
           //   {
           //       ConsultingFieldWorkID = 1,
           //       Desc = "Test",
           //       ConsultingSuratPerintahID = 1,
           //   }
           //   );

           // context.ConsultingReportings.AddOrUpdate(x => x.ConsultingReportingID,
           //    new Models.ConsultingReporting()
           //    {
           //        ConsultingReportingID = 1,
           //        ConsultingSuratPerintahID = 1,
           //        ActivityID = 3,
           //        NoLaporan = "1123",
           //        Kepada = "John",
           //        Dari = "Chris",
           //        Lampiran = "Test",
           //        Perihal = "Test",
           //        Hasil = "Test",
           //}
           //);

           // context.ConsultingFeedbacks.AddOrUpdate(x => x.ConsultingFeedbackID,
           // new Models.ConsultingFeedback()
           // {
           //     ConsultingFeedbackID = 1,
           //     ConsultingSuratPerintahID = 1,
           // }
           // );

           // context.ConsultingFeedbackQuestionDetails.AddOrUpdate(x => x.ConsultingFeedbackQuestionDetailID,
           // new Models.ConsultingFeedbackQuestionDetail()
           // {
           //     ConsultingFeedbackQuestionDetailID = 1,
           //     ConsultingFeedbackID = 1,
           //     PicID = 1,
           //     QuestionID = 1,
           // }
           // );

           // context.ExitMeetings.AddOrUpdate(x => x.ExitMeetingID,
           //    new Models.ExitMeeting()
           //    {
           //        ExitMeetingID = 1,
           //        ActivityID = 3,
           //        EngagementID = 1,
           //        Status = "On Progress",
           //        Reviewer1 = "Supervisor",
           //        Reviewer2 = "CIA",
           //        Date_Start = DateTime.Parse("2015-09-1"),
           //        Date_End = DateTime.Parse("2015-10-1"),
           //        LetterOfCommandID = 1,
           //        OrganizationID = 1,
                  
           //    }
           //    );
        }
    }
}

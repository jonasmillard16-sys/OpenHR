using Microsoft.EntityFrameworkCore;
using RegionHR.SharedKernel.Domain;
using RegionHR.Core.Domain;
using RegionHR.Payroll.Domain;
using RegionHR.Scheduling.Domain;
using RegionHR.CaseManagement.Domain;
using RegionHR.LAS.Domain;
using RegionHR.HalsoSAM.Domain;
using RegionHR.SalaryReview.Domain;
using RegionHR.Travel.Domain;
using RegionHR.Recruitment.Domain;
using RegionHR.IntegrationHub.Infrastructure;
using RegionHR.Audit.Domain;
using RegionHR.Notifications.Domain;
using RegionHR.Leave.Domain;
using RegionHR.Documents.Domain;
using RegionHR.Performance.Domain;
using RegionHR.Reporting.Domain;
using RegionHR.GDPR.Domain;
using RegionHR.Competence.Domain;
using RegionHR.Benefits.Domain;
using RegionHR.LMS.Domain;
using RegionHR.Positions.Domain;
using RegionHR.Offboarding.Domain;
using RegionHR.Analytics.Domain;
using RegionHR.Configuration.Domain;
using RegionHR.Infrastructure.Authorization;
using RegionHR.Infrastructure.Provisioning;
using RegionHR.Infrastructure.Arbetsmiljo;
using RegionHR.Infrastructure.Journeys;

namespace RegionHR.Infrastructure.Persistence;

public class RegionHRDbContext : DbContext
{
    public RegionHRDbContext(DbContextOptions<RegionHRDbContext> options) : base(options) { }

    // Core HR (schema: core_hr)
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Employment> Employments => Set<Employment>();
    public DbSet<OrganizationUnit> OrganizationUnits => Set<OrganizationUnit>();
    public DbSet<EmergencyContact> EmergencyContacts => Set<EmergencyContact>();

    // Payroll (schema: payroll)
    public DbSet<PayrollRun> PayrollRuns => Set<PayrollRun>();
    public DbSet<PayrollResult> PayrollResults => Set<PayrollResult>();
    public DbSet<TaxTable> TaxTables => Set<TaxTable>();
    public DbSet<SalaryCode> SalaryCodes => Set<SalaryCode>();

    // Scheduling (schema: scheduling)
    public DbSet<Schedule> Schedules => Set<Schedule>();
    public DbSet<ScheduledShift> ScheduledShifts => Set<ScheduledShift>();
    public DbSet<TimeClockEvent> TimeClockEvents => Set<TimeClockEvent>();
    public DbSet<StaffingTemplate> StaffingTemplates => Set<StaffingTemplate>();
    public DbSet<ShiftSwapRequest> ShiftSwapRequests => Set<ShiftSwapRequest>();
    public DbSet<Timesheet> Timesheets => Set<Timesheet>();

    // Case Management (schema: case_mgmt)
    public DbSet<Case> Cases => Set<Case>();

    // LAS (schema: las)
    public DbSet<LASAccumulation> LASAccumulations => Set<LASAccumulation>();

    // HälsoSAM (schema: halsosam)
    public DbSet<RehabCase> RehabCases => Set<RehabCase>();

    // Salary Review (schema: salary_review)
    public DbSet<SalaryReviewRound> SalaryReviewRounds => Set<SalaryReviewRound>();

    // Travel (schema: travel)
    public DbSet<TravelClaim> TravelClaims => Set<TravelClaim>();

    // Recruitment (schema: recruitment)
    public DbSet<Vacancy> Vacancies => Set<Vacancy>();
    public DbSet<CommunicationTemplate> CommunicationTemplates => Set<CommunicationTemplate>();
    public DbSet<OnboardingChecklist> OnboardingChecklists => Set<OnboardingChecklist>();
    public DbSet<RequisitionApproval> RequisitionApprovals => Set<RequisitionApproval>();
    public DbSet<InterviewSchedule> InterviewSchedules => Set<InterviewSchedule>();
    public DbSet<Scorecard> Scorecards => Set<Scorecard>();
    public DbSet<TalentPoolEntry> TalentPoolEntries => Set<TalentPoolEntry>();

    // Integration Hub (schema: integration_hub)
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    // Audit (schema: audit)
    public DbSet<AuditEntry> AuditEntries => Set<AuditEntry>();

    // Notifications (schema: notifications)
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<NotificationTemplate> NotificationTemplates => Set<NotificationTemplate>();

    // Leave (schema: leave)
    public DbSet<VacationBalance> VacationBalances => Set<VacationBalance>();
    public DbSet<LeaveRequest> LeaveRequests => Set<LeaveRequest>();
    public DbSet<SickLeaveNotification> SickLeaveNotifications => Set<SickLeaveNotification>();

    // Documents (schema: documents)
    public DbSet<Document> Documents => Set<Document>();
    public DbSet<DocumentTemplate> DocumentTemplates => Set<DocumentTemplate>();
    public DbSet<DocumentSignature> DocumentSignatures => Set<DocumentSignature>();
    public DbSet<DocumentVersion> DocumentVersions => Set<DocumentVersion>();

    // Performance (schema: performance)
    public DbSet<PerformanceReview> PerformanceReviews => Set<PerformanceReview>();

    // Reporting (schema: reporting)
    public DbSet<ReportDefinition> ReportDefinitions => Set<ReportDefinition>();
    public DbSet<ReportExecution> ReportExecutions => Set<ReportExecution>();

    // GDPR (schema: gdpr)
    public DbSet<DataSubjectRequest> DataSubjectRequests => Set<DataSubjectRequest>();
    public DbSet<RetentionRecord> RetentionRecords => Set<RetentionRecord>();

    // Competence (schema: competence)
    public DbSet<Certification> Certifications => Set<Certification>();
    public DbSet<MandatoryTraining> MandatoryTrainings => Set<MandatoryTraining>();
    public DbSet<Skill> Skills => Set<Skill>();
    public DbSet<EmployeeSkill> EmployeeSkills => Set<EmployeeSkill>();
    public DbSet<PositionSkillRequirement> PositionSkillRequirements => Set<PositionSkillRequirement>();

    // Benefits (schema: benefits)
    public DbSet<Benefit> Benefits => Set<Benefit>();
    public DbSet<EmployeeBenefit> EmployeeBenefits => Set<EmployeeBenefit>();

    // LMS (schema: lms)
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<CourseEnrollment> CourseEnrollments => Set<CourseEnrollment>();
    public DbSet<LearningPath> LearningPaths => Set<LearningPath>();
    public DbSet<LearningPathStep> LearningPathSteps => Set<LearningPathStep>();

    // Positions (schema: positions)
    public DbSet<Position> Positions_Table => Set<Position>();
    public DbSet<HeadcountPlan> HeadcountPlans => Set<HeadcountPlan>();

    // Offboarding (schema: offboarding)
    public DbSet<OffboardingCase> OffboardingCases => Set<OffboardingCase>();

    // Analytics (schema: analytics)
    public DbSet<SavedReport> SavedReports => Set<SavedReport>();
    public DbSet<Dashboard> Dashboards => Set<Dashboard>();

    // Configuration (schema: configuration)
    public DbSet<TenantConfiguration> TenantConfigurations => Set<TenantConfiguration>();
    public DbSet<CustomField> CustomFields => Set<CustomField>();
    public DbSet<CustomFieldValue> CustomFieldValues => Set<CustomFieldValue>();
    public DbSet<WorkflowDefinition> WorkflowDefinitions => Set<WorkflowDefinition>();

    // Authorization (schema: authorization)
    public DbSet<FieldPermission> FieldPermissions => Set<FieldPermission>();
    public DbSet<DelegatedAccess> DelegatedAccesses => Set<DelegatedAccess>();

    // Arbetsmiljo (schema: arbetsmiljo)
    public DbSet<Incident> Incidents => Set<Incident>();
    public DbSet<SafetyRound> SafetyRounds => Set<SafetyRound>();
    public DbSet<RiskAssessment> RiskAssessments => Set<RiskAssessment>();

    // Journeys (schema: journeys) — only 2 DbSet, step entities are owned
    public DbSet<JourneyTemplate> JourneyTemplates => Set<JourneyTemplate>();
    public DbSet<JourneyInstance> JourneyInstances => Set<JourneyInstance>();

    // Provisioning (schema: provisioning)
    public DbSet<ProvisioningEvent> ProvisioningEvents => Set<ProvisioningEvent>();
    public DbSet<ProvisioningRule> ProvisioningRules => Set<ProvisioningRule>();

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        // Register value converters BEFORE model validation — this is the only way
        // to make EF Core accept strongly-typed IDs without [NotMapped]
        // ALL strongly-typed IDs and value objects need converters
        configurationBuilder.Properties<EmployeeId>().HaveConversion<EmployeeIdConverter>();
        configurationBuilder.Properties<OrganizationId>().HaveConversion<OrganizationIdConverter>();
        configurationBuilder.Properties<EmploymentId>().HaveConversion<EmploymentIdConverter>();
        configurationBuilder.Properties<CaseId>().HaveConversion<CaseIdConverter>();
        configurationBuilder.Properties<PayrollRunId>().HaveConversion<PayrollRunIdConverter>();
        configurationBuilder.Properties<ScheduleId>().HaveConversion<ScheduleIdConverter>();
        configurationBuilder.Properties<StaffingTemplateId>().HaveConversion<StaffingTemplateIdConverter>();
        configurationBuilder.Properties<ShiftSwapId>().HaveConversion<ShiftSwapIdConverter>();
        configurationBuilder.Properties<Money>().HaveConversion<MoneyConverter>();
        configurationBuilder.Properties<Percentage>().HaveConversion<PercentageConverter>();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(RegionHRDbContext).Assembly);
    }
}

// EF Core value converters for strongly-typed IDs
public class EmployeeIdConverter : Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<EmployeeId, Guid>
{
    public EmployeeIdConverter() : base(v => v.Value, v => EmployeeId.From(v)) { }
}

public class OrganizationIdConverter : Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<OrganizationId, Guid>
{
    public OrganizationIdConverter() : base(v => v.Value, v => new OrganizationId(v)) { }
}

public class EmploymentIdConverter : Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<EmploymentId, Guid>
{
    public EmploymentIdConverter() : base(v => v.Value, v => new EmploymentId(v)) { }
}

public class CaseIdConverter : Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<CaseId, Guid>
{
    public CaseIdConverter() : base(v => v.Value, v => new CaseId(v)) { }
}

public class PayrollRunIdConverter : Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<PayrollRunId, Guid>
{
    public PayrollRunIdConverter() : base(v => v.Value, v => new PayrollRunId(v)) { }
}

public class ScheduleIdConverter : Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<ScheduleId, Guid>
{
    public ScheduleIdConverter() : base(v => v.Value, v => new ScheduleId(v)) { }
}

public class StaffingTemplateIdConverter : Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<StaffingTemplateId, Guid>
{
    public StaffingTemplateIdConverter() : base(v => v.Value, v => new StaffingTemplateId(v)) { }
}

public class ShiftSwapIdConverter : Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<ShiftSwapId, Guid>
{
    public ShiftSwapIdConverter() : base(v => v.Value, v => new ShiftSwapId(v)) { }
}

public class MoneyConverter : Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<Money, decimal>
{
    public MoneyConverter() : base(v => v.Amount, v => new Money(v, "SEK")) { }
}

public class PercentageConverter : Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<Percentage, decimal>
{
    public PercentageConverter() : base(v => v.Value, v => new Percentage(v)) { }
}

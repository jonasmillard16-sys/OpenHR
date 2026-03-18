using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace RegionHR.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "las");

            migrationBuilder.EnsureSchema(
                name: "audit");

            migrationBuilder.EnsureSchema(
                name: "benefits");

            migrationBuilder.EnsureSchema(
                name: "case_mgmt");

            migrationBuilder.EnsureSchema(
                name: "competence");

            migrationBuilder.EnsureSchema(
                name: "recruitment");

            migrationBuilder.EnsureSchema(
                name: "lms");

            migrationBuilder.EnsureSchema(
                name: "configuration");

            migrationBuilder.EnsureSchema(
                name: "analytics");

            migrationBuilder.EnsureSchema(
                name: "gdpr");

            migrationBuilder.EnsureSchema(
                name: "authorization");

            migrationBuilder.EnsureSchema(
                name: "documents");

            migrationBuilder.EnsureSchema(
                name: "core_hr");

            migrationBuilder.EnsureSchema(
                name: "positions");

            migrationBuilder.EnsureSchema(
                name: "leave");

            migrationBuilder.EnsureSchema(
                name: "notifications");

            migrationBuilder.EnsureSchema(
                name: "offboarding");

            migrationBuilder.EnsureSchema(
                name: "payroll");

            migrationBuilder.EnsureSchema(
                name: "performance");

            migrationBuilder.EnsureSchema(
                name: "reporting");

            migrationBuilder.EnsureSchema(
                name: "scheduling");

            migrationBuilder.CreateTable(
                name: "accumulations",
                schema: "las",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    anstalld_id = table.Column<Guid>(type: "uuid", nullable: false),
                    anstallningsform = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ackumulerade_dagar = table.Column<int>(type: "integer", nullable: false),
                    referensfonster_start = table.Column<DateOnly>(type: "date", nullable: false),
                    referensfonster_slut = table.Column<DateOnly>(type: "date", nullable: false),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    konverterings_datum = table.Column<DateOnly>(type: "date", nullable: true),
                    har_foretradesratt = table.Column<bool>(type: "boolean", nullable: false),
                    foretradesratt_utgar = table.Column<DateOnly>(type: "date", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_accumulations", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "audit_entries",
                schema: "audit",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EntityType = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    EntityId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Action = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    OldValues = table.Column<string>(type: "jsonb", nullable: true),
                    NewValues = table.Column<string>(type: "jsonb", nullable: true),
                    UserId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    UserName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IpAddress = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_entries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "benefits",
                schema: "benefits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Namn = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Beskrivning = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Kategori = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    MaxBelopp = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ArbetsgivarAndel = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    ArbetstagarAndel = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    ArSkattepliktig = table.Column<bool>(type: "boolean", nullable: false),
                    ArAktiv = table.Column<bool>(type: "boolean", nullable: false),
                    EligibilityRegler = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    SkapadVid = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_benefits", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "cases",
                schema: "case_mgmt",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    typ = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    anstalld_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    beskrivning = table.Column<string>(type: "text", nullable: false),
                    aktuellt_steg = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    tilldelad_till = table.Column<Guid>(type: "uuid", nullable: true),
                    slutford_vid = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    franvaro_data = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cases", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "certifications",
                schema: "competence",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AnstallId = table.Column<Guid>(type: "uuid", nullable: false),
                    Namn = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Typ = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Utfardare = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    GiltigFran = table.Column<DateOnly>(type: "date", nullable: true),
                    GiltigTill = table.Column<DateOnly>(type: "date", nullable: true),
                    ArObligatorisk = table.Column<bool>(type: "boolean", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_certifications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "communication_templates",
                schema: "recruitment",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Namn = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Typ = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Amne = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Brodtext = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    SkapadVid = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_communication_templates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "course_enrollments",
                schema: "lms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AnstallId = table.Column<Guid>(type: "uuid", nullable: false),
                    CourseId = table.Column<Guid>(type: "uuid", nullable: false),
                    Progress = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Resultat = table.Column<int>(type: "integer", nullable: true),
                    Godkand = table.Column<bool>(type: "boolean", nullable: false),
                    AnmalanVid = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PaborjadVid = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    GenomfordVid = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    GiltigTill = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_course_enrollments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "courses",
                schema: "lms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Namn = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Beskrivning = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    Format = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    LangdMinuter = table.Column<int>(type: "integer", nullable: false),
                    ArObligatorisk = table.Column<bool>(type: "boolean", nullable: false),
                    Kategori = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    GiltighetManader = table.Column<int>(type: "integer", nullable: true),
                    MaxDeltagare = table.Column<int>(type: "integer", nullable: false),
                    SkapadVid = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_courses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "custom_field_values",
                schema: "configuration",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomFieldId = table.Column<Guid>(type: "uuid", nullable: false),
                    EntityId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Varde = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    UppdateradVid = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_custom_field_values", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "custom_fields",
                schema: "configuration",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FieldName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    FieldType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Target = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ArObligatorisk = table.Column<bool>(type: "boolean", nullable: false),
                    Alternativ = table.Column<string>(type: "jsonb", nullable: true),
                    Standardvarde = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Ordning = table.Column<int>(type: "integer", nullable: false),
                    ArAktiv = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_custom_fields", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "dashboards",
                schema: "analytics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Namn = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    AgarId = table.Column<Guid>(type: "uuid", nullable: false),
                    Layout = table.Column<string>(type: "jsonb", nullable: false),
                    ArDelad = table.Column<bool>(type: "boolean", nullable: false),
                    SkapadVid = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dashboards", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "data_subject_requests",
                schema: "gdpr",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AnstallId = table.Column<Guid>(type: "uuid", nullable: false),
                    Typ = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Mottagen = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Deadline = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SlutfordVid = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    HandlaggarId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Kommentar = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ResultatFilSokvag = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_data_subject_requests", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "delegated_access",
                schema: "authorization",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DelegatorId = table.Column<Guid>(type: "uuid", nullable: false),
                    DelegatId = table.Column<Guid>(type: "uuid", nullable: false),
                    Roll = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    FranDatum = table.Column<DateOnly>(type: "date", nullable: false),
                    TillDatum = table.Column<DateOnly>(type: "date", nullable: false),
                    Anledning = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ArAktiv = table.Column<bool>(type: "boolean", nullable: false),
                    SkapadVid = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_delegated_access", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "document_signatures",
                schema: "documents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    SignerarId = table.Column<Guid>(type: "uuid", nullable: false),
                    Ordning = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    SigneradVid = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IPAdress = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    SkapadVid = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_document_signatures", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "document_templates",
                schema: "documents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Namn = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Kategori = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    MallInnehall = table.Column<string>(type: "character varying(10000)", maxLength: 10000, nullable: false),
                    MergeFields = table.Column<string>(type: "text", nullable: false),
                    SkapadVid = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_document_templates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "document_versions",
                schema: "documents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    VersionNummer = table.Column<int>(type: "integer", nullable: false),
                    StoragePath = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    FileSizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    SkapadAv = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    AndringsBeskrivning = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    SkapadVid = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_document_versions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "documents",
                schema: "documents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AnstallId = table.Column<Guid>(type: "uuid", nullable: false),
                    Kategori = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    FileName = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    StoragePath = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    FileSizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    ContentType = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Beskrivning = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    UppladdadVid = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UppladdadAv = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    RetentionUntil = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsArchived = table.Column<bool>(type: "boolean", nullable: false),
                    Klassificering = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_documents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "emergency_contacts",
                schema: "core_hr",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AnstallId = table.Column<Guid>(type: "uuid", nullable: false),
                    Namn = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Relation = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Telefon = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Epost = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ArPrimar = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_emergency_contacts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "employee_benefits",
                schema: "benefits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AnstallId = table.Column<Guid>(type: "uuid", nullable: false),
                    BenefitId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    StartDatum = table.Column<DateOnly>(type: "date", nullable: false),
                    SlutDatum = table.Column<DateOnly>(type: "date", nullable: true),
                    ValtBelopp = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    LivshandardAnledning = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    SkapadVid = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_employee_benefits", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "employees",
                schema: "core_hr",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    personnummer_encrypted = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: false),
                    fornamn = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    efternamn = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    mellan_namn = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    epost = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    telefon = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    gatuadress = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    postnummer = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    ort = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    land = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    clearingnummer_encrypted = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    kontonummer_encrypted = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    skattetabell = table.Column<int>(type: "integer", nullable: true),
                    skattekolumn = table.Column<int>(type: "integer", nullable: true),
                    kommun = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    kommunal_skattesats = table.Column<decimal>(type: "numeric", nullable: true),
                    har_kyrkoavgift = table.Column<bool>(type: "boolean", nullable: false),
                    kyrkoavgiftssats = table.Column<decimal>(type: "numeric", nullable: true),
                    har_jamkning = table.Column<bool>(type: "boolean", nullable: false),
                    jamkning_belopp = table.Column<decimal>(type: "numeric", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    updated_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_employees", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "field_permissions",
                schema: "authorization",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Roll = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EntityType = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    FieldName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    AccessLevel = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_field_permissions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "headcount_plans",
                schema: "positions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EnhetId = table.Column<Guid>(type: "uuid", nullable: false),
                    Ar = table.Column<int>(type: "integer", nullable: false),
                    BudgeteradePositioner = table.Column<int>(type: "integer", nullable: false),
                    BudgeteradFTE = table.Column<decimal>(type: "numeric(8,2)", nullable: false),
                    BudgeteradKostnad = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    FaktiskaPositioner = table.Column<int>(type: "integer", nullable: false),
                    FaktiskFTE = table.Column<decimal>(type: "numeric(8,2)", nullable: false),
                    FaktiskKostnad = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    SkapadVid = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_headcount_plans", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "interview_schedules",
                schema: "recruitment",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ApplicationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Tidpunkt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LangdMinuter = table.Column<int>(type: "integer", nullable: false),
                    Plats = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    InterviewerIds = table.Column<string>(type: "text", nullable: false),
                    Anteckningar = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    Genomford = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_interview_schedules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "learning_paths",
                schema: "lms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Namn = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Beskrivning = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    RollNamn = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    SkapadVid = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_learning_paths", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "leave_requests",
                schema: "leave",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AnstallId = table.Column<Guid>(type: "uuid", nullable: false),
                    Typ = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    FranDatum = table.Column<DateOnly>(type: "date", nullable: false),
                    TillDatum = table.Column<DateOnly>(type: "date", nullable: false),
                    AntalDagar = table.Column<int>(type: "integer", nullable: false),
                    Beskrivning = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    GodkandAv = table.Column<Guid>(type: "uuid", nullable: true),
                    GodkandVid = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Kommentar = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    SkapadVid = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_leave_requests", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "mandatory_trainings",
                schema: "competence",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RollNamn = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    UtbildningNamn = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    GiltighetManader = table.Column<int>(type: "integer", nullable: false),
                    Beskrivning = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mandatory_trainings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "notification_templates",
                schema: "notifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TemplateKey = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    TitleTemplate = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    MessageTemplate = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    DefaultType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    DefaultChannel = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notification_templates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "notifications",
                schema: "notifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Message = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Channel = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    IsRead = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ReadAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ActionUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    RelatedEntityType = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    RelatedEntityId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notifications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "offboarding_cases",
                schema: "offboarding",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AnstallId = table.Column<Guid>(type: "uuid", nullable: false),
                    Anledning = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    SistaArbetsdag = table.Column<DateOnly>(type: "date", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ExitSamtalKommentar = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    ExitSamtalGenomfort = table.Column<bool>(type: "boolean", nullable: false),
                    ArReHireEligible = table.Column<bool>(type: "boolean", nullable: false),
                    ReHireKommentar = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    SkapadVid = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SlutfordVid = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_offboarding_cases", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "onboarding_checklists",
                schema: "recruitment",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AnstallId = table.Column<Guid>(type: "uuid", nullable: false),
                    VakansId = table.Column<Guid>(type: "uuid", nullable: false),
                    Startdatum = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SkapadVid = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_onboarding_checklists", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "organization_units",
                schema: "core_hr",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    namn = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    typ = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    overordnad_enhet_id = table.Column<Guid>(type: "uuid", nullable: true),
                    kostnadsstalle = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    cfar_kod = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    chef_id = table.Column<Guid>(type: "uuid", nullable: true),
                    giltig_fran = table.Column<DateOnly>(type: "date", nullable: false),
                    giltig_till = table.Column<DateOnly>(type: "date", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    updated_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_organization_units", x => x.id);
                    table.ForeignKey(
                        name: "FK_organization_units_organization_units_overordnad_enhet_id",
                        column: x => x.overordnad_enhet_id,
                        principalSchema: "core_hr",
                        principalTable: "organization_units",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "OutboxMessages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Destination = table.Column<string>(type: "text", nullable: false),
                    MessageType = table.Column<string>(type: "text", nullable: false),
                    Payload = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RetryCount = table.Column<int>(type: "integer", nullable: false),
                    MaxRetries = table.Column<int>(type: "integer", nullable: false),
                    NextRetryAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastError = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutboxMessages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "payroll_runs",
                schema: "payroll",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ar = table.Column<int>(type: "integer", nullable: false),
                    manad = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    startad_vid = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    avslutad_vid = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    startad_av = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    godkand_av = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    antal_anstallda = table.Column<int>(type: "integer", nullable: false),
                    total_brutto = table.Column<decimal>(type: "numeric", nullable: false),
                    total_netto = table.Column<decimal>(type: "numeric", nullable: false),
                    total_skatt = table.Column<decimal>(type: "numeric", nullable: false),
                    total_arbetsgivaravgifter = table.Column<decimal>(type: "numeric", nullable: false),
                    ar_retroaktiv = table.Column<bool>(type: "boolean", nullable: false),
                    retroaktivt_for_period = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payroll_runs", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "performance_reviews",
                schema: "performance",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AnstallId = table.Column<Guid>(type: "uuid", nullable: false),
                    ChefId = table.Column<Guid>(type: "uuid", nullable: false),
                    Ar = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    SjalvBedomning = table.Column<string>(type: "jsonb", nullable: true),
                    ChefsBedomning = table.Column<string>(type: "jsonb", nullable: true),
                    OverallRating = table.Column<int>(type: "integer", nullable: true),
                    Malsattning = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    Kommentar = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    SkapadVid = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    GenomfordVid = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_performance_reviews", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "positions",
                schema: "positions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EnhetId = table.Column<Guid>(type: "uuid", nullable: false),
                    Titel = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    BESTAKod = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    AIDKod = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    BudgeteradManadslon = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Sysselsattningsgrad = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    InnehavareAnstallId = table.Column<Guid>(type: "uuid", nullable: true),
                    EftertradarePlanerad = table.Column<Guid>(type: "uuid", nullable: true),
                    SkapadVid = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AvveckladVid = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    KravdaKompetenser = table.Column<List<string>>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_positions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RehabCases",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AnstallId = table.Column<Guid>(type: "uuid", nullable: false),
                    Trigger = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ArendeagareHR = table.Column<Guid>(type: "uuid", nullable: true),
                    SkapadVid = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RehabPlan = table.Column<string>(type: "text", nullable: true),
                    Uppfoljning14Dagar = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Uppfoljning90Dagar = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Uppfoljning180Dagar = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Uppfoljning365Dagar = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    GallringsDatum = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    Version = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RehabCases", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "report_definitions",
                schema: "reporting",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Namn = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Beskrivning = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Typ = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ParameterSchema = table.Column<string>(type: "jsonb", nullable: true),
                    ArSchemalagd = table.Column<bool>(type: "boolean", nullable: false),
                    CronExpression = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    MottagareEpost = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_report_definitions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "report_executions",
                schema: "reporting",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReportDefinitionId = table.Column<Guid>(type: "uuid", nullable: false),
                    StartadVid = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SlutfordVid = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ResultatFilSokvag = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    FelMeddelande = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Parametrar = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_report_executions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "requisition_approvals",
                schema: "recruitment",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VakansId = table.Column<Guid>(type: "uuid", nullable: false),
                    GodkannareId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Kommentar = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    SkapadVid = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    BeslutVid = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_requisition_approvals", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "retention_records",
                schema: "gdpr",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EntityType = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    EntityId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    RetentionExpires = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RetentionReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    IsAnonymized = table.Column<bool>(type: "boolean", nullable: false),
                    AnonymizedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_retention_records", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "salary_codes",
                schema: "payroll",
                columns: table => new
                {
                    kod = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    benamning = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    skattekategori = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ar_semestergrundande = table.Column<bool>(type: "boolean", nullable: false),
                    ar_pensionsgrundande = table.Column<bool>(type: "boolean", nullable: false),
                    ar_ob_grundande = table.Column<bool>(type: "boolean", nullable: false),
                    agi_faltkod = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    ar_avdrag = table.Column<bool>(type: "boolean", nullable: false),
                    ar_aktiv = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_salary_codes", x => x.kod);
                });

            migrationBuilder.CreateTable(
                name: "SalaryReviewRounds",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Namn = table.Column<string>(type: "text", nullable: false),
                    Ar = table.Column<int>(type: "integer", nullable: false),
                    Avtalsomrade = table.Column<int>(type: "integer", nullable: false),
                    TotalBudget = table.Column<decimal>(type: "numeric", nullable: false),
                    FordeladBudget = table.Column<decimal>(type: "numeric", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    IkrafttradandeDatum = table.Column<DateOnly>(type: "date", nullable: false),
                    FackligRepresentant = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    Version = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalaryReviewRounds", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "saved_reports",
                schema: "analytics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Namn = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Beskrivning = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    SkapadAvId = table.Column<Guid>(type: "uuid", nullable: false),
                    QueryDefinition = table.Column<string>(type: "jsonb", nullable: false),
                    Visualisering = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    ArDelad = table.Column<bool>(type: "boolean", nullable: false),
                    SkapadVid = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SenastKordVid = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_saved_reports", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "schedules",
                schema: "scheduling",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    enhet_id = table.Column<Guid>(type: "uuid", nullable: false),
                    namn = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    typ = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    period_start = table.Column<DateOnly>(type: "date", nullable: false),
                    period_slut = table.Column<DateOnly>(type: "date", nullable: true),
                    cykel_langd_veckor = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_schedules", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "scorecards",
                schema: "recruitment",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ApplicationId = table.Column<Guid>(type: "uuid", nullable: false),
                    BedomareId = table.Column<Guid>(type: "uuid", nullable: false),
                    KompetensPoang = table.Column<int>(type: "integer", nullable: false),
                    ErfarenhetsPoang = table.Column<int>(type: "integer", nullable: false),
                    PersonlighetPoang = table.Column<int>(type: "integer", nullable: false),
                    MotivationPoang = table.Column<int>(type: "integer", nullable: false),
                    Kommentar = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    Rekommendation = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    SkapadVid = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_scorecards", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "shift_swap_requests",
                schema: "scheduling",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BegardAv = table.Column<Guid>(type: "uuid", nullable: false),
                    ErbjodsAv = table.Column<Guid>(type: "uuid", nullable: true),
                    UrsprungligtPassId = table.Column<Guid>(type: "uuid", nullable: false),
                    ErsattningsPassId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Motivering = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    SkapadVid = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    GodkannareId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    AvvisningsAnledning = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    HandlagdVid = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    Version = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_shift_swap_requests", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "sick_leave_notifications",
                schema: "leave",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AnstallId = table.Column<Guid>(type: "uuid", nullable: false),
                    StartDatum = table.Column<DateOnly>(type: "date", nullable: false),
                    SlutDatum = table.Column<DateOnly>(type: "date", nullable: true),
                    SjukDag = table.Column<int>(type: "integer", nullable: false),
                    LakarintygKravs = table.Column<bool>(type: "boolean", nullable: false),
                    FKAnmalanKravs = table.Column<bool>(type: "boolean", nullable: false),
                    LakarintygInlamnat = table.Column<bool>(type: "boolean", nullable: false),
                    FKAnmalanGjord = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sick_leave_notifications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "staffing_templates",
                schema: "scheduling",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EnhetId = table.Column<Guid>(type: "uuid", nullable: false),
                    Namn = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    giltighet_fran = table.Column<DateOnly>(type: "date", nullable: false),
                    giltighet_till = table.Column<DateOnly>(type: "date", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    Version = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_staffing_templates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "talent_pool",
                schema: "recruitment",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Namn = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Epost = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Telefon = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    KompetensOmrade = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Anteckningar = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    UrsprungsAnsokanId = table.Column<Guid>(type: "uuid", nullable: true),
                    SkapadVid = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_talent_pool", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "tax_tables",
                schema: "payroll",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ar = table.Column<int>(type: "integer", nullable: false),
                    tabellnummer = table.Column<int>(type: "integer", nullable: false),
                    kolumn = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tax_tables", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "tenant_configurations",
                schema: "configuration",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantNamn = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Organisationsnummer = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Land = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    Sprak = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Valuta = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    LogoUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Konfiguration = table.Column<string>(type: "jsonb", nullable: true),
                    ArAktiv = table.Column<bool>(type: "boolean", nullable: false),
                    SkapadVid = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tenant_configurations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "time_clock_events",
                schema: "scheduling",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    anstalld_id = table.Column<Guid>(type: "uuid", nullable: false),
                    typ = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    tidpunkt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    kalla = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ip_adress = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    latitud = table.Column<double>(type: "double precision", nullable: true),
                    longitud = table.Column<double>(type: "double precision", nullable: true),
                    ar_offline_stampling = table.Column<bool>(type: "boolean", nullable: false),
                    synkad_vid = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    kopplat_pass_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_time_clock_events", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "timesheets",
                schema: "scheduling",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    anstalld_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ar = table.Column<int>(type: "integer", nullable: false),
                    manad = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    planerade_timmar = table.Column<decimal>(type: "numeric", nullable: false),
                    faktiska_timmar = table.Column<decimal>(type: "numeric", nullable: false),
                    overtid = table.Column<decimal>(type: "numeric", nullable: false),
                    godkand_av = table.Column<Guid>(type: "uuid", nullable: true),
                    godkand_vid = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    skapad_vid = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    kommentar = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_timesheets", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "TravelClaims",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AnstallId = table.Column<Guid>(type: "uuid", nullable: false),
                    Beskrivning = table.Column<string>(type: "text", nullable: false),
                    ReseDatum = table.Column<DateOnly>(type: "date", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    TotalBelopp = table.Column<decimal>(type: "numeric", nullable: false),
                    HelaDagar = table.Column<int>(type: "integer", nullable: true),
                    HalvaDagar = table.Column<int>(type: "integer", nullable: true),
                    Traktamente = table.Column<decimal>(type: "numeric", nullable: true),
                    KordaMil = table.Column<decimal>(type: "numeric", nullable: true),
                    Milersattning = table.Column<decimal>(type: "numeric", nullable: true),
                    AttesteradAv = table.Column<string>(type: "text", nullable: true),
                    AttesteradVid = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AvvisningsAnledning = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    Version = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TravelClaims", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Vacancies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EnhetId = table.Column<Guid>(type: "uuid", nullable: false),
                    Titel = table.Column<string>(type: "text", nullable: false),
                    Beskrivning = table.Column<string>(type: "text", nullable: false),
                    Anstallningsform = table.Column<int>(type: "integer", nullable: false),
                    Lonespann_Min = table.Column<decimal>(type: "numeric", nullable: true),
                    Lonespann_Max = table.Column<decimal>(type: "numeric", nullable: true),
                    SistaAnsokningsDag = table.Column<DateOnly>(type: "date", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    PubliceradExternt = table.Column<bool>(type: "boolean", nullable: false),
                    PubliceradPlatsbanken = table.Column<bool>(type: "boolean", nullable: false),
                    TillsattAnsokanId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    Version = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vacancies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "vacation_balances",
                schema: "leave",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AnstallId = table.Column<Guid>(type: "uuid", nullable: false),
                    Ar = table.Column<int>(type: "integer", nullable: false),
                    IntjanadeDagar = table.Column<int>(type: "integer", nullable: false),
                    UttagnaDagar = table.Column<int>(type: "integer", nullable: false),
                    SparadeDagar = table.Column<int>(type: "integer", nullable: false),
                    Tilldelning = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vacation_balances", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "workflow_definitions",
                schema: "configuration",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Namn = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    TargetEntityType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    StegDefinition = table.Column<string>(type: "jsonb", nullable: false),
                    ArAktiv = table.Column<bool>(type: "boolean", nullable: false),
                    SkapadVid = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_workflow_definitions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "events",
                schema: "las",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    typ = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    tidpunkt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    beskrivning = table.Column<string>(type: "text", nullable: false),
                    accumulation_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_events", x => x.id);
                    table.ForeignKey(
                        name: "FK_events_accumulations_accumulation_id",
                        column: x => x.accumulation_id,
                        principalSchema: "las",
                        principalTable: "accumulations",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "periods",
                schema: "las",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    start_datum = table.Column<DateOnly>(type: "date", nullable: false),
                    slut_datum = table.Column<DateOnly>(type: "date", nullable: false),
                    antal_dagar = table.Column<int>(type: "integer", nullable: false),
                    anstallnings_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    accumulation_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_periods", x => x.Id);
                    table.ForeignKey(
                        name: "FK_periods_accumulations_accumulation_id",
                        column: x => x.accumulation_id,
                        principalSchema: "las",
                        principalTable: "accumulations",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "CaseApproval",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Steg = table.Column<string>(type: "text", nullable: false),
                    GodkannareId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    BeslutVid = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Kommentar = table.Column<string>(type: "text", nullable: true),
                    case_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CaseApproval", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CaseApproval_cases_case_id",
                        column: x => x.case_id,
                        principalSchema: "case_mgmt",
                        principalTable: "cases",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "CaseComment",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ForfattareId = table.Column<Guid>(type: "uuid", nullable: false),
                    Text = table.Column<string>(type: "text", nullable: false),
                    SkapadVid = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    case_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CaseComment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CaseComment_cases_case_id",
                        column: x => x.case_id,
                        principalSchema: "case_mgmt",
                        principalTable: "cases",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "employments",
                schema: "core_hr",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    anstalld_id = table.Column<Guid>(type: "uuid", nullable: false),
                    enhet_id = table.Column<Guid>(type: "uuid", nullable: false),
                    anstallningsform = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    kollektivavtal = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    manadslon = table.Column<decimal>(type: "numeric", nullable: false),
                    sysselsattningsgrad = table.Column<decimal>(type: "numeric", nullable: false),
                    start_datum = table.Column<DateOnly>(type: "date", nullable: false),
                    slut_datum = table.Column<DateOnly>(type: "date", nullable: true),
                    besta_kod = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    aid_kod = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    befattningstitel = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    updated_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_employments", x => x.id);
                    table.ForeignKey(
                        name: "FK_employments_employees_anstalld_id",
                        column: x => x.anstalld_id,
                        principalSchema: "core_hr",
                        principalTable: "employees",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "learning_path_steps",
                schema: "lms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CourseId = table.Column<Guid>(type: "uuid", nullable: false),
                    Ordning = table.Column<int>(type: "integer", nullable: false),
                    Obligatorisk = table.Column<bool>(type: "boolean", nullable: false),
                    LearningPathId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_learning_path_steps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_learning_path_steps_learning_paths_LearningPathId",
                        column: x => x.LearningPathId,
                        principalSchema: "lms",
                        principalTable: "learning_paths",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "offboarding_items",
                schema: "offboarding",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Beskrivning = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Klar = table.Column<bool>(type: "boolean", nullable: false),
                    KlarVid = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Kommentar = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    OffboardingCaseId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_offboarding_items", x => x.Id);
                    table.ForeignKey(
                        name: "FK_offboarding_items_offboarding_cases_OffboardingCaseId",
                        column: x => x.OffboardingCaseId,
                        principalSchema: "offboarding",
                        principalTable: "offboarding_cases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "onboarding_items",
                schema: "recruitment",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Beskrivning = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Klar = table.Column<bool>(type: "boolean", nullable: false),
                    KlarVid = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    OnboardingChecklistId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_onboarding_items", x => x.Id);
                    table.ForeignKey(
                        name: "FK_onboarding_items_onboarding_checklists_OnboardingChecklistId",
                        column: x => x.OnboardingChecklistId,
                        principalSchema: "recruitment",
                        principalTable: "onboarding_checklists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "payroll_results",
                schema: "payroll",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    kornings_id = table.Column<Guid>(type: "uuid", nullable: false),
                    anstalld_id = table.Column<Guid>(type: "uuid", nullable: false),
                    anstallnings_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ar = table.Column<int>(type: "integer", nullable: false),
                    manad = table.Column<int>(type: "integer", nullable: false),
                    manadslon = table.Column<decimal>(type: "numeric", nullable: false),
                    sysselsattningsgrad = table.Column<decimal>(type: "numeric", nullable: false),
                    kollektivavtal = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    brutto = table.Column<decimal>(type: "numeric", nullable: false),
                    skatteplikt_brutto = table.Column<decimal>(type: "numeric", nullable: false),
                    skatt = table.Column<decimal>(type: "numeric", nullable: false),
                    netto = table.Column<decimal>(type: "numeric", nullable: false),
                    arbetsgivaravgifter = table.Column<decimal>(type: "numeric", nullable: false),
                    arbetsgivaravgift_sats = table.Column<decimal>(type: "numeric", nullable: false),
                    semesterlon = table.Column<decimal>(type: "numeric", nullable: false),
                    semestertillagg = table.Column<decimal>(type: "numeric", nullable: false),
                    semesterdagar_intjanade = table.Column<int>(type: "integer", nullable: false),
                    semesterdagar_uttagna = table.Column<int>(type: "integer", nullable: false),
                    pensionsgrundande = table.Column<decimal>(type: "numeric", nullable: false),
                    pensionsavgift = table.Column<decimal>(type: "numeric", nullable: false),
                    ob_tillagg = table.Column<decimal>(type: "numeric", nullable: false),
                    overtidstillagg = table.Column<decimal>(type: "numeric", nullable: false),
                    JourTillagg = table.Column<decimal>(type: "numeric", nullable: false),
                    BeredskapsTillagg = table.Column<decimal>(type: "numeric", nullable: false),
                    sjuklon = table.Column<decimal>(type: "numeric", nullable: false),
                    karensavdrag = table.Column<decimal>(type: "numeric", nullable: false),
                    ForaldraloneUtfyllnad = table.Column<decimal>(type: "numeric", nullable: false),
                    loneutmatning = table.Column<decimal>(type: "numeric", nullable: false),
                    fackavgift = table.Column<decimal>(type: "numeric", nullable: false),
                    ovriga_avdrag = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payroll_results", x => x.id);
                    table.ForeignKey(
                        name: "FK_payroll_results_payroll_runs_kornings_id",
                        column: x => x.kornings_id,
                        principalSchema: "payroll",
                        principalTable: "payroll_runs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "position_historik",
                schema: "positions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PositionId = table.Column<Guid>(type: "uuid", nullable: false),
                    TidigareInnehavare = table.Column<Guid>(type: "uuid", nullable: true),
                    NyInnehavare = table.Column<Guid>(type: "uuid", nullable: true),
                    AndradVid = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Anledning = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_position_historik", x => x.Id);
                    table.ForeignKey(
                        name: "FK_position_historik_positions_PositionId",
                        column: x => x.PositionId,
                        principalSchema: "positions",
                        principalTable: "positions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RehabNote",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Text = table.Column<string>(type: "text", nullable: false),
                    ForfattareId = table.Column<Guid>(type: "uuid", nullable: false),
                    SkapadVid = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RehabCaseId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RehabNote", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RehabNote_RehabCases_RehabCaseId",
                        column: x => x.RehabCaseId,
                        principalTable: "RehabCases",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "RehabUppfoljning",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DagNr = table.Column<int>(type: "integer", nullable: false),
                    UtfordVid = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Kommentar = table.Column<string>(type: "text", nullable: false),
                    UtfordAv = table.Column<Guid>(type: "uuid", nullable: false),
                    RehabCaseId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RehabUppfoljning", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RehabUppfoljning_RehabCases_RehabCaseId",
                        column: x => x.RehabCaseId,
                        principalTable: "RehabCases",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SalaryProposal",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AnstallId = table.Column<Guid>(type: "uuid", nullable: false),
                    NuvarandeLon = table.Column<decimal>(type: "numeric", nullable: false),
                    ForeslagenLon = table.Column<decimal>(type: "numeric", nullable: false),
                    Okning = table.Column<decimal>(type: "numeric", nullable: false),
                    OkningProcent = table.Column<decimal>(type: "numeric", nullable: false),
                    Motivering = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    AvvisningsAnledning = table.Column<string>(type: "text", nullable: true),
                    SalaryReviewRoundId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalaryProposal", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SalaryProposal_SalaryReviewRounds_SalaryReviewRoundId",
                        column: x => x.SalaryReviewRoundId,
                        principalTable: "SalaryReviewRounds",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "scheduled_shifts",
                schema: "scheduling",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    schema_id = table.Column<Guid>(type: "uuid", nullable: false),
                    anstalld_id = table.Column<Guid>(type: "uuid", nullable: false),
                    datum = table.Column<DateOnly>(type: "date", nullable: false),
                    pass_typ = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    planerad_start = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    planerad_slut = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    rast = table.Column<TimeSpan>(type: "interval", nullable: false),
                    faktisk_start = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    faktisk_slut = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ob_kategori = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Avvikelse = table.Column<int>(type: "integer", nullable: true),
                    AvvikelseBeskrivning = table.Column<string>(type: "text", nullable: true),
                    OvertidTimmar = table.Column<decimal>(type: "numeric", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_scheduled_shifts", x => x.id);
                    table.ForeignKey(
                        name: "FK_scheduled_shifts_schedules_schema_id",
                        column: x => x.schema_id,
                        principalSchema: "scheduling",
                        principalTable: "schedules",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "staffing_template_lines",
                schema: "scheduling",
                columns: table => new
                {
                    StaffingTemplateId = table.Column<Guid>(type: "uuid", nullable: false),
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Veckodag = table.Column<int>(type: "integer", nullable: false),
                    PassTyp = table.Column<int>(type: "integer", nullable: false),
                    Start = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    Slut = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    Rast = table.Column<TimeSpan>(type: "interval", nullable: false),
                    MinAntal = table.Column<int>(type: "integer", nullable: false),
                    OptimalAntal = table.Column<int>(type: "integer", nullable: false),
                    KravdaKompetenser = table.Column<List<string>>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_staffing_template_lines", x => new { x.StaffingTemplateId, x.Id });
                    table.ForeignKey(
                        name: "FK_staffing_template_lines_staffing_templates_StaffingTemplate~",
                        column: x => x.StaffingTemplateId,
                        principalSchema: "scheduling",
                        principalTable: "staffing_templates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tax_table_rows",
                schema: "payroll",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    inkomst_fran = table.Column<decimal>(type: "numeric", nullable: false),
                    inkomst_till = table.Column<decimal>(type: "numeric", nullable: false),
                    skattebelopp = table.Column<decimal>(type: "numeric", nullable: false),
                    tax_table_id = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tax_table_rows", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tax_table_rows_tax_tables_tax_table_id",
                        column: x => x.tax_table_id,
                        principalSchema: "payroll",
                        principalTable: "tax_tables",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ExpenseItem",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Beskrivning = table.Column<string>(type: "text", nullable: false),
                    Belopp = table.Column<decimal>(type: "numeric", nullable: false),
                    KvittoBildId = table.Column<string>(type: "text", nullable: true),
                    TravelClaimId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExpenseItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExpenseItem_TravelClaims_TravelClaimId",
                        column: x => x.TravelClaimId,
                        principalTable: "TravelClaims",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Application",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Namn = table.Column<string>(type: "text", nullable: false),
                    Epost = table.Column<string>(type: "text", nullable: false),
                    CVFilId = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Poang = table.Column<int>(type: "integer", nullable: true),
                    BedomningsKommentar = table.Column<string>(type: "text", nullable: true),
                    IntervjuTidpunkt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    InkomVid = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    VacancyId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Application", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Application_Vacancies_VacancyId",
                        column: x => x.VacancyId,
                        principalTable: "Vacancies",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "payroll_result_lines",
                schema: "payroll",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    loneart_kod = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    benamning = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    antal = table.Column<decimal>(type: "numeric", nullable: false),
                    sats = table.Column<decimal>(type: "numeric", nullable: false),
                    belopp = table.Column<decimal>(type: "numeric", nullable: false),
                    skattekategori = table.Column<int>(type: "integer", nullable: false),
                    ar_semestergrundande = table.Column<bool>(type: "boolean", nullable: false),
                    ar_pensionsgrundande = table.Column<bool>(type: "boolean", nullable: false),
                    kostnadsstalle = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    projekt = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    agi_faltkod = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    ar_avdrag = table.Column<bool>(type: "boolean", nullable: false),
                    result_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payroll_result_lines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_payroll_result_lines_payroll_results_result_id",
                        column: x => x.result_id,
                        principalSchema: "payroll",
                        principalTable: "payroll_results",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Application_VacancyId",
                table: "Application",
                column: "VacancyId");

            migrationBuilder.CreateIndex(
                name: "IX_audit_entries_EntityType",
                schema: "audit",
                table: "audit_entries",
                column: "EntityType");

            migrationBuilder.CreateIndex(
                name: "IX_audit_entries_EntityType_EntityId",
                schema: "audit",
                table: "audit_entries",
                columns: new[] { "EntityType", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_audit_entries_Timestamp",
                schema: "audit",
                table: "audit_entries",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_benefits_ArAktiv",
                schema: "benefits",
                table: "benefits",
                column: "ArAktiv");

            migrationBuilder.CreateIndex(
                name: "IX_benefits_Kategori",
                schema: "benefits",
                table: "benefits",
                column: "Kategori");

            migrationBuilder.CreateIndex(
                name: "IX_CaseApproval_case_id",
                table: "CaseApproval",
                column: "case_id");

            migrationBuilder.CreateIndex(
                name: "IX_CaseComment_case_id",
                table: "CaseComment",
                column: "case_id");

            migrationBuilder.CreateIndex(
                name: "IX_certifications_AnstallId",
                schema: "competence",
                table: "certifications",
                column: "AnstallId");

            migrationBuilder.CreateIndex(
                name: "IX_certifications_GiltigTill",
                schema: "competence",
                table: "certifications",
                column: "GiltigTill");

            migrationBuilder.CreateIndex(
                name: "IX_course_enrollments_AnstallId",
                schema: "lms",
                table: "course_enrollments",
                column: "AnstallId");

            migrationBuilder.CreateIndex(
                name: "IX_course_enrollments_CourseId",
                schema: "lms",
                table: "course_enrollments",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_course_enrollments_GiltigTill",
                schema: "lms",
                table: "course_enrollments",
                column: "GiltigTill");

            migrationBuilder.CreateIndex(
                name: "IX_course_enrollments_Progress",
                schema: "lms",
                table: "course_enrollments",
                column: "Progress");

            migrationBuilder.CreateIndex(
                name: "IX_courses_ArObligatorisk",
                schema: "lms",
                table: "courses",
                column: "ArObligatorisk");

            migrationBuilder.CreateIndex(
                name: "IX_courses_Status",
                schema: "lms",
                table: "courses",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_custom_field_values_CustomFieldId",
                schema: "configuration",
                table: "custom_field_values",
                column: "CustomFieldId");

            migrationBuilder.CreateIndex(
                name: "IX_custom_field_values_CustomFieldId_EntityId",
                schema: "configuration",
                table: "custom_field_values",
                columns: new[] { "CustomFieldId", "EntityId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_custom_fields_FieldName_Target",
                schema: "configuration",
                table: "custom_fields",
                columns: new[] { "FieldName", "Target" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_custom_fields_Target",
                schema: "configuration",
                table: "custom_fields",
                column: "Target");

            migrationBuilder.CreateIndex(
                name: "IX_dashboards_AgarId",
                schema: "analytics",
                table: "dashboards",
                column: "AgarId");

            migrationBuilder.CreateIndex(
                name: "IX_dashboards_ArDelad",
                schema: "analytics",
                table: "dashboards",
                column: "ArDelad");

            migrationBuilder.CreateIndex(
                name: "IX_data_subject_requests_AnstallId",
                schema: "gdpr",
                table: "data_subject_requests",
                column: "AnstallId");

            migrationBuilder.CreateIndex(
                name: "IX_data_subject_requests_Deadline",
                schema: "gdpr",
                table: "data_subject_requests",
                column: "Deadline");

            migrationBuilder.CreateIndex(
                name: "IX_data_subject_requests_Status",
                schema: "gdpr",
                table: "data_subject_requests",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_delegated_access_DelegatId",
                schema: "authorization",
                table: "delegated_access",
                column: "DelegatId");

            migrationBuilder.CreateIndex(
                name: "IX_delegated_access_DelegatId_ArAktiv",
                schema: "authorization",
                table: "delegated_access",
                columns: new[] { "DelegatId", "ArAktiv" });

            migrationBuilder.CreateIndex(
                name: "IX_delegated_access_DelegatorId",
                schema: "authorization",
                table: "delegated_access",
                column: "DelegatorId");

            migrationBuilder.CreateIndex(
                name: "IX_document_signatures_DocumentId",
                schema: "documents",
                table: "document_signatures",
                column: "DocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_document_signatures_SignerarId",
                schema: "documents",
                table: "document_signatures",
                column: "SignerarId");

            migrationBuilder.CreateIndex(
                name: "IX_document_templates_Kategori",
                schema: "documents",
                table: "document_templates",
                column: "Kategori");

            migrationBuilder.CreateIndex(
                name: "IX_document_versions_DocumentId",
                schema: "documents",
                table: "document_versions",
                column: "DocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_document_versions_DocumentId_VersionNummer",
                schema: "documents",
                table: "document_versions",
                columns: new[] { "DocumentId", "VersionNummer" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_documents_AnstallId",
                schema: "documents",
                table: "documents",
                column: "AnstallId");

            migrationBuilder.CreateIndex(
                name: "IX_documents_Kategori",
                schema: "documents",
                table: "documents",
                column: "Kategori");

            migrationBuilder.CreateIndex(
                name: "IX_documents_RetentionUntil",
                schema: "documents",
                table: "documents",
                column: "RetentionUntil");

            migrationBuilder.CreateIndex(
                name: "IX_emergency_contacts_AnstallId",
                schema: "core_hr",
                table: "emergency_contacts",
                column: "AnstallId");

            migrationBuilder.CreateIndex(
                name: "IX_employee_benefits_AnstallId",
                schema: "benefits",
                table: "employee_benefits",
                column: "AnstallId");

            migrationBuilder.CreateIndex(
                name: "IX_employee_benefits_BenefitId",
                schema: "benefits",
                table: "employee_benefits",
                column: "BenefitId");

            migrationBuilder.CreateIndex(
                name: "IX_employee_benefits_Status",
                schema: "benefits",
                table: "employee_benefits",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_employments_anstalld_id",
                schema: "core_hr",
                table: "employments",
                column: "anstalld_id");

            migrationBuilder.CreateIndex(
                name: "IX_events_accumulation_id",
                schema: "las",
                table: "events",
                column: "accumulation_id");

            migrationBuilder.CreateIndex(
                name: "IX_ExpenseItem_TravelClaimId",
                table: "ExpenseItem",
                column: "TravelClaimId");

            migrationBuilder.CreateIndex(
                name: "IX_field_permissions_Roll",
                schema: "authorization",
                table: "field_permissions",
                column: "Roll");

            migrationBuilder.CreateIndex(
                name: "IX_field_permissions_Roll_EntityType",
                schema: "authorization",
                table: "field_permissions",
                columns: new[] { "Roll", "EntityType" });

            migrationBuilder.CreateIndex(
                name: "IX_headcount_plans_EnhetId_Ar",
                schema: "positions",
                table: "headcount_plans",
                columns: new[] { "EnhetId", "Ar" });

            migrationBuilder.CreateIndex(
                name: "IX_interview_schedules_ApplicationId",
                schema: "recruitment",
                table: "interview_schedules",
                column: "ApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_learning_path_steps_CourseId",
                schema: "lms",
                table: "learning_path_steps",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_learning_path_steps_LearningPathId",
                schema: "lms",
                table: "learning_path_steps",
                column: "LearningPathId");

            migrationBuilder.CreateIndex(
                name: "IX_leave_requests_AnstallId",
                schema: "leave",
                table: "leave_requests",
                column: "AnstallId");

            migrationBuilder.CreateIndex(
                name: "IX_leave_requests_Status",
                schema: "leave",
                table: "leave_requests",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_notification_templates_TemplateKey",
                schema: "notifications",
                table: "notification_templates",
                column: "TemplateKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_notifications_CreatedAt",
                schema: "notifications",
                table: "notifications",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_notifications_UserId_IsRead",
                schema: "notifications",
                table: "notifications",
                columns: new[] { "UserId", "IsRead" });

            migrationBuilder.CreateIndex(
                name: "IX_offboarding_cases_AnstallId",
                schema: "offboarding",
                table: "offboarding_cases",
                column: "AnstallId");

            migrationBuilder.CreateIndex(
                name: "IX_offboarding_items_OffboardingCaseId",
                schema: "offboarding",
                table: "offboarding_items",
                column: "OffboardingCaseId");

            migrationBuilder.CreateIndex(
                name: "IX_onboarding_checklists_AnstallId",
                schema: "recruitment",
                table: "onboarding_checklists",
                column: "AnstallId");

            migrationBuilder.CreateIndex(
                name: "IX_onboarding_checklists_VakansId",
                schema: "recruitment",
                table: "onboarding_checklists",
                column: "VakansId");

            migrationBuilder.CreateIndex(
                name: "IX_onboarding_items_OnboardingChecklistId",
                schema: "recruitment",
                table: "onboarding_items",
                column: "OnboardingChecklistId");

            migrationBuilder.CreateIndex(
                name: "IX_organization_units_overordnad_enhet_id",
                schema: "core_hr",
                table: "organization_units",
                column: "overordnad_enhet_id");

            migrationBuilder.CreateIndex(
                name: "IX_payroll_result_lines_result_id",
                schema: "payroll",
                table: "payroll_result_lines",
                column: "result_id");

            migrationBuilder.CreateIndex(
                name: "IX_payroll_results_kornings_id",
                schema: "payroll",
                table: "payroll_results",
                column: "kornings_id");

            migrationBuilder.CreateIndex(
                name: "IX_performance_reviews_AnstallId_Ar",
                schema: "performance",
                table: "performance_reviews",
                columns: new[] { "AnstallId", "Ar" });

            migrationBuilder.CreateIndex(
                name: "IX_performance_reviews_ChefId",
                schema: "performance",
                table: "performance_reviews",
                column: "ChefId");

            migrationBuilder.CreateIndex(
                name: "IX_periods_accumulation_id",
                schema: "las",
                table: "periods",
                column: "accumulation_id");

            migrationBuilder.CreateIndex(
                name: "IX_position_historik_PositionId",
                schema: "positions",
                table: "position_historik",
                column: "PositionId");

            migrationBuilder.CreateIndex(
                name: "IX_positions_EnhetId",
                schema: "positions",
                table: "positions",
                column: "EnhetId");

            migrationBuilder.CreateIndex(
                name: "IX_positions_InnehavareAnstallId",
                schema: "positions",
                table: "positions",
                column: "InnehavareAnstallId");

            migrationBuilder.CreateIndex(
                name: "IX_RehabNote_RehabCaseId",
                table: "RehabNote",
                column: "RehabCaseId");

            migrationBuilder.CreateIndex(
                name: "IX_RehabUppfoljning_RehabCaseId",
                table: "RehabUppfoljning",
                column: "RehabCaseId");

            migrationBuilder.CreateIndex(
                name: "IX_report_executions_ReportDefinitionId",
                schema: "reporting",
                table: "report_executions",
                column: "ReportDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_report_executions_Status",
                schema: "reporting",
                table: "report_executions",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_requisition_approvals_GodkannareId",
                schema: "recruitment",
                table: "requisition_approvals",
                column: "GodkannareId");

            migrationBuilder.CreateIndex(
                name: "IX_requisition_approvals_VakansId",
                schema: "recruitment",
                table: "requisition_approvals",
                column: "VakansId");

            migrationBuilder.CreateIndex(
                name: "IX_retention_records_EntityType_EntityId",
                schema: "gdpr",
                table: "retention_records",
                columns: new[] { "EntityType", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_retention_records_RetentionExpires",
                schema: "gdpr",
                table: "retention_records",
                column: "RetentionExpires");

            migrationBuilder.CreateIndex(
                name: "IX_SalaryProposal_SalaryReviewRoundId",
                table: "SalaryProposal",
                column: "SalaryReviewRoundId");

            migrationBuilder.CreateIndex(
                name: "IX_saved_reports_ArDelad",
                schema: "analytics",
                table: "saved_reports",
                column: "ArDelad");

            migrationBuilder.CreateIndex(
                name: "IX_saved_reports_SkapadAvId",
                schema: "analytics",
                table: "saved_reports",
                column: "SkapadAvId");

            migrationBuilder.CreateIndex(
                name: "IX_scheduled_shifts_schema_id",
                schema: "scheduling",
                table: "scheduled_shifts",
                column: "schema_id");

            migrationBuilder.CreateIndex(
                name: "IX_scorecards_ApplicationId",
                schema: "recruitment",
                table: "scorecards",
                column: "ApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_scorecards_BedomareId",
                schema: "recruitment",
                table: "scorecards",
                column: "BedomareId");

            migrationBuilder.CreateIndex(
                name: "IX_sick_leave_notifications_AnstallId",
                schema: "leave",
                table: "sick_leave_notifications",
                column: "AnstallId");

            migrationBuilder.CreateIndex(
                name: "IX_talent_pool_Epost",
                schema: "recruitment",
                table: "talent_pool",
                column: "Epost");

            migrationBuilder.CreateIndex(
                name: "IX_tax_table_rows_tax_table_id",
                schema: "payroll",
                table: "tax_table_rows",
                column: "tax_table_id");

            migrationBuilder.CreateIndex(
                name: "IX_tax_tables_ar_tabellnummer_kolumn",
                schema: "payroll",
                table: "tax_tables",
                columns: new[] { "ar", "tabellnummer", "kolumn" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tenant_configurations_Organisationsnummer",
                schema: "configuration",
                table: "tenant_configurations",
                column: "Organisationsnummer",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_timesheets_anstalld_id_ar_manad",
                schema: "scheduling",
                table: "timesheets",
                columns: new[] { "anstalld_id", "ar", "manad" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_vacation_balances_AnstallId_Ar",
                schema: "leave",
                table: "vacation_balances",
                columns: new[] { "AnstallId", "Ar" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_workflow_definitions_TargetEntityType",
                schema: "configuration",
                table: "workflow_definitions",
                column: "TargetEntityType");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Application");

            migrationBuilder.DropTable(
                name: "audit_entries",
                schema: "audit");

            migrationBuilder.DropTable(
                name: "benefits",
                schema: "benefits");

            migrationBuilder.DropTable(
                name: "CaseApproval");

            migrationBuilder.DropTable(
                name: "CaseComment");

            migrationBuilder.DropTable(
                name: "certifications",
                schema: "competence");

            migrationBuilder.DropTable(
                name: "communication_templates",
                schema: "recruitment");

            migrationBuilder.DropTable(
                name: "course_enrollments",
                schema: "lms");

            migrationBuilder.DropTable(
                name: "courses",
                schema: "lms");

            migrationBuilder.DropTable(
                name: "custom_field_values",
                schema: "configuration");

            migrationBuilder.DropTable(
                name: "custom_fields",
                schema: "configuration");

            migrationBuilder.DropTable(
                name: "dashboards",
                schema: "analytics");

            migrationBuilder.DropTable(
                name: "data_subject_requests",
                schema: "gdpr");

            migrationBuilder.DropTable(
                name: "delegated_access",
                schema: "authorization");

            migrationBuilder.DropTable(
                name: "document_signatures",
                schema: "documents");

            migrationBuilder.DropTable(
                name: "document_templates",
                schema: "documents");

            migrationBuilder.DropTable(
                name: "document_versions",
                schema: "documents");

            migrationBuilder.DropTable(
                name: "documents",
                schema: "documents");

            migrationBuilder.DropTable(
                name: "emergency_contacts",
                schema: "core_hr");

            migrationBuilder.DropTable(
                name: "employee_benefits",
                schema: "benefits");

            migrationBuilder.DropTable(
                name: "employments",
                schema: "core_hr");

            migrationBuilder.DropTable(
                name: "events",
                schema: "las");

            migrationBuilder.DropTable(
                name: "ExpenseItem");

            migrationBuilder.DropTable(
                name: "field_permissions",
                schema: "authorization");

            migrationBuilder.DropTable(
                name: "headcount_plans",
                schema: "positions");

            migrationBuilder.DropTable(
                name: "interview_schedules",
                schema: "recruitment");

            migrationBuilder.DropTable(
                name: "learning_path_steps",
                schema: "lms");

            migrationBuilder.DropTable(
                name: "leave_requests",
                schema: "leave");

            migrationBuilder.DropTable(
                name: "mandatory_trainings",
                schema: "competence");

            migrationBuilder.DropTable(
                name: "notification_templates",
                schema: "notifications");

            migrationBuilder.DropTable(
                name: "notifications",
                schema: "notifications");

            migrationBuilder.DropTable(
                name: "offboarding_items",
                schema: "offboarding");

            migrationBuilder.DropTable(
                name: "onboarding_items",
                schema: "recruitment");

            migrationBuilder.DropTable(
                name: "organization_units",
                schema: "core_hr");

            migrationBuilder.DropTable(
                name: "OutboxMessages");

            migrationBuilder.DropTable(
                name: "payroll_result_lines",
                schema: "payroll");

            migrationBuilder.DropTable(
                name: "performance_reviews",
                schema: "performance");

            migrationBuilder.DropTable(
                name: "periods",
                schema: "las");

            migrationBuilder.DropTable(
                name: "position_historik",
                schema: "positions");

            migrationBuilder.DropTable(
                name: "RehabNote");

            migrationBuilder.DropTable(
                name: "RehabUppfoljning");

            migrationBuilder.DropTable(
                name: "report_definitions",
                schema: "reporting");

            migrationBuilder.DropTable(
                name: "report_executions",
                schema: "reporting");

            migrationBuilder.DropTable(
                name: "requisition_approvals",
                schema: "recruitment");

            migrationBuilder.DropTable(
                name: "retention_records",
                schema: "gdpr");

            migrationBuilder.DropTable(
                name: "salary_codes",
                schema: "payroll");

            migrationBuilder.DropTable(
                name: "SalaryProposal");

            migrationBuilder.DropTable(
                name: "saved_reports",
                schema: "analytics");

            migrationBuilder.DropTable(
                name: "scheduled_shifts",
                schema: "scheduling");

            migrationBuilder.DropTable(
                name: "scorecards",
                schema: "recruitment");

            migrationBuilder.DropTable(
                name: "shift_swap_requests",
                schema: "scheduling");

            migrationBuilder.DropTable(
                name: "sick_leave_notifications",
                schema: "leave");

            migrationBuilder.DropTable(
                name: "staffing_template_lines",
                schema: "scheduling");

            migrationBuilder.DropTable(
                name: "talent_pool",
                schema: "recruitment");

            migrationBuilder.DropTable(
                name: "tax_table_rows",
                schema: "payroll");

            migrationBuilder.DropTable(
                name: "tenant_configurations",
                schema: "configuration");

            migrationBuilder.DropTable(
                name: "time_clock_events",
                schema: "scheduling");

            migrationBuilder.DropTable(
                name: "timesheets",
                schema: "scheduling");

            migrationBuilder.DropTable(
                name: "vacation_balances",
                schema: "leave");

            migrationBuilder.DropTable(
                name: "workflow_definitions",
                schema: "configuration");

            migrationBuilder.DropTable(
                name: "Vacancies");

            migrationBuilder.DropTable(
                name: "cases",
                schema: "case_mgmt");

            migrationBuilder.DropTable(
                name: "employees",
                schema: "core_hr");

            migrationBuilder.DropTable(
                name: "TravelClaims");

            migrationBuilder.DropTable(
                name: "learning_paths",
                schema: "lms");

            migrationBuilder.DropTable(
                name: "offboarding_cases",
                schema: "offboarding");

            migrationBuilder.DropTable(
                name: "onboarding_checklists",
                schema: "recruitment");

            migrationBuilder.DropTable(
                name: "payroll_results",
                schema: "payroll");

            migrationBuilder.DropTable(
                name: "accumulations",
                schema: "las");

            migrationBuilder.DropTable(
                name: "positions",
                schema: "positions");

            migrationBuilder.DropTable(
                name: "RehabCases");

            migrationBuilder.DropTable(
                name: "SalaryReviewRounds");

            migrationBuilder.DropTable(
                name: "schedules",
                schema: "scheduling");

            migrationBuilder.DropTable(
                name: "staffing_templates",
                schema: "scheduling");

            migrationBuilder.DropTable(
                name: "tax_tables",
                schema: "payroll");

            migrationBuilder.DropTable(
                name: "payroll_runs",
                schema: "payroll");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RegionHR.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class OpenHR2Enterprise : Microsoft.EntityFrameworkCore.Migrations.Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "agreements");

            migrationBuilder.EnsureSchema(
                name: "platform");

            migrationBuilder.EnsureSchema(
                name: "compensation");

            migrationBuilder.EnsureSchema(
                name: "automation");

            migrationBuilder.EnsureSchema(
                name: "vms");

            migrationBuilder.EnsureSchema(
                name: "migration");

            migrationBuilder.AddColumn<Guid>(
                name: "SkillCategoryEntityId",
                schema: "competence",
                table: "skills",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Filter",
                schema: "reporting",
                table: "report_definitions",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Gruppering",
                schema: "reporting",
                table: "report_definitions",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Kolumner",
                schema: "reporting",
                table: "report_definitions",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VisualiseringsTyp",
                schema: "reporting",
                table: "report_definitions",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "default_avtals_id",
                schema: "core_hr",
                table: "organization_units",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "avtals_id",
                schema: "core_hr",
                table: "employments",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "api_keys",
                schema: "platform",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    namn = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    nyckel_hash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    nyckel_prefix = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    scope = table.Column<string>(type: "jsonb", nullable: false),
                    utgar_datum = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    skapad_av = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    skapad_vid = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    senast_anvand = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ar_aktiv = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_api_keys", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "benefit_enrollments",
                schema: "benefits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AnstallId = table.Column<Guid>(type: "uuid", nullable: false),
                    BenefitId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    StartDatum = table.Column<DateOnly>(type: "date", nullable: false),
                    ValdNiva = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    SkapadVid = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_benefit_enrollments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "benefit_statements",
                schema: "benefits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AnstallId = table.Column<Guid>(type: "uuid", nullable: false),
                    Ar = table.Column<int>(type: "integer", nullable: false),
                    AktivaFormaner = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: true),
                    TotaltVarde = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    GenereradVid = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_benefit_statements", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "benefit_transactions",
                schema: "benefits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AnstallId = table.Column<Guid>(type: "uuid", nullable: false),
                    BenefitId = table.Column<Guid>(type: "uuid", nullable: false),
                    Typ = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Belopp = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Datum = table.Column<DateOnly>(type: "date", nullable: false),
                    Beskrivning = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    SkapadVid = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_benefit_transactions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "bonus_plans",
                schema: "compensation",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    namn = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    typ = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    beraknings_modell = table.Column<string>(type: "jsonb", nullable: true),
                    utbetalnings_tidpunkt = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    version = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bonus_plans", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "career_paths",
                schema: "competence",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Namn = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Bransch = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Beskrivning = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_career_paths", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "categories",
                schema: "automation",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    namn = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    beskrivning = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ikon = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    updated_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_categories", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "collective_agreements",
                schema: "agreements",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    namn = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    parter = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    giltig_fran = table.Column<DateOnly>(type: "date", nullable: false),
                    giltig_till = table.Column<DateOnly>(type: "date", nullable: true),
                    bransch = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    updated_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_collective_agreements", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "compensation_bands",
                schema: "compensation",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    befattningskategori = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    min_lon = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    mal_lon = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    max_lon = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    steg1_min = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    steg1_max = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    steg2_min = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    steg2_max = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    steg3_min = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    steg3_max = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    steg4_min = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    steg4_max = table.Column<decimal>(type: "numeric(18,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_compensation_bands", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "compensation_plans",
                schema: "compensation",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    namn = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    giltig_fran = table.Column<DateOnly>(type: "date", nullable: false),
                    giltig_till = table.Column<DateOnly>(type: "date", nullable: false),
                    total_budget = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    version = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_compensation_plans", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "compensation_simulations",
                schema: "compensation",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    namn = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    parametrar = table.Column<string>(type: "jsonb", nullable: true),
                    beraknat_resultat = table.Column<string>(type: "jsonb", nullable: true),
                    skapad_vid = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    skapad_av = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_compensation_simulations", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "contingent_time_reports",
                schema: "vms",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    contingent_worker_id = table.Column<Guid>(type: "uuid", nullable: false),
                    period = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    timmar = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    ob_timmar = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    overtid = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    attesterad_av = table.Column<Guid>(type: "uuid", nullable: true),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_contingent_time_reports", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "contingent_workers",
                schema: "vms",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    namn = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    vendor_id = table.Column<Guid>(type: "uuid", nullable: false),
                    staffing_request_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tilltradesdatum = table.Column<DateOnly>(type: "date", nullable: false),
                    slutdatum = table.Column<DateOnly>(type: "date", nullable: true),
                    tim_kostnad = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    enhet_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_contingent_workers", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "custom_object_records",
                schema: "platform",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomObjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    Data = table.Column<string>(type: "jsonb", nullable: false),
                    SkapadAv = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    SkapadVid = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UppdateradVid = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_custom_object_records", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "custom_object_relations",
                schema: "platform",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomObjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    KallEntityTyp = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    RelationsTyp = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_custom_object_relations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "custom_objects",
                schema: "platform",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Namn = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    PluralNamn = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Beskrivning = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    FaltSchema = table.Column<string>(type: "jsonb", nullable: false),
                    Relationer = table.Column<string>(type: "jsonb", nullable: false),
                    Ikon = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SkapadVid = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_custom_objects", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "demand_events",
                schema: "scheduling",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    namn = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    typ = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    paverkan_grad = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    datum_fran = table.Column<DateOnly>(type: "date", nullable: false),
                    datum_till = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_demand_events", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "demand_forecasts",
                schema: "scheduling",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    enhet_id = table.Column<Guid>(type: "uuid", nullable: false),
                    datum = table.Column<DateOnly>(type: "date", nullable: false),
                    beraknat_antal = table.Column<int>(type: "integer", nullable: false),
                    beraknade_tidmmar = table.Column<decimal>(type: "numeric(8,2)", precision: 8, scale: 2, nullable: false),
                    konfidensgrad = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    beraknad_vid = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_demand_forecasts", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "demand_patterns",
                schema: "scheduling",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    enhet_id = table.Column<Guid>(type: "uuid", nullable: false),
                    veckodag = table.Column<int>(type: "integer", nullable: false),
                    tim_pa_aret = table.Column<int>(type: "integer", nullable: true),
                    genomsnittlig_belastning = table.Column<decimal>(type: "numeric(8,2)", precision: 8, scale: 2, nullable: false),
                    sasongs_variation = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_demand_patterns", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "development_plans",
                schema: "competence",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AnstallId = table.Column<Guid>(type: "uuid", nullable: false),
                    MalRoll = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    StartDatum = table.Column<DateOnly>(type: "date", nullable: false),
                    MalDatum = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_development_plans", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "domain_events",
                schema: "platform",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    typ = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    aggregat_typ = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    aggregat_id = table.Column<Guid>(type: "uuid", nullable: false),
                    data = table.Column<string>(type: "jsonb", nullable: false),
                    korrelations_id = table.Column<Guid>(type: "uuid", nullable: false),
                    skapad_vid = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_domain_events", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "eligibility_rules",
                schema: "benefits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BenefitId = table.Column<Guid>(type: "uuid", nullable: false),
                    Namn = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Kombination = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    SkapadVid = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_eligibility_rules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "employee_availability",
                schema: "scheduling",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    anstalld_id = table.Column<Guid>(type: "uuid", nullable: false),
                    veckodag = table.Column<int>(type: "integer", nullable: true),
                    datum = table.Column<DateOnly>(type: "date", nullable: true),
                    tid_fran = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    tid_till = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    preferens = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ar_repeterande = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_employee_availability", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "enrollment_periods",
                schema: "benefits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Namn = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    StartDatum = table.Column<DateOnly>(type: "date", nullable: false),
                    SlutDatum = table.Column<DateOnly>(type: "date", nullable: false),
                    InkluderadePlaner = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    SkapadVid = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_enrollment_periods", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "event_deliveries",
                schema: "platform",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    event_subscription_id = table.Column<Guid>(type: "uuid", nullable: false),
                    domain_event_record_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    http_status_kod = table.Column<int>(type: "integer", nullable: true),
                    antal_forsok = table.Column<int>(type: "integer", nullable: false),
                    nasta_retry = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    skapad_vid = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    leverad_vid = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_event_deliveries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "event_subscriptions",
                schema: "platform",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    namn = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    hemlig_nyckel = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    event_filter = table.Column<string>(type: "jsonb", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    retry_config = table.Column<string>(type: "jsonb", nullable: false),
                    skapad_vid = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    konsekutiva_misslyckanden = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_event_subscriptions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "executions",
                schema: "automation",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    regel_id = table.Column<Guid>(type: "uuid", nullable: false),
                    handelse_typ = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    resultat = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    anvand_niva = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    utford_atgard = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    tidsstampel = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    audit_entry_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_executions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "extension_installations",
                schema: "platform",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    extension_id = table.Column<Guid>(type: "uuid", nullable: false),
                    version = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    installations_datum = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    konfiguration = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_extension_installations", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "extensions",
                schema: "platform",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    namn = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    version = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    forfattare = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    beskrivning = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    typ = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    licens = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    kompatibilitet = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    innehall = table.Column<string>(type: "jsonb", nullable: false),
                    skapad_vid = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_extensions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "fatigue_scores",
                schema: "scheduling",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    anstalld_id = table.Column<Guid>(type: "uuid", nullable: false),
                    poang = table.Column<int>(type: "integer", nullable: false),
                    konsekutiva_dagar = table.Column<int>(type: "integer", nullable: false),
                    nattpass_senaste_7_dagar = table.Column<int>(type: "integer", nullable: false),
                    total_timmar_senaste_7_dagar = table.Column<decimal>(type: "numeric(8,2)", precision: 8, scale: 2, nullable: false),
                    kort_vila = table.Column<int>(type: "integer", nullable: false),
                    helgarbete_senaste_4_veckor = table.Column<int>(type: "integer", nullable: false),
                    beraknad_vid = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fatigue_scores", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "framework_agreements",
                schema: "vms",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    vendor_id = table.Column<Guid>(type: "uuid", nullable: false),
                    giltig_fran = table.Column<DateOnly>(type: "date", nullable: false),
                    giltig_till = table.Column<DateOnly>(type: "date", nullable: false),
                    avtalsvillkor = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    uppsagningstid_manader = table.Column<int>(type: "integer", nullable: false),
                    forlangnings_klausul = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    avtalsvarde = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    updated_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_framework_agreements", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "inferred_skills",
                schema: "competence",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AnstallId = table.Column<Guid>(type: "uuid", nullable: false),
                    SkillId = table.Column<Guid>(type: "uuid", nullable: false),
                    Kalla = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Konfidens = table.Column<int>(type: "integer", nullable: false),
                    ArBekraftad = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_inferred_skills", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "internal_opportunities",
                schema: "competence",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Typ = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Titel = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    EnhetId = table.Column<Guid>(type: "uuid", nullable: false),
                    PeriodFran = table.Column<DateOnly>(type: "date", nullable: true),
                    PeriodTill = table.Column<DateOnly>(type: "date", nullable: true),
                    Kravprofil = table.Column<string>(type: "jsonb", nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_internal_opportunities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "jobs",
                schema: "migration",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    kalla = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    fil_namn = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    totalt_antal_rader = table.Column<int>(type: "integer", nullable: false),
                    importerade_rader = table.Column<int>(type: "integer", nullable: false),
                    fel_rader = table.Column<int>(type: "integer", nullable: false),
                    skapad_av = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    fel_meddelande = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    updated_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_jobs", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "kpi_alerts",
                schema: "analytics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    KPIDefinitionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Troskel = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    Mottagare = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ArAktiv = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_kpi_alerts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "kpi_definitions",
                schema: "analytics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Namn = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Kategori = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    BerakningsFormel = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Enhet = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Riktning = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    GronTroskel = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    GulTroskel = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    RodTroskel = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    ArAktiv = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_kpi_definitions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "kpi_snapshots",
                schema: "analytics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    KPIDefinitionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Period = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Varde = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    JamforelseVarde = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    Trend = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    BeraknadVid = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_kpi_snapshots", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "level_configs",
                schema: "automation",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    kategori_id = table.Column<Guid>(type: "uuid", nullable: false),
                    vald_niva = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    updated_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_level_configs", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "life_event_occurrences",
                schema: "benefits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LifeEventId = table.Column<Guid>(type: "uuid", nullable: false),
                    AnstallId = table.Column<Guid>(type: "uuid", nullable: false),
                    Datum = table.Column<DateOnly>(type: "date", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    KoppladeAtgarder = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    SkapadVid = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_life_event_occurrences", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "life_events",
                schema: "benefits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Namn = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Typ = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TillatnaAndringar = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    TidsFonsterDagar = table.Column<int>(type: "integer", nullable: false),
                    SkapadVid = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_life_events", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "mentor_relations",
                schema: "competence",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MentorId = table.Column<Guid>(type: "uuid", nullable: false),
                    AdeptId = table.Column<Guid>(type: "uuid", nullable: false),
                    FokusOmrade = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    StartDatum = table.Column<DateOnly>(type: "date", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    MotesFrekvensDagar = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mentor_relations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "prediction_models",
                schema: "analytics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Namn = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Typ = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    InputParametrar = table.Column<string>(type: "jsonb", nullable: false),
                    SenasteTranningsDatum = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Accuracy = table.Column<decimal>(type: "numeric(8,4)", precision: 8, scale: 4, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_prediction_models", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "prediction_results",
                schema: "analytics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PredictionModelId = table.Column<Guid>(type: "uuid", nullable: false),
                    EntityTyp = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    EntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    Score = table.Column<decimal>(type: "numeric(8,4)", precision: 8, scale: 4, nullable: false),
                    RiskNiva = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    BidragandeFaktorer = table.Column<string>(type: "jsonb", nullable: false),
                    BeraknadVid = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_prediction_results", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "rules",
                schema: "automation",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    namn = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    kategori_id = table.Column<Guid>(type: "uuid", nullable: false),
                    trigger_typ = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    villkor = table.Column<string>(type: "jsonb", nullable: false),
                    atgard = table.Column<string>(type: "jsonb", nullable: false),
                    ar_aktiv = table.Column<bool>(type: "boolean", nullable: false),
                    minimum_niva = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ar_system_regel = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    updated_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rules", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "scheduled_reports",
                schema: "reporting",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReportTemplateId = table.Column<Guid>(type: "uuid", nullable: false),
                    Frekvens = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Mottagare = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Format = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    SenastKord = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    NastaKorning = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_scheduled_reports", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "scheduling_constraints",
                schema: "scheduling",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    typ = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    beskrivning = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    vikt = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    ar_hard = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_scheduling_constraints", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "scheduling_runs",
                schema: "scheduling",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    enhet_id = table.Column<Guid>(type: "uuid", nullable: false),
                    period_fran = table.Column<DateOnly>(type: "date", nullable: false),
                    period_till = table.Column<DateOnly>(type: "date", nullable: false),
                    parametrar = table.Column<string>(type: "jsonb", nullable: false),
                    genererade_pass = table.Column<int>(type: "integer", nullable: false),
                    total_ob_kostnad = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    total_overtid_kostnad = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    atl_kompliant = table.Column<bool>(type: "boolean", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    skapad_vid = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_scheduling_runs", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "shift_coverage_requests",
                schema: "scheduling",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    scheduled_shift_id = table.Column<Guid>(type: "uuid", nullable: false),
                    anledning = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    tilldelad_anstalld_id = table.Column<Guid>(type: "uuid", nullable: true),
                    skapad_vid = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_shift_coverage_requests", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "skill_categories",
                schema: "competence",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Namn = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Beskrivning = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_skill_categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "skill_endorsements",
                schema: "competence",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SkillId = table.Column<Guid>(type: "uuid", nullable: false),
                    AnstallId = table.Column<Guid>(type: "uuid", nullable: false),
                    BekraftadAv = table.Column<Guid>(type: "uuid", nullable: false),
                    Datum = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_skill_endorsements", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "skill_relations",
                schema: "competence",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FranSkillId = table.Column<Guid>(type: "uuid", nullable: false),
                    TillSkillId = table.Column<Guid>(type: "uuid", nullable: false),
                    Typ = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_skill_relations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "spend_categories",
                schema: "vms",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    namn = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    beskrivning = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_spend_categories", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "staffing_requests",
                schema: "vms",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    enhet_id = table.Column<Guid>(type: "uuid", nullable: false),
                    befattning = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    period_fran = table.Column<DateOnly>(type: "date", nullable: false),
                    period_till = table.Column<DateOnly>(type: "date", nullable: false),
                    antal_personer = table.Column<int>(type: "integer", nullable: false),
                    kravprofil = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    updated_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_staffing_requests", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "suggestions",
                schema: "automation",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    regel_id = table.Column<Guid>(type: "uuid", nullable: false),
                    foreslagen_atgard = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    skapad_for = table.Column<Guid>(type: "uuid", nullable: true),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    skapad_vid = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    giltig_till = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_suggestions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "templates",
                schema: "migration",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    namn = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    kall_system = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    mappningar = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_templates", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "total_rewards_statements",
                schema: "compensation",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    anstall_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ar = table.Column<int>(type: "integer", nullable: false),
                    grund_lon = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    tillagg = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    pension = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    forsakringar = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    formaner = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    ag_avgifter = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    total_kompensation = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    genererad_vid = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_total_rewards_statements", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "variable_pay_components",
                schema: "compensation",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    namn = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    typ = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    beraknings_regel = table.Column<string>(type: "jsonb", nullable: true),
                    kopplad_till_tiddata = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_variable_pay_components", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "vendor_invoices",
                schema: "vms",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    vendor_id = table.Column<Guid>(type: "uuid", nullable: false),
                    period = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    belopp = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    matchad_mot_tidrapporter = table.Column<bool>(type: "boolean", nullable: false),
                    differens = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vendor_invoices", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "vendor_performances",
                schema: "vms",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    vendor_id = table.Column<Guid>(type: "uuid", nullable: false),
                    period = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    poang = table.Column<int>(type: "integer", nullable: false),
                    kommentar = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vendor_performances", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "vendors",
                schema: "vms",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    namn = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    org_nummer = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    kontaktperson = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    epost = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    telefon = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    kategori = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    updated_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vendors", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "workflow_nodes",
                schema: "platform",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkflowDefinitionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Ordning = table.Column<int>(type: "integer", nullable: false),
                    Typ = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Konfiguration = table.Column<string>(type: "jsonb", nullable: false),
                    Namn = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_workflow_nodes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "workflow_run_instances",
                schema: "platform",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkflowDefinitionId = table.Column<Guid>(type: "uuid", nullable: false),
                    AktuellNodeOrdning = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    EntityTyp = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    Data = table.Column<string>(type: "jsonb", nullable: false),
                    StartadVid = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AvslutadVid = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_workflow_run_instances", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "bonus_targets",
                schema: "compensation",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    bonus_plan_id = table.Column<Guid>(type: "uuid", nullable: false),
                    anstall_id = table.Column<Guid>(type: "uuid", nullable: true),
                    grupp_id = table.Column<Guid>(type: "uuid", nullable: true),
                    mal_kpi = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    vikt = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    troskel = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    tak = table.Column<decimal>(type: "numeric(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bonus_targets", x => x.id);
                    table.ForeignKey(
                        name: "FK_bonus_targets_bonus_plans_bonus_plan_id",
                        column: x => x.bonus_plan_id,
                        principalSchema: "compensation",
                        principalTable: "bonus_plans",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "career_path_steps",
                schema: "competence",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CareerPathId = table.Column<Guid>(type: "uuid", nullable: false),
                    Ordning = table.Column<int>(type: "integer", nullable: false),
                    Befattning = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    TypiskTidManader = table.Column<int>(type: "integer", nullable: false),
                    KravdaSkills = table.Column<string>(type: "jsonb", nullable: true),
                    KravdErfarenhetManader = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_career_path_steps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_career_path_steps_career_paths_CareerPathId",
                        column: x => x.CareerPathId,
                        principalSchema: "competence",
                        principalTable: "career_paths",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "agreement_insurance_packages",
                schema: "agreements",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    avtals_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tgl = table.Column<string>(type: "jsonb", nullable: false),
                    ags = table.Column<string>(type: "jsonb", nullable: false),
                    tfa = table.Column<string>(type: "jsonb", nullable: false),
                    afa = table.Column<string>(type: "jsonb", nullable: false),
                    psa = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_agreement_insurance_packages", x => x.id);
                    table.ForeignKey(
                        name: "FK_agreement_insurance_packages_collective_agreements_avtals_id",
                        column: x => x.avtals_id,
                        principalSchema: "agreements",
                        principalTable: "collective_agreements",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "agreement_notice_periods",
                schema: "agreements",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    avtals_id = table.Column<Guid>(type: "uuid", nullable: false),
                    anstallningstid_manader = table.Column<int>(type: "integer", nullable: false),
                    uppsagningstid_manader = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_agreement_notice_periods", x => x.id);
                    table.ForeignKey(
                        name: "FK_agreement_notice_periods_collective_agreements_avtals_id",
                        column: x => x.avtals_id,
                        principalSchema: "agreements",
                        principalTable: "collective_agreements",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "agreement_ob_rates",
                schema: "agreements",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    avtals_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tidstyp = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    belopp = table.Column<decimal>(type: "numeric", nullable: false),
                    giltig_fran = table.Column<DateOnly>(type: "date", nullable: false),
                    giltig_till = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_agreement_ob_rates", x => x.id);
                    table.ForeignKey(
                        name: "FK_agreement_ob_rates_collective_agreements_avtals_id",
                        column: x => x.avtals_id,
                        principalSchema: "agreements",
                        principalTable: "collective_agreements",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "agreement_overtime_rules",
                schema: "agreements",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    avtals_id = table.Column<Guid>(type: "uuid", nullable: false),
                    troskel = table.Column<decimal>(type: "numeric", nullable: false),
                    multiplikator = table.Column<decimal>(type: "numeric", nullable: false),
                    max_per_ar = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_agreement_overtime_rules", x => x.id);
                    table.ForeignKey(
                        name: "FK_agreement_overtime_rules_collective_agreements_avtals_id",
                        column: x => x.avtals_id,
                        principalSchema: "agreements",
                        principalTable: "collective_agreements",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "agreement_pension_rules",
                schema: "agreements",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    avtals_id = table.Column<Guid>(type: "uuid", nullable: false),
                    pensions_typ = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    sats_under_tak = table.Column<decimal>(type: "numeric", nullable: false),
                    sats_over_tak = table.Column<decimal>(type: "numeric", nullable: false),
                    tak = table.Column<decimal>(type: "numeric", nullable: false),
                    beraknings_modell = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_agreement_pension_rules", x => x.id);
                    table.ForeignKey(
                        name: "FK_agreement_pension_rules_collective_agreements_avtals_id",
                        column: x => x.avtals_id,
                        principalSchema: "agreements",
                        principalTable: "collective_agreements",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "agreement_rest_rules",
                schema: "agreements",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    avtals_id = table.Column<Guid>(type: "uuid", nullable: false),
                    min_dygnsvila = table.Column<decimal>(type: "numeric", nullable: false),
                    min_veckovila = table.Column<decimal>(type: "numeric", nullable: false),
                    rast_per_pass = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_agreement_rest_rules", x => x.id);
                    table.ForeignKey(
                        name: "FK_agreement_rest_rules_collective_agreements_avtals_id",
                        column: x => x.avtals_id,
                        principalSchema: "agreements",
                        principalTable: "collective_agreements",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "agreement_salary_structures",
                schema: "agreements",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    avtals_id = table.Column<Guid>(type: "uuid", nullable: false),
                    min_lon_per_kategori = table.Column<string>(type: "jsonb", nullable: false),
                    lone_steg = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_agreement_salary_structures", x => x.id);
                    table.ForeignKey(
                        name: "FK_agreement_salary_structures_collective_agreements_avtals_id",
                        column: x => x.avtals_id,
                        principalSchema: "agreements",
                        principalTable: "collective_agreements",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "agreement_vacation_rules",
                schema: "agreements",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    avtals_id = table.Column<Guid>(type: "uuid", nullable: false),
                    bas_dagar = table.Column<int>(type: "integer", nullable: false),
                    extra_dagar_vid_40 = table.Column<int>(type: "integer", nullable: false),
                    extra_dagar_vid_50 = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_agreement_vacation_rules", x => x.id);
                    table.ForeignKey(
                        name: "FK_agreement_vacation_rules_collective_agreements_avtals_id",
                        column: x => x.avtals_id,
                        principalSchema: "agreements",
                        principalTable: "collective_agreements",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "agreement_working_hours",
                schema: "agreements",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    avtals_id = table.Column<Guid>(type: "uuid", nullable: false),
                    normal_timmar_per_vecka = table.Column<decimal>(type: "numeric", nullable: false),
                    flex_regler = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_agreement_working_hours", x => x.id);
                    table.ForeignKey(
                        name: "FK_agreement_working_hours_collective_agreements_avtals_id",
                        column: x => x.avtals_id,
                        principalSchema: "agreements",
                        principalTable: "collective_agreements",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "private_compensation_plans",
                schema: "agreements",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    avtals_id = table.Column<Guid>(type: "uuid", nullable: false),
                    bonus = table.Column<string>(type: "jsonb", nullable: false),
                    provision = table.Column<string>(type: "jsonb", nullable: false),
                    aktier = table.Column<string>(type: "jsonb", nullable: false),
                    tjanstebil = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_private_compensation_plans", x => x.id);
                    table.ForeignKey(
                        name: "FK_private_compensation_plans_collective_agreements_avtals_id",
                        column: x => x.avtals_id,
                        principalSchema: "agreements",
                        principalTable: "collective_agreements",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "compensation_budgets",
                schema: "compensation",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    compensation_plan_id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_unit_id = table.Column<Guid>(type: "uuid", nullable: false),
                    total_utrymme = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    fordelat = table.Column<decimal>(type: "numeric(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_compensation_budgets", x => x.id);
                    table.ForeignKey(
                        name: "FK_compensation_budgets_compensation_plans_compensation_plan_id",
                        column: x => x.compensation_plan_id,
                        principalSchema: "compensation",
                        principalTable: "compensation_plans",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "compensation_guidelines",
                schema: "compensation",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    compensation_plan_id = table.Column<Guid>(type: "uuid", nullable: false),
                    prestations_niva = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    rekommenderad_hojning_procent = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    max_hojning_procent = table.Column<decimal>(type: "numeric(5,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_compensation_guidelines", x => x.id);
                    table.ForeignKey(
                        name: "FK_compensation_guidelines_compensation_plans_compensation_pla~",
                        column: x => x.compensation_plan_id,
                        principalSchema: "compensation",
                        principalTable: "compensation_plans",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "development_milestones",
                schema: "competence",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DevelopmentPlanId = table.Column<Guid>(type: "uuid", nullable: false),
                    Beskrivning = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Typ = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    MalDatum = table.Column<DateOnly>(type: "date", nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_development_milestones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_development_milestones_development_plans_DevelopmentPlanId",
                        column: x => x.DevelopmentPlanId,
                        principalSchema: "competence",
                        principalTable: "development_plans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "eligibility_conditions",
                schema: "benefits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EligibilityRuleId = table.Column<Guid>(type: "uuid", nullable: false),
                    Falt = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Operator = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Varde = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_eligibility_conditions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_eligibility_conditions_eligibility_rules_EligibilityRuleId",
                        column: x => x.EligibilityRuleId,
                        principalSchema: "benefits",
                        principalTable: "eligibility_rules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "rate_cards",
                schema: "vms",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    framework_agreement_id = table.Column<Guid>(type: "uuid", nullable: false),
                    yrkes_kategori = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    tim_pris = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    ob_paslag = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    overtid_paslag = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    moms = table.Column<decimal>(type: "numeric(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rate_cards", x => x.id);
                    table.ForeignKey(
                        name: "FK_rate_cards_framework_agreements_framework_agreement_id",
                        column: x => x.framework_agreement_id,
                        principalSchema: "vms",
                        principalTable: "framework_agreements",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "opportunity_applications",
                schema: "competence",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InternalOpportunityId = table.Column<Guid>(type: "uuid", nullable: false),
                    AnstallId = table.Column<Guid>(type: "uuid", nullable: false),
                    Motivering = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    MatchScore = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    SkapadVid = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_opportunity_applications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_opportunity_applications_internal_opportunities_InternalOpp~",
                        column: x => x.InternalOpportunityId,
                        principalSchema: "competence",
                        principalTable: "internal_opportunities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "logs",
                schema: "migration",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    migration_job_id = table.Column<Guid>(type: "uuid", nullable: false),
                    entity_typ = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    importerad_post_id = table.Column<Guid>(type: "uuid", nullable: true),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    fel_meddelande = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_logs", x => x.id);
                    table.ForeignKey(
                        name: "FK_logs_jobs_migration_job_id",
                        column: x => x.migration_job_id,
                        principalSchema: "migration",
                        principalTable: "jobs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "mappings",
                schema: "migration",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    migration_job_id = table.Column<Guid>(type: "uuid", nullable: false),
                    kall_falt = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    mal_falt = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    transformations_regel = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mappings", x => x.id);
                    table.ForeignKey(
                        name: "FK_mappings_jobs_migration_job_id",
                        column: x => x.migration_job_id,
                        principalSchema: "migration",
                        principalTable: "jobs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "validation_errors",
                schema: "migration",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    migration_job_id = table.Column<Guid>(type: "uuid", nullable: false),
                    rad_nummer = table.Column<int>(type: "integer", nullable: false),
                    falt = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    fel_typ = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    original_varde = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    foreslagn_korrektion = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_validation_errors", x => x.id);
                    table.ForeignKey(
                        name: "FK_validation_errors_jobs_migration_job_id",
                        column: x => x.migration_job_id,
                        principalSchema: "migration",
                        principalTable: "jobs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "bonus_outcomes",
                schema: "compensation",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    bonus_target_id = table.Column<Guid>(type: "uuid", nullable: false),
                    utfall_varde = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    beraknat_belopp = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bonus_outcomes", x => x.id);
                    table.ForeignKey(
                        name: "FK_bonus_outcomes_bonus_targets_bonus_target_id",
                        column: x => x.bonus_target_id,
                        principalSchema: "compensation",
                        principalTable: "bonus_targets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_agreement_insurance_packages_avtals_id",
                schema: "agreements",
                table: "agreement_insurance_packages",
                column: "avtals_id");

            migrationBuilder.CreateIndex(
                name: "IX_agreement_notice_periods_avtals_id",
                schema: "agreements",
                table: "agreement_notice_periods",
                column: "avtals_id");

            migrationBuilder.CreateIndex(
                name: "IX_agreement_ob_rates_avtals_id",
                schema: "agreements",
                table: "agreement_ob_rates",
                column: "avtals_id");

            migrationBuilder.CreateIndex(
                name: "IX_agreement_overtime_rules_avtals_id",
                schema: "agreements",
                table: "agreement_overtime_rules",
                column: "avtals_id");

            migrationBuilder.CreateIndex(
                name: "IX_agreement_pension_rules_avtals_id",
                schema: "agreements",
                table: "agreement_pension_rules",
                column: "avtals_id");

            migrationBuilder.CreateIndex(
                name: "IX_agreement_rest_rules_avtals_id",
                schema: "agreements",
                table: "agreement_rest_rules",
                column: "avtals_id");

            migrationBuilder.CreateIndex(
                name: "IX_agreement_salary_structures_avtals_id",
                schema: "agreements",
                table: "agreement_salary_structures",
                column: "avtals_id");

            migrationBuilder.CreateIndex(
                name: "IX_agreement_vacation_rules_avtals_id",
                schema: "agreements",
                table: "agreement_vacation_rules",
                column: "avtals_id");

            migrationBuilder.CreateIndex(
                name: "IX_agreement_working_hours_avtals_id",
                schema: "agreements",
                table: "agreement_working_hours",
                column: "avtals_id");

            migrationBuilder.CreateIndex(
                name: "IX_api_keys_ar_aktiv",
                schema: "platform",
                table: "api_keys",
                column: "ar_aktiv");

            migrationBuilder.CreateIndex(
                name: "IX_api_keys_nyckel_hash",
                schema: "platform",
                table: "api_keys",
                column: "nyckel_hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_benefit_enrollments_AnstallId",
                schema: "benefits",
                table: "benefit_enrollments",
                column: "AnstallId");

            migrationBuilder.CreateIndex(
                name: "IX_benefit_enrollments_BenefitId",
                schema: "benefits",
                table: "benefit_enrollments",
                column: "BenefitId");

            migrationBuilder.CreateIndex(
                name: "IX_benefit_enrollments_Status",
                schema: "benefits",
                table: "benefit_enrollments",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_benefit_statements_AnstallId",
                schema: "benefits",
                table: "benefit_statements",
                column: "AnstallId");

            migrationBuilder.CreateIndex(
                name: "IX_benefit_statements_AnstallId_Ar",
                schema: "benefits",
                table: "benefit_statements",
                columns: new[] { "AnstallId", "Ar" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_benefit_transactions_AnstallId",
                schema: "benefits",
                table: "benefit_transactions",
                column: "AnstallId");

            migrationBuilder.CreateIndex(
                name: "IX_benefit_transactions_BenefitId",
                schema: "benefits",
                table: "benefit_transactions",
                column: "BenefitId");

            migrationBuilder.CreateIndex(
                name: "IX_benefit_transactions_Datum",
                schema: "benefits",
                table: "benefit_transactions",
                column: "Datum");

            migrationBuilder.CreateIndex(
                name: "IX_bonus_outcomes_bonus_target_id",
                schema: "compensation",
                table: "bonus_outcomes",
                column: "bonus_target_id");

            migrationBuilder.CreateIndex(
                name: "IX_bonus_targets_bonus_plan_id",
                schema: "compensation",
                table: "bonus_targets",
                column: "bonus_plan_id");

            migrationBuilder.CreateIndex(
                name: "IX_career_path_steps_CareerPathId",
                schema: "competence",
                table: "career_path_steps",
                column: "CareerPathId");

            migrationBuilder.CreateIndex(
                name: "IX_compensation_budgets_compensation_plan_id",
                schema: "compensation",
                table: "compensation_budgets",
                column: "compensation_plan_id");

            migrationBuilder.CreateIndex(
                name: "IX_compensation_guidelines_compensation_plan_id",
                schema: "compensation",
                table: "compensation_guidelines",
                column: "compensation_plan_id");

            migrationBuilder.CreateIndex(
                name: "IX_custom_object_records_CustomObjectId",
                schema: "platform",
                table: "custom_object_records",
                column: "CustomObjectId");

            migrationBuilder.CreateIndex(
                name: "IX_custom_object_relations_CustomObjectId",
                schema: "platform",
                table: "custom_object_relations",
                column: "CustomObjectId");

            migrationBuilder.CreateIndex(
                name: "IX_custom_objects_Namn",
                schema: "platform",
                table: "custom_objects",
                column: "Namn",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_development_milestones_DevelopmentPlanId",
                schema: "competence",
                table: "development_milestones",
                column: "DevelopmentPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_development_plans_AnstallId",
                schema: "competence",
                table: "development_plans",
                column: "AnstallId");

            migrationBuilder.CreateIndex(
                name: "IX_domain_events_aggregat_id",
                schema: "platform",
                table: "domain_events",
                column: "aggregat_id");

            migrationBuilder.CreateIndex(
                name: "IX_domain_events_skapad_vid",
                schema: "platform",
                table: "domain_events",
                column: "skapad_vid");

            migrationBuilder.CreateIndex(
                name: "IX_domain_events_typ",
                schema: "platform",
                table: "domain_events",
                column: "typ");

            migrationBuilder.CreateIndex(
                name: "IX_eligibility_conditions_EligibilityRuleId",
                schema: "benefits",
                table: "eligibility_conditions",
                column: "EligibilityRuleId");

            migrationBuilder.CreateIndex(
                name: "IX_eligibility_rules_BenefitId",
                schema: "benefits",
                table: "eligibility_rules",
                column: "BenefitId");

            migrationBuilder.CreateIndex(
                name: "IX_enrollment_periods_Status",
                schema: "benefits",
                table: "enrollment_periods",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_event_deliveries_event_subscription_id",
                schema: "platform",
                table: "event_deliveries",
                column: "event_subscription_id");

            migrationBuilder.CreateIndex(
                name: "IX_event_deliveries_nasta_retry",
                schema: "platform",
                table: "event_deliveries",
                column: "nasta_retry");

            migrationBuilder.CreateIndex(
                name: "IX_event_deliveries_status",
                schema: "platform",
                table: "event_deliveries",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_event_subscriptions_status",
                schema: "platform",
                table: "event_subscriptions",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_executions_regel_id",
                schema: "automation",
                table: "executions",
                column: "regel_id");

            migrationBuilder.CreateIndex(
                name: "IX_executions_tidsstampel",
                schema: "automation",
                table: "executions",
                column: "tidsstampel");

            migrationBuilder.CreateIndex(
                name: "IX_extension_installations_extension_id",
                schema: "platform",
                table: "extension_installations",
                column: "extension_id");

            migrationBuilder.CreateIndex(
                name: "IX_extensions_namn",
                schema: "platform",
                table: "extensions",
                column: "namn");

            migrationBuilder.CreateIndex(
                name: "IX_inferred_skills_AnstallId_SkillId",
                schema: "competence",
                table: "inferred_skills",
                columns: new[] { "AnstallId", "SkillId" });

            migrationBuilder.CreateIndex(
                name: "IX_internal_opportunities_Status",
                schema: "competence",
                table: "internal_opportunities",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_jobs_created_at",
                schema: "migration",
                table: "jobs",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_jobs_status",
                schema: "migration",
                table: "jobs",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_kpi_alerts_KPIDefinitionId",
                schema: "analytics",
                table: "kpi_alerts",
                column: "KPIDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_kpi_definitions_ArAktiv",
                schema: "analytics",
                table: "kpi_definitions",
                column: "ArAktiv");

            migrationBuilder.CreateIndex(
                name: "IX_kpi_definitions_Kategori",
                schema: "analytics",
                table: "kpi_definitions",
                column: "Kategori");

            migrationBuilder.CreateIndex(
                name: "IX_kpi_snapshots_KPIDefinitionId",
                schema: "analytics",
                table: "kpi_snapshots",
                column: "KPIDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_kpi_snapshots_Period",
                schema: "analytics",
                table: "kpi_snapshots",
                column: "Period");

            migrationBuilder.CreateIndex(
                name: "IX_level_configs_kategori_id",
                schema: "automation",
                table: "level_configs",
                column: "kategori_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_life_event_occurrences_AnstallId",
                schema: "benefits",
                table: "life_event_occurrences",
                column: "AnstallId");

            migrationBuilder.CreateIndex(
                name: "IX_life_event_occurrences_LifeEventId",
                schema: "benefits",
                table: "life_event_occurrences",
                column: "LifeEventId");

            migrationBuilder.CreateIndex(
                name: "IX_life_event_occurrences_Status",
                schema: "benefits",
                table: "life_event_occurrences",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_life_events_Typ",
                schema: "benefits",
                table: "life_events",
                column: "Typ");

            migrationBuilder.CreateIndex(
                name: "IX_logs_migration_job_id",
                schema: "migration",
                table: "logs",
                column: "migration_job_id");

            migrationBuilder.CreateIndex(
                name: "IX_logs_status",
                schema: "migration",
                table: "logs",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_mappings_migration_job_id",
                schema: "migration",
                table: "mappings",
                column: "migration_job_id");

            migrationBuilder.CreateIndex(
                name: "IX_mentor_relations_MentorId_AdeptId",
                schema: "competence",
                table: "mentor_relations",
                columns: new[] { "MentorId", "AdeptId" });

            migrationBuilder.CreateIndex(
                name: "IX_opportunity_applications_InternalOpportunityId_AnstallId",
                schema: "competence",
                table: "opportunity_applications",
                columns: new[] { "InternalOpportunityId", "AnstallId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_prediction_models_Typ",
                schema: "analytics",
                table: "prediction_models",
                column: "Typ");

            migrationBuilder.CreateIndex(
                name: "IX_prediction_results_EntityTyp_EntityId",
                schema: "analytics",
                table: "prediction_results",
                columns: new[] { "EntityTyp", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_prediction_results_PredictionModelId",
                schema: "analytics",
                table: "prediction_results",
                column: "PredictionModelId");

            migrationBuilder.CreateIndex(
                name: "IX_private_compensation_plans_avtals_id",
                schema: "agreements",
                table: "private_compensation_plans",
                column: "avtals_id");

            migrationBuilder.CreateIndex(
                name: "IX_rate_cards_framework_agreement_id",
                schema: "vms",
                table: "rate_cards",
                column: "framework_agreement_id");

            migrationBuilder.CreateIndex(
                name: "IX_rules_kategori_id",
                schema: "automation",
                table: "rules",
                column: "kategori_id");

            migrationBuilder.CreateIndex(
                name: "IX_rules_trigger_typ",
                schema: "automation",
                table: "rules",
                column: "trigger_typ");

            migrationBuilder.CreateIndex(
                name: "IX_scheduled_reports_NastaKorning",
                schema: "reporting",
                table: "scheduled_reports",
                column: "NastaKorning");

            migrationBuilder.CreateIndex(
                name: "IX_scheduled_reports_ReportTemplateId",
                schema: "reporting",
                table: "scheduled_reports",
                column: "ReportTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_skill_categories_Namn",
                schema: "competence",
                table: "skill_categories",
                column: "Namn",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_skill_endorsements_SkillId_AnstallId_BekraftadAv",
                schema: "competence",
                table: "skill_endorsements",
                columns: new[] { "SkillId", "AnstallId", "BekraftadAv" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_skill_relations_FranSkillId_TillSkillId",
                schema: "competence",
                table: "skill_relations",
                columns: new[] { "FranSkillId", "TillSkillId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_suggestions_regel_id",
                schema: "automation",
                table: "suggestions",
                column: "regel_id");

            migrationBuilder.CreateIndex(
                name: "IX_suggestions_status",
                schema: "automation",
                table: "suggestions",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_total_rewards_statements_anstall_id_ar",
                schema: "compensation",
                table: "total_rewards_statements",
                columns: new[] { "anstall_id", "ar" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_validation_errors_migration_job_id",
                schema: "migration",
                table: "validation_errors",
                column: "migration_job_id");

            migrationBuilder.CreateIndex(
                name: "IX_workflow_nodes_WorkflowDefinitionId_Ordning",
                schema: "platform",
                table: "workflow_nodes",
                columns: new[] { "WorkflowDefinitionId", "Ordning" });

            migrationBuilder.CreateIndex(
                name: "IX_workflow_run_instances_EntityTyp_EntityId",
                schema: "platform",
                table: "workflow_run_instances",
                columns: new[] { "EntityTyp", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_workflow_run_instances_Status",
                schema: "platform",
                table: "workflow_run_instances",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_workflow_run_instances_WorkflowDefinitionId",
                schema: "platform",
                table: "workflow_run_instances",
                column: "WorkflowDefinitionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "agreement_insurance_packages",
                schema: "agreements");

            migrationBuilder.DropTable(
                name: "agreement_notice_periods",
                schema: "agreements");

            migrationBuilder.DropTable(
                name: "agreement_ob_rates",
                schema: "agreements");

            migrationBuilder.DropTable(
                name: "agreement_overtime_rules",
                schema: "agreements");

            migrationBuilder.DropTable(
                name: "agreement_pension_rules",
                schema: "agreements");

            migrationBuilder.DropTable(
                name: "agreement_rest_rules",
                schema: "agreements");

            migrationBuilder.DropTable(
                name: "agreement_salary_structures",
                schema: "agreements");

            migrationBuilder.DropTable(
                name: "agreement_vacation_rules",
                schema: "agreements");

            migrationBuilder.DropTable(
                name: "agreement_working_hours",
                schema: "agreements");

            migrationBuilder.DropTable(
                name: "api_keys",
                schema: "platform");

            migrationBuilder.DropTable(
                name: "benefit_enrollments",
                schema: "benefits");

            migrationBuilder.DropTable(
                name: "benefit_statements",
                schema: "benefits");

            migrationBuilder.DropTable(
                name: "benefit_transactions",
                schema: "benefits");

            migrationBuilder.DropTable(
                name: "bonus_outcomes",
                schema: "compensation");

            migrationBuilder.DropTable(
                name: "career_path_steps",
                schema: "competence");

            migrationBuilder.DropTable(
                name: "categories",
                schema: "automation");

            migrationBuilder.DropTable(
                name: "compensation_bands",
                schema: "compensation");

            migrationBuilder.DropTable(
                name: "compensation_budgets",
                schema: "compensation");

            migrationBuilder.DropTable(
                name: "compensation_guidelines",
                schema: "compensation");

            migrationBuilder.DropTable(
                name: "compensation_simulations",
                schema: "compensation");

            migrationBuilder.DropTable(
                name: "contingent_time_reports",
                schema: "vms");

            migrationBuilder.DropTable(
                name: "contingent_workers",
                schema: "vms");

            migrationBuilder.DropTable(
                name: "custom_object_records",
                schema: "platform");

            migrationBuilder.DropTable(
                name: "custom_object_relations",
                schema: "platform");

            migrationBuilder.DropTable(
                name: "custom_objects",
                schema: "platform");

            migrationBuilder.DropTable(
                name: "demand_events",
                schema: "scheduling");

            migrationBuilder.DropTable(
                name: "demand_forecasts",
                schema: "scheduling");

            migrationBuilder.DropTable(
                name: "demand_patterns",
                schema: "scheduling");

            migrationBuilder.DropTable(
                name: "development_milestones",
                schema: "competence");

            migrationBuilder.DropTable(
                name: "domain_events",
                schema: "platform");

            migrationBuilder.DropTable(
                name: "eligibility_conditions",
                schema: "benefits");

            migrationBuilder.DropTable(
                name: "employee_availability",
                schema: "scheduling");

            migrationBuilder.DropTable(
                name: "enrollment_periods",
                schema: "benefits");

            migrationBuilder.DropTable(
                name: "event_deliveries",
                schema: "platform");

            migrationBuilder.DropTable(
                name: "event_subscriptions",
                schema: "platform");

            migrationBuilder.DropTable(
                name: "executions",
                schema: "automation");

            migrationBuilder.DropTable(
                name: "extension_installations",
                schema: "platform");

            migrationBuilder.DropTable(
                name: "extensions",
                schema: "platform");

            migrationBuilder.DropTable(
                name: "fatigue_scores",
                schema: "scheduling");

            migrationBuilder.DropTable(
                name: "inferred_skills",
                schema: "competence");

            migrationBuilder.DropTable(
                name: "kpi_alerts",
                schema: "analytics");

            migrationBuilder.DropTable(
                name: "kpi_definitions",
                schema: "analytics");

            migrationBuilder.DropTable(
                name: "kpi_snapshots",
                schema: "analytics");

            migrationBuilder.DropTable(
                name: "level_configs",
                schema: "automation");

            migrationBuilder.DropTable(
                name: "life_event_occurrences",
                schema: "benefits");

            migrationBuilder.DropTable(
                name: "life_events",
                schema: "benefits");

            migrationBuilder.DropTable(
                name: "logs",
                schema: "migration");

            migrationBuilder.DropTable(
                name: "mappings",
                schema: "migration");

            migrationBuilder.DropTable(
                name: "mentor_relations",
                schema: "competence");

            migrationBuilder.DropTable(
                name: "opportunity_applications",
                schema: "competence");

            migrationBuilder.DropTable(
                name: "prediction_models",
                schema: "analytics");

            migrationBuilder.DropTable(
                name: "prediction_results",
                schema: "analytics");

            migrationBuilder.DropTable(
                name: "private_compensation_plans",
                schema: "agreements");

            migrationBuilder.DropTable(
                name: "rate_cards",
                schema: "vms");

            migrationBuilder.DropTable(
                name: "rules",
                schema: "automation");

            migrationBuilder.DropTable(
                name: "scheduled_reports",
                schema: "reporting");

            migrationBuilder.DropTable(
                name: "scheduling_constraints",
                schema: "scheduling");

            migrationBuilder.DropTable(
                name: "scheduling_runs",
                schema: "scheduling");

            migrationBuilder.DropTable(
                name: "shift_coverage_requests",
                schema: "scheduling");

            migrationBuilder.DropTable(
                name: "skill_categories",
                schema: "competence");

            migrationBuilder.DropTable(
                name: "skill_endorsements",
                schema: "competence");

            migrationBuilder.DropTable(
                name: "skill_relations",
                schema: "competence");

            migrationBuilder.DropTable(
                name: "spend_categories",
                schema: "vms");

            migrationBuilder.DropTable(
                name: "staffing_requests",
                schema: "vms");

            migrationBuilder.DropTable(
                name: "suggestions",
                schema: "automation");

            migrationBuilder.DropTable(
                name: "templates",
                schema: "migration");

            migrationBuilder.DropTable(
                name: "total_rewards_statements",
                schema: "compensation");

            migrationBuilder.DropTable(
                name: "validation_errors",
                schema: "migration");

            migrationBuilder.DropTable(
                name: "variable_pay_components",
                schema: "compensation");

            migrationBuilder.DropTable(
                name: "vendor_invoices",
                schema: "vms");

            migrationBuilder.DropTable(
                name: "vendor_performances",
                schema: "vms");

            migrationBuilder.DropTable(
                name: "vendors",
                schema: "vms");

            migrationBuilder.DropTable(
                name: "workflow_nodes",
                schema: "platform");

            migrationBuilder.DropTable(
                name: "workflow_run_instances",
                schema: "platform");

            migrationBuilder.DropTable(
                name: "bonus_targets",
                schema: "compensation");

            migrationBuilder.DropTable(
                name: "career_paths",
                schema: "competence");

            migrationBuilder.DropTable(
                name: "compensation_plans",
                schema: "compensation");

            migrationBuilder.DropTable(
                name: "development_plans",
                schema: "competence");

            migrationBuilder.DropTable(
                name: "eligibility_rules",
                schema: "benefits");

            migrationBuilder.DropTable(
                name: "internal_opportunities",
                schema: "competence");

            migrationBuilder.DropTable(
                name: "collective_agreements",
                schema: "agreements");

            migrationBuilder.DropTable(
                name: "framework_agreements",
                schema: "vms");

            migrationBuilder.DropTable(
                name: "jobs",
                schema: "migration");

            migrationBuilder.DropTable(
                name: "bonus_plans",
                schema: "compensation");

            migrationBuilder.DropColumn(
                name: "SkillCategoryEntityId",
                schema: "competence",
                table: "skills");

            migrationBuilder.DropColumn(
                name: "Filter",
                schema: "reporting",
                table: "report_definitions");

            migrationBuilder.DropColumn(
                name: "Gruppering",
                schema: "reporting",
                table: "report_definitions");

            migrationBuilder.DropColumn(
                name: "Kolumner",
                schema: "reporting",
                table: "report_definitions");

            migrationBuilder.DropColumn(
                name: "VisualiseringsTyp",
                schema: "reporting",
                table: "report_definitions");

            migrationBuilder.DropColumn(
                name: "default_avtals_id",
                schema: "core_hr",
                table: "organization_units");

            migrationBuilder.DropColumn(
                name: "avtals_id",
                schema: "core_hr",
                table: "employments");
        }
    }
}

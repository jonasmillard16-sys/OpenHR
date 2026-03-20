using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RegionHR.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddArbetsmiljoEntities : Microsoft.EntityFrameworkCore.Migrations.Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "arbetsmiljo");

            migrationBuilder.CreateTable(
                name: "incidents",
                schema: "arbetsmiljo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Datum = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RapporterareNamn = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    EnhetId = table.Column<Guid>(type: "uuid", nullable: false),
                    Plats = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Beskrivning = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Allvarlighetsgrad = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Typ = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    AtgardsForslag = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_incidents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "risk_assessments",
                schema: "arbetsmiljo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RiskNamn = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Beskrivning = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Sannolikhet = table.Column<int>(type: "integer", nullable: false),
                    Konsekvens = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Atgard = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Ansvarig = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Deadline = table.Column<DateOnly>(type: "date", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_risk_assessments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "safety_rounds",
                schema: "arbetsmiljo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Datum = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EnhetId = table.Column<Guid>(type: "uuid", nullable: false),
                    Deltagare = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    AntalBrister = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Anteckningar = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_safety_rounds", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_incidents_Datum",
                schema: "arbetsmiljo",
                table: "incidents",
                column: "Datum");

            migrationBuilder.CreateIndex(
                name: "IX_incidents_EnhetId",
                schema: "arbetsmiljo",
                table: "incidents",
                column: "EnhetId");

            migrationBuilder.CreateIndex(
                name: "IX_safety_rounds_Datum",
                schema: "arbetsmiljo",
                table: "safety_rounds",
                column: "Datum");

            migrationBuilder.CreateIndex(
                name: "IX_safety_rounds_EnhetId",
                schema: "arbetsmiljo",
                table: "safety_rounds",
                column: "EnhetId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "incidents",
                schema: "arbetsmiljo");

            migrationBuilder.DropTable(
                name: "risk_assessments",
                schema: "arbetsmiljo");

            migrationBuilder.DropTable(
                name: "safety_rounds",
                schema: "arbetsmiljo");
        }
    }
}

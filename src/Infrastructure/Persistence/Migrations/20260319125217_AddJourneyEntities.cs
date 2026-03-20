using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RegionHR.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddJourneyEntities : Microsoft.EntityFrameworkCore.Migrations.Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "journeys");

            migrationBuilder.CreateTable(
                name: "journey_instances",
                schema: "journeys",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TemplateId = table.Column<Guid>(type: "uuid", nullable: false),
                    MallNamn = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    AnstallId = table.Column<Guid>(type: "uuid", nullable: false),
                    AnstallNamn = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Startdatum = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_journey_instances", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "journey_templates",
                schema: "journeys",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Namn = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Beskrivning = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Kategori = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_journey_templates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "journey_step_instances",
                schema: "journeys",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Ordning = table.Column<int>(type: "integer", nullable: false),
                    Titel = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Beskrivning = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    AnsvarigRoll = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DagOffset = table.Column<int>(type: "integer", nullable: false),
                    Klar = table.Column<bool>(type: "boolean", nullable: false),
                    KlarVid = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    KlarAv = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    JourneyInstanceId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_journey_step_instances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_journey_step_instances_journey_instances_JourneyInstanceId",
                        column: x => x.JourneyInstanceId,
                        principalSchema: "journeys",
                        principalTable: "journey_instances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "journey_step_templates",
                schema: "journeys",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Ordning = table.Column<int>(type: "integer", nullable: false),
                    Titel = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Beskrivning = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    AnsvarigRoll = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DagOffset = table.Column<int>(type: "integer", nullable: false),
                    JourneyTemplateId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_journey_step_templates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_journey_step_templates_journey_templates_JourneyTemplateId",
                        column: x => x.JourneyTemplateId,
                        principalSchema: "journeys",
                        principalTable: "journey_templates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_journey_instances_AnstallId",
                schema: "journeys",
                table: "journey_instances",
                column: "AnstallId");

            migrationBuilder.CreateIndex(
                name: "IX_journey_instances_TemplateId",
                schema: "journeys",
                table: "journey_instances",
                column: "TemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_journey_step_instances_JourneyInstanceId",
                schema: "journeys",
                table: "journey_step_instances",
                column: "JourneyInstanceId");

            migrationBuilder.CreateIndex(
                name: "IX_journey_step_templates_JourneyTemplateId",
                schema: "journeys",
                table: "journey_step_templates",
                column: "JourneyTemplateId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "journey_step_instances",
                schema: "journeys");

            migrationBuilder.DropTable(
                name: "journey_step_templates",
                schema: "journeys");

            migrationBuilder.DropTable(
                name: "journey_instances",
                schema: "journeys");

            migrationBuilder.DropTable(
                name: "journey_templates",
                schema: "journeys");
        }
    }
}

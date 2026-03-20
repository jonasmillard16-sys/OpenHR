using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RegionHR.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPulseSurveyEntities : Microsoft.EntityFrameworkCore.Migrations.Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "pulse");

            migrationBuilder.CreateTable(
                name: "pulse_survey_responses",
                schema: "pulse",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    survey_id = table.Column<Guid>(type: "uuid", nullable: false),
                    svarad_vid = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pulse_survey_responses", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "pulse_surveys",
                schema: "pulse",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    titel = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    beskrivning = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    skapad_vid = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    skapad_av = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    oppnad_vid = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    stangd_vid = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pulse_surveys", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "pulse_survey_answers",
                schema: "pulse",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    fraga_id = table.Column<Guid>(type: "uuid", nullable: false),
                    varde = table.Column<int>(type: "integer", nullable: false),
                    kommentar = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ResponseId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pulse_survey_answers", x => x.id);
                    table.ForeignKey(
                        name: "FK_pulse_survey_answers_pulse_survey_responses_ResponseId",
                        column: x => x.ResponseId,
                        principalSchema: "pulse",
                        principalTable: "pulse_survey_responses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "pulse_survey_questions",
                schema: "pulse",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    text = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ordning = table.Column<int>(type: "integer", nullable: false),
                    SurveyId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pulse_survey_questions", x => x.id);
                    table.ForeignKey(
                        name: "FK_pulse_survey_questions_pulse_surveys_SurveyId",
                        column: x => x.SurveyId,
                        principalSchema: "pulse",
                        principalTable: "pulse_surveys",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_pulse_survey_answers_ResponseId",
                schema: "pulse",
                table: "pulse_survey_answers",
                column: "ResponseId");

            migrationBuilder.CreateIndex(
                name: "IX_pulse_survey_questions_SurveyId",
                schema: "pulse",
                table: "pulse_survey_questions",
                column: "SurveyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "pulse_survey_answers",
                schema: "pulse");

            migrationBuilder.DropTable(
                name: "pulse_survey_questions",
                schema: "pulse");

            migrationBuilder.DropTable(
                name: "pulse_survey_responses",
                schema: "pulse");

            migrationBuilder.DropTable(
                name: "pulse_surveys",
                schema: "pulse");
        }
    }
}

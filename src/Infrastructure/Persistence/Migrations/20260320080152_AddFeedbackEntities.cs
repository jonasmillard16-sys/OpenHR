using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RegionHR.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddFeedbackEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "feedback_responses",
                schema: "performance",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    round_id = table.Column<Guid>(type: "uuid", nullable: false),
                    bedomare_id = table.Column<Guid>(type: "uuid", nullable: false),
                    relation = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    betyg = table.Column<int>(type: "integer", nullable: false),
                    kommentar = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    svarad_vid = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_feedback_responses", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "feedback_rounds",
                schema: "performance",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    anstall_id = table.Column<Guid>(type: "uuid", nullable: false),
                    titel = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    skapad_vid = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    oppnad_vid = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    stangd_vid = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_feedback_rounds", x => x.id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "feedback_responses",
                schema: "performance");

            migrationBuilder.DropTable(
                name: "feedback_rounds",
                schema: "performance");
        }
    }
}

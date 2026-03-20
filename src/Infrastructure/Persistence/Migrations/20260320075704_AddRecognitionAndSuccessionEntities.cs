using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RegionHR.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRecognitionAndSuccessionEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "recognitions",
                schema: "communication",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    fran_anstall_id = table.Column<Guid>(type: "uuid", nullable: false),
                    till_anstall_id = table.Column<Guid>(type: "uuid", nullable: false),
                    kategori = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    meddelande = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    skapad_vid = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_recognitions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "succession_plans",
                schema: "positions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    position_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nuvarande_innehavare = table.Column<Guid>(type: "uuid", nullable: true),
                    beraknad_pension_ar = table.Column<int>(type: "integer", nullable: true),
                    eftertradar_kandidat = table.Column<Guid>(type: "uuid", nullable: true),
                    beredskap = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    beredskap_procent = table.Column<int>(type: "integer", nullable: false),
                    skapad_vid = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_succession_plans", x => x.id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "recognitions",
                schema: "communication");

            migrationBuilder.DropTable(
                name: "succession_plans",
                schema: "positions");
        }
    }
}

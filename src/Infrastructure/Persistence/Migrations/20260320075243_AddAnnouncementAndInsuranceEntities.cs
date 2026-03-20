using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RegionHR.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAnnouncementAndInsuranceEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "communication");

            migrationBuilder.EnsureSchema(
                name: "insurance");

            migrationBuilder.CreateTable(
                name: "announcements",
                schema: "communication",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    titel = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    innehall = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    prioritet = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    skapad_vid = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    skapad_av = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    publicerad_vid = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_announcements", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "insurance_coverages",
                schema: "insurance",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    typ = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    namn = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    beskrivning = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    forsakringsgivare = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ar_aktiv = table.Column<bool>(type: "boolean", nullable: false),
                    skapad_vid = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_insurance_coverages", x => x.id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "announcements",
                schema: "communication");

            migrationBuilder.DropTable(
                name: "insurance_coverages",
                schema: "insurance");
        }
    }
}

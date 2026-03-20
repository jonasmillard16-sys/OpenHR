using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RegionHR.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddReferenceCheckAndMBLEntities : Microsoft.EntityFrameworkCore.Migrations.Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "mbl_negotiations",
                schema: "case_mgmt",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    arende = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    typ = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    datum = table.Column<DateOnly>(type: "date", nullable: false),
                    fackombud = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    arbetsgivarombud = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    protokoll = table.Column<string>(type: "text", nullable: true),
                    skapad_vid = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mbl_negotiations", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "reference_checks",
                schema: "recruitment",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    vacancy_id = table.Column<Guid>(type: "uuid", nullable: false),
                    kandidat_namn = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    referens_namn = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    referens_relation = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    kommentar = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    skapad_vid = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reference_checks", x => x.id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "mbl_negotiations",
                schema: "case_mgmt");

            migrationBuilder.DropTable(
                name: "reference_checks",
                schema: "recruitment");
        }
    }
}

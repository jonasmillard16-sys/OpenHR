using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RegionHR.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddWellnessClaimEntities : Microsoft.EntityFrameworkCore.Migrations.Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "wellness");

            migrationBuilder.CreateTable(
                name: "wellness_claims",
                schema: "wellness",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    anstall_id = table.Column<Guid>(type: "uuid", nullable: false),
                    aktivitet = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    belopp = table.Column<decimal>(type: "numeric", nullable: false),
                    datum = table.Column<DateOnly>(type: "date", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    kvitto_fil_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    skapad_vid = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    godkand_av = table.Column<Guid>(type: "uuid", nullable: true),
                    godkand_vid = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    kommentar = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_wellness_claims", x => x.id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "wellness_claims",
                schema: "wellness");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RegionHR.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProvisioningEntities : Microsoft.EntityFrameworkCore.Migrations.Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "provisioning");

            migrationBuilder.CreateTable(
                name: "provisioning_events",
                schema: "provisioning",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AnstallId = table.Column<Guid>(type: "uuid", nullable: false),
                    AnstallNamn = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    TargetSystem = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Aktion = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Trigger = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Tidpunkt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Detaljer = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_provisioning_events", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "provisioning_rules",
                schema: "provisioning",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Trigger = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TargetSystem = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Aktion = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ArAktiv = table.Column<bool>(type: "boolean", nullable: false),
                    Beskrivning = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_provisioning_rules", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_provisioning_events_AnstallId",
                schema: "provisioning",
                table: "provisioning_events",
                column: "AnstallId");

            migrationBuilder.CreateIndex(
                name: "IX_provisioning_events_Tidpunkt",
                schema: "provisioning",
                table: "provisioning_events",
                column: "Tidpunkt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "provisioning_events",
                schema: "provisioning");

            migrationBuilder.DropTable(
                name: "provisioning_rules",
                schema: "provisioning");
        }
    }
}

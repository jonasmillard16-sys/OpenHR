using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RegionHR.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPolicyEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "policy");

            migrationBuilder.CreateTable(
                name: "policies",
                schema: "policy",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    titel = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    sammanfattning = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    innehall = table.Column<string>(type: "text", nullable: false),
                    kategori = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    version = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    kraver_bekraftelse = table.Column<bool>(type: "boolean", nullable: false),
                    skapad_vid = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    publicerad_vid = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    skapad_av = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_policies", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "policy_confirmations",
                schema: "policy",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    policy_id = table.Column<Guid>(type: "uuid", nullable: false),
                    anstall_id = table.Column<Guid>(type: "uuid", nullable: false),
                    policy_version = table.Column<int>(type: "integer", nullable: false),
                    bekraftad_vid = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_policy_confirmations", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_policy_confirmations_policy_id_anstall_id",
                schema: "policy",
                table: "policy_confirmations",
                columns: new[] { "policy_id", "anstall_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "policies",
                schema: "policy");

            migrationBuilder.DropTable(
                name: "policy_confirmations",
                schema: "policy");
        }
    }
}

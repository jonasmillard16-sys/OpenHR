using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RegionHR.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddNotificationPreference : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "notification_preferences",
                schema: "notifications",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    anstall_id = table.Column<Guid>(type: "uuid", nullable: false),
                    notis_typ = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    in_app = table.Column<bool>(type: "boolean", nullable: false),
                    epost = table.Column<bool>(type: "boolean", nullable: false),
                    uppdaterad_vid = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notification_preferences", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_notification_preferences_anstall_id_notis_typ",
                schema: "notifications",
                table: "notification_preferences",
                columns: new[] { "anstall_id", "notis_typ" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "notification_preferences",
                schema: "notifications");
        }
    }
}

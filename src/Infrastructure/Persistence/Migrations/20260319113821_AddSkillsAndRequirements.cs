using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RegionHR.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSkillsAndRequirements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "employee_skills",
                schema: "competence",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AnstallId = table.Column<Guid>(type: "uuid", nullable: false),
                    SkillId = table.Column<Guid>(type: "uuid", nullable: false),
                    Niva = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_employee_skills", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "position_skill_requirements",
                schema: "competence",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PositionId = table.Column<Guid>(type: "uuid", nullable: false),
                    SkillId = table.Column<Guid>(type: "uuid", nullable: false),
                    MinNiva = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_position_skill_requirements", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "skills",
                schema: "competence",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Namn = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Kategori = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Beskrivning = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_skills", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_employee_skills_AnstallId_SkillId",
                schema: "competence",
                table: "employee_skills",
                columns: new[] { "AnstallId", "SkillId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_position_skill_requirements_PositionId_SkillId",
                schema: "competence",
                table: "position_skill_requirements",
                columns: new[] { "PositionId", "SkillId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_skills_Namn",
                schema: "competence",
                table: "skills",
                column: "Namn",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "employee_skills",
                schema: "competence");

            migrationBuilder.DropTable(
                name: "position_skill_requirements",
                schema: "competence");

            migrationBuilder.DropTable(
                name: "skills",
                schema: "competence");
        }
    }
}

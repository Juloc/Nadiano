using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nadiano.Web.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSkillEvidence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SkillEvidence",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProfileId = table.Column<Guid>(type: "TEXT", nullable: false),
                    LessonId = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    SkillId = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    SelfReportedSuccess = table.Column<bool>(type: "INTEGER", nullable: false),
                    RecordedAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SkillEvidence", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SkillEvidence_LearnerProfiles_ProfileId",
                        column: x => x.ProfileId,
                        principalTable: "LearnerProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SkillEvidence_ProfileId_LessonId",
                table: "SkillEvidence",
                columns: new[] { "ProfileId", "LessonId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SkillEvidence");
        }
    }
}

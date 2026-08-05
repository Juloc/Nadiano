using System;

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nadiano.Web.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCourseProgress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CourseEnrollments",
                columns: table => new
                {
                    ProfileId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CourseId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    EnrolledAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseEnrollments", x => new { x.ProfileId, x.CourseId });
                    table.ForeignKey(
                        name: "FK_CourseEnrollments_LearnerProfiles_ProfileId",
                        column: x => x.ProfileId,
                        principalTable: "LearnerProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LessonProgress",
                columns: table => new
                {
                    ProfileId = table.Column<Guid>(type: "TEXT", nullable: false),
                    LessonId = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    CourseId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    CompletedAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LessonProgress", x => new { x.ProfileId, x.LessonId });
                    table.ForeignKey(
                        name: "FK_LessonProgress_LearnerProfiles_ProfileId",
                        column: x => x.ProfileId,
                        principalTable: "LearnerProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LessonProgress_ProfileId_CourseId",
                table: "LessonProgress",
                columns: new[] { "ProfileId", "CourseId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CourseEnrollments");

            migrationBuilder.DropTable(
                name: "LessonProgress");
        }
    }
}
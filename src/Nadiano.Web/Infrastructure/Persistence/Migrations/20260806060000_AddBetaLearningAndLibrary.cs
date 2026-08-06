using System;

using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Nadiano.Web.Infrastructure.Persistence.Migrations;

[DbContext(typeof(NadianoDbContext))]
[Migration("20260806060000_AddBetaLearningAndLibrary")]
public sealed class AddBetaLearningAndLibrary : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "LearningEvidence",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                ProfileId = table.Column<Guid>(type: "TEXT", nullable: false),
                ActivityId = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                ActivityKind = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                Seed = table.Column<int>(type: "INTEGER", nullable: true),
                ExpectedJson = table.Column<string>(type: "TEXT", nullable: false),
                ResponseJson = table.Column<string>(type: "TEXT", nullable: false),
                ResultJson = table.Column<string>(type: "TEXT", nullable: false),
                RecordedAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_LearningEvidence", x => x.Id);
                table.ForeignKey(
                    name: "FK_LearningEvidence_LearnerProfiles_ProfileId",
                    column: x => x.ProfileId,
                    principalTable: "LearnerProfiles",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "PrivateLibraryItems",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                ProfileId = table.Column<Guid>(type: "TEXT", nullable: false),
                DisplayTitle = table.Column<string>(type: "TEXT", maxLength: 300, nullable: false),
                SourceFileName = table.Column<string>(type: "TEXT", maxLength: 300, nullable: false),
                StoredDirectoryName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                OriginalSha256 = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                ContentLength = table.Column<long>(type: "INTEGER", nullable: false),
                ValidationState = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                WarningJson = table.Column<string>(type: "TEXT", nullable: false),
                MetadataJson = table.Column<string>(type: "TEXT", nullable: false),
                Version = table.Column<int>(type: "INTEGER", nullable: false),
                ImportedAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PrivateLibraryItems", x => x.Id);
                table.ForeignKey(
                    name: "FK_PrivateLibraryItems_LearnerProfiles_ProfileId",
                    column: x => x.ProfileId,
                    principalTable: "LearnerProfiles",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "ReviewQueue",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                ProfileId = table.Column<Guid>(type: "TEXT", nullable: false),
                SkillId = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                SourceId = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                DueAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                IntervalDays = table.Column<int>(type: "INTEGER", nullable: false),
                ReasonCode = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                UpdatedAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ReviewQueue", x => x.Id);
                table.ForeignKey(
                    name: "FK_ReviewQueue_LearnerProfiles_ProfileId",
                    column: x => x.ProfileId,
                    principalTable: "LearnerProfiles",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_LearningEvidence_ProfileId_ActivityId_RecordedAtUtc",
            table: "LearningEvidence",
            columns: new[] { "ProfileId", "ActivityId", "RecordedAtUtc" });
        migrationBuilder.CreateIndex(
            name: "IX_PrivateLibraryItems_ProfileId_ImportedAtUtc",
            table: "PrivateLibraryItems",
            columns: new[] { "ProfileId", "ImportedAtUtc" });
        migrationBuilder.CreateIndex(
            name: "IX_ReviewQueue_ProfileId_DueAtUtc",
            table: "ReviewQueue",
            columns: new[] { "ProfileId", "DueAtUtc" });
        migrationBuilder.CreateIndex(
            name: "IX_ReviewQueue_ProfileId_SkillId_SourceId",
            table: "ReviewQueue",
            columns: new[] { "ProfileId", "SkillId", "SourceId" },
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "LearningEvidence");
        migrationBuilder.DropTable(name: "PrivateLibraryItems");
        migrationBuilder.DropTable(name: "ReviewQueue");
    }
}

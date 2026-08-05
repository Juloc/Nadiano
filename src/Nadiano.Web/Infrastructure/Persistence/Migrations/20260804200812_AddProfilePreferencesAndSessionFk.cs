using System;

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nadiano.Web.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProfilePreferencesAndSessionFk : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProfilePreferences",
                columns: table => new
                {
                    ProfileId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Language = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    NoteNameSystem = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    SessionLengthMinutes = table.Column<int>(type: "INTEGER", nullable: false),
                    PreferredMidiDeviceId = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    PreferredMidiDeviceName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProfilePreferences", x => x.ProfileId);
                    table.ForeignKey(
                        name: "FK_ProfilePreferences_LearnerProfiles_ProfileId",
                        column: x => x.ProfileId,
                        principalTable: "LearnerProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_PracticeSessions_LearnerProfiles_ProfileId",
                table: "PracticeSessions",
                column: "ProfileId",
                principalTable: "LearnerProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PracticeSessions_LearnerProfiles_ProfileId",
                table: "PracticeSessions");

            migrationBuilder.DropTable(
                name: "ProfilePreferences");
        }
    }
}
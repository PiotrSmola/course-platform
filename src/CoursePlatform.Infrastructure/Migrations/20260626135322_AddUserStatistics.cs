using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoursePlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserStatistics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserStatistics",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TotalEnrollments = table.Column<int>(type: "integer", nullable: false),
                    CompletedCourses = table.Column<int>(type: "integer", nullable: false),
                    TotalLessonsCompleted = table.Column<int>(type: "integer", nullable: false),
                    TotalLessonsAvailable = table.Column<int>(type: "integer", nullable: false),
                    AverageProgressPercentage = table.Column<double>(type: "double precision", precision: 5, scale: 2, nullable: false),
                    TotalLearningTimeSeconds = table.Column<int>(type: "integer", nullable: false),
                    CertificatesEarned = table.Column<int>(type: "integer", nullable: false),
                    LastActivityAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserStatistics", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_UserStatistics_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserStatistics");
        }
    }
}

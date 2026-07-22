using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoursePlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLessonWatchPosition : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LastPositionSeconds",
                table: "LessonProgresses",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastWatchedAt",
                table: "LessonProgresses",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastPositionSeconds",
                table: "LessonProgresses");

            migrationBuilder.DropColumn(
                name: "LastWatchedAt",
                table: "LessonProgresses");
        }
    }
}

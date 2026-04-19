using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cadence.API.Migrations
{
    /// <inheritdoc />
    public partial class AddHabitStats : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HabitStats",
                columns: table => new
                {
                    HabitId = table.Column<long>(type: "bigint", nullable: false),
                    CurrentStreak = table.Column<int>(type: "integer", nullable: false),
                    LongestStreak = table.Column<int>(type: "integer", nullable: false),
                    TotalCompletions = table.Column<int>(type: "integer", nullable: false),
                    LastCompletedDate = table.Column<DateOnly>(type: "date", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HabitStats", x => x.HabitId);
                    table.ForeignKey(
                        name: "FK_HabitStats_Habits_HabitId",
                        column: x => x.HabitId,
                        principalTable: "Habits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HabitStats");
        }
    }
}

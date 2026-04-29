using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cadence.API.Migrations
{
    /// <inheritdoc />
    public partial class AddStartAndEndTimeOnHabitEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<TimeOnly>(
                name: "EndTime",
                table: "Habits",
                type: "time without time zone",
                nullable: true);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "StartTime",
                table: "Habits",
                type: "time without time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EndTime",
                table: "Habits");

            migrationBuilder.DropColumn(
                name: "StartTime",
                table: "Habits");
        }
    }
}

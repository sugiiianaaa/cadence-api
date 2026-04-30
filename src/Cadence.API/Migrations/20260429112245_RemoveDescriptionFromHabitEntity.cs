using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cadence.API.Migrations
{
    /// <inheritdoc />
    public partial class RemoveDescriptionFromHabitEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "Habits");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Habits",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);
        }
    }
}

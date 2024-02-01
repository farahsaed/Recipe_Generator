using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Recipe_Generator.Migrations
{
    /// <inheritdoc />
    public partial class RecipeTotalrating : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TotalRating",
                table: "Recipes",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalRating",
                table: "Recipes");
        }
    }
}

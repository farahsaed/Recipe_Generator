using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Recipe_Generator.Migrations
{
    /// <inheritdoc />
    public partial class repliesandrecipes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RecipeId",
                table: "Replies",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Replies_RecipeId",
                table: "Replies",
                column: "RecipeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Replies_Recipes_RecipeId",
                table: "Replies",
                column: "RecipeId",
                principalTable: "Recipes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Replies_Recipes_RecipeId",
                table: "Replies");

            migrationBuilder.DropIndex(
                name: "IX_Replies_RecipeId",
                table: "Replies");

            migrationBuilder.DropColumn(
                name: "RecipeId",
                table: "Replies");
        }
    }
}

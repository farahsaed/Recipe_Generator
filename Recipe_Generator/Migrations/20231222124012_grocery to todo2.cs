using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Recipe_Generator.Migrations
{
    /// <inheritdoc />
    public partial class grocerytotodo2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Groceries",
                table: "Groceries");

            migrationBuilder.RenameTable(
                name: "Groceries",
                newName: "ToDos");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ToDos",
                table: "ToDos",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ToDos",
                table: "ToDos");

            migrationBuilder.RenameTable(
                name: "ToDos",
                newName: "Groceries");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Groceries",
                table: "Groceries",
                column: "Id");
        }
    }
}

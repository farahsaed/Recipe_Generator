﻿using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Recipe_Generator.Migrations
{
    /// <inheritdoc />
    public partial class updatesintodomodel2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedTime",
                table: "ToDos",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UpdatedTime",
                table: "ToDos");
        }
    }
}

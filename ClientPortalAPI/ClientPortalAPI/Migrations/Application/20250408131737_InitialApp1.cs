using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClientPortalAPI.Migrations.Application
{
    /// <inheritdoc />
    public partial class InitialApp1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AssignedAt",
                table: "FormTemplates");

            migrationBuilder.AddColumn<DateTime>(
                name: "AssignedAt",
                table: "FormAssignments",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AssignedAt",
                table: "FormAssignments");

            migrationBuilder.AddColumn<DateTime>(
                name: "AssignedAt",
                table: "FormTemplates",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}

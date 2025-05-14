using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClientPortalAPI.Migrations.Application
{
    /// <inheritdoc />
    public partial class AddFormAssignmentStatusAndNotes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "FormAssignments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "FormAssignments",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Notes",
                table: "FormAssignments");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "FormAssignments");
        }
    }
}

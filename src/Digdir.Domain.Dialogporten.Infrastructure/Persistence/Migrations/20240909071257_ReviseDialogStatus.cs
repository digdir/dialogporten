using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Digdir.Domain.Dialogporten.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ReviseDialogStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "DialogStatus",
                keyColumn: "Id",
                keyValue: 3,
                column: "Name",
                value: "Draft");

            migrationBuilder.UpdateData(
                table: "DialogStatus",
                keyColumn: "Id",
                keyValue: 4,
                column: "Name",
                value: "Sent");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "DialogStatus",
                keyColumn: "Id",
                keyValue: 3,
                column: "Name",
                value: "Signing");

            migrationBuilder.UpdateData(
                table: "DialogStatus",
                keyColumn: "Id",
                keyValue: 4,
                column: "Name",
                value: "Processing");
        }
    }
}

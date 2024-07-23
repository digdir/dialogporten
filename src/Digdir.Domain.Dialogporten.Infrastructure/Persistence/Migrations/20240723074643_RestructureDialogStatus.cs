using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Digdir.Domain.Dialogporten.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RestructureDialogStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.UpdateData(
                table: "DialogStatus",
                keyColumn: "Id",
                keyValue: 5,
                column: "Name",
                value: "RequiresAttention");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "DialogStatus",
                keyColumn: "Id",
                keyValue: 3,
                column: "Name",
                value: "Waiting");

            migrationBuilder.UpdateData(
                table: "DialogStatus",
                keyColumn: "Id",
                keyValue: 4,
                column: "Name",
                value: "Signing");

            migrationBuilder.UpdateData(
                table: "DialogStatus",
                keyColumn: "Id",
                keyValue: 5,
                column: "Name",
                value: "Cancelled");
        }
    }
}

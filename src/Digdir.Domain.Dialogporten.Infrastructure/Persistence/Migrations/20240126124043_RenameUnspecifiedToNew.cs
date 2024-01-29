using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Digdir.Domain.Dialogporten.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RenameUnspecifiedToNew : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "DialogStatus",
                keyColumn: "Id",
                keyValue: 1,
                column: "Name",
                value: "New");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "DialogStatus",
                keyColumn: "Id",
                keyValue: 1,
                column: "Name",
                value: "Unspecified");
        }
    }
}

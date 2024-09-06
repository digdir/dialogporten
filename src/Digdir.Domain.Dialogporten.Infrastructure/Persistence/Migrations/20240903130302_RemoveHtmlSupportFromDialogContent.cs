using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Digdir.Domain.Dialogporten.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveHtmlSupportFromDialogContent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "DialogContentType",
                keyColumn: "Id",
                keyValue: 4,
                column: "AllowedMediaTypes",
                value: new[] { "text/plain", "text/markdown" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "DialogContentType",
                keyColumn: "Id",
                keyValue: 4,
                column: "AllowedMediaTypes",
                value: new[] { "text/html", "text/plain", "text/markdown" });
        }
    }
}

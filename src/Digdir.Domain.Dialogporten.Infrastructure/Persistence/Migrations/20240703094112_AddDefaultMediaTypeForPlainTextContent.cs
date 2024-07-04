using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Digdir.Domain.Dialogporten.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDefaultMediaTypeForPlainTextContent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "DialogContentType",
                keyColumn: "Id",
                keyValue: 1,
                column: "AllowedMediaTypes",
                value: new[] { "text/plain" });

            migrationBuilder.UpdateData(
                table: "DialogContentType",
                keyColumn: "Id",
                keyValue: 2,
                column: "AllowedMediaTypes",
                value: new[] { "text/plain" });

            migrationBuilder.UpdateData(
                table: "DialogContentType",
                keyColumn: "Id",
                keyValue: 3,
                column: "AllowedMediaTypes",
                value: new[] { "text/plain" });

            migrationBuilder.UpdateData(
                table: "DialogContentType",
                keyColumn: "Id",
                keyValue: 5,
                column: "AllowedMediaTypes",
                value: new[] { "text/plain" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "DialogContentType",
                keyColumn: "Id",
                keyValue: 1,
                column: "AllowedMediaTypes",
                value: new string[0]);

            migrationBuilder.UpdateData(
                table: "DialogContentType",
                keyColumn: "Id",
                keyValue: 2,
                column: "AllowedMediaTypes",
                value: new string[0]);

            migrationBuilder.UpdateData(
                table: "DialogContentType",
                keyColumn: "Id",
                keyValue: 3,
                column: "AllowedMediaTypes",
                value: new string[0]);

            migrationBuilder.UpdateData(
                table: "DialogContentType",
                keyColumn: "Id",
                keyValue: 5,
                column: "AllowedMediaTypes",
                value: new string[0]);
        }
    }
}

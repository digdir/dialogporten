using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Digdir.Domain.Dialogporten.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class MoveFrontChannelEmbedsToContent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MediaType",
                table: "DialogElementUrl");

            migrationBuilder.DropColumn(
                name: "RenderAsHtml",
                table: "DialogContentType");

            migrationBuilder.AddColumn<string[]>(
                name: "AllowedMediaTypes",
                table: "DialogContentType",
                type: "text[]",
                nullable: false,
                defaultValue: new string[0]);

            migrationBuilder.AddColumn<string>(
                name: "MediaType",
                table: "DialogContent",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

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
                keyValue: 4,
                column: "AllowedMediaTypes",
                value: new[] { "text/html", "text/plain", "text/markdown" });

            migrationBuilder.UpdateData(
                table: "DialogContentType",
                keyColumn: "Id",
                keyValue: 5,
                column: "AllowedMediaTypes",
                value: new string[0]);

            migrationBuilder.InsertData(
                table: "DialogContentType",
                columns: new[] { "Id", "AllowedMediaTypes", "MaxLength", "Name", "OutputInList", "Required" },
                values: new object[] { 6, new[] { "application/vnd.dialogporten.frontchannelembed+json;type=markdown" }, 1023, "MainContentReference", false, false });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "DialogContentType",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DropColumn(
                name: "AllowedMediaTypes",
                table: "DialogContentType");

            migrationBuilder.DropColumn(
                name: "MediaType",
                table: "DialogContent");

            migrationBuilder.AddColumn<string>(
                name: "MediaType",
                table: "DialogElementUrl",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "RenderAsHtml",
                table: "DialogContentType",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "DialogContentType",
                keyColumn: "Id",
                keyValue: 1,
                column: "RenderAsHtml",
                value: false);

            migrationBuilder.UpdateData(
                table: "DialogContentType",
                keyColumn: "Id",
                keyValue: 2,
                column: "RenderAsHtml",
                value: false);

            migrationBuilder.UpdateData(
                table: "DialogContentType",
                keyColumn: "Id",
                keyValue: 3,
                column: "RenderAsHtml",
                value: false);

            migrationBuilder.UpdateData(
                table: "DialogContentType",
                keyColumn: "Id",
                keyValue: 4,
                column: "RenderAsHtml",
                value: true);

            migrationBuilder.UpdateData(
                table: "DialogContentType",
                keyColumn: "Id",
                keyValue: 5,
                column: "RenderAsHtml",
                value: false);
        }
    }
}

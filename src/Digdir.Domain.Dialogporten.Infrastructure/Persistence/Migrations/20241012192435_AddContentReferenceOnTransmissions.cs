using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Digdir.Domain.Dialogporten.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddContentReferenceOnTransmissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "DialogTransmissionContentType",
                columns: new[] { "Id", "AllowedMediaTypes", "MaxLength", "Name", "Required" },
                values: new object[] { 3, new[] { "application/vnd.dialogporten.frontchannelembed+json;type=markdown" }, 1023, "ContentReference", false });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "DialogTransmissionContentType",
                keyColumn: "Id",
                keyValue: 3);
        }
    }
}

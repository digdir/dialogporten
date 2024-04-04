using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Digdir.Domain.Dialogporten.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddExtendedStatusContentType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "DialogContentType",
                columns: new[] { "Id", "MaxLength", "Name", "OutputInList", "RenderAsHtml", "Required" },
                values: new object[] { 5, 20, "ExtendedStatus", true, false, false });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "DialogContentType",
                keyColumn: "Id",
                keyValue: 5);
        }
    }
}

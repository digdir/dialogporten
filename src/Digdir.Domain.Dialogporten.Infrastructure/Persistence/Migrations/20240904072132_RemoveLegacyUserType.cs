using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Digdir.Domain.Dialogporten.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveLegacyUserType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "DialogUserType",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.UpdateData(
                table: "DialogUserType",
                keyColumn: "Id",
                keyValue: 2,
                column: "Name",
                value: "SystemUser");

            migrationBuilder.UpdateData(
                table: "DialogUserType",
                keyColumn: "Id",
                keyValue: 3,
                column: "Name",
                value: "ServiceOwner");

            migrationBuilder.UpdateData(
                table: "DialogUserType",
                keyColumn: "Id",
                keyValue: 4,
                column: "Name",
                value: "ServiceOwnerOnBehalfOfPerson");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "DialogUserType",
                keyColumn: "Id",
                keyValue: 2,
                column: "Name",
                value: "LegacySystemUser");

            migrationBuilder.UpdateData(
                table: "DialogUserType",
                keyColumn: "Id",
                keyValue: 3,
                column: "Name",
                value: "SystemUser");

            migrationBuilder.UpdateData(
                table: "DialogUserType",
                keyColumn: "Id",
                keyValue: 4,
                column: "Name",
                value: "ServiceOwner");

            migrationBuilder.InsertData(
                table: "DialogUserType",
                columns: new[] { "Id", "Name" },
                values: new object[] { 5, "ServiceOwnerOnBehalfOfPerson" });
        }
    }
}

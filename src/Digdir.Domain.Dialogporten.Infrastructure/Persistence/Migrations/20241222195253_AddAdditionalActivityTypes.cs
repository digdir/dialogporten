using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Digdir.Domain.Dialogporten.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAdditionalActivityTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "DialogActivityType",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 8, "DialogDeleted" },
                    { 9, "DialogRestored" },
                    { 10, "SentToSigning" },
                    { 11, "SentToFormFill" },
                    { 12, "SentToSendIn" },
                    { 13, "SentToPayment" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "DialogActivityType",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "DialogActivityType",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "DialogActivityType",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "DialogActivityType",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "DialogActivityType",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "DialogActivityType",
                keyColumn: "Id",
                keyValue: 13);
        }
    }
}

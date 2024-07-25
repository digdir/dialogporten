using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Digdir.Domain.Dialogporten.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RefactorDialogActivityTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "DialogActivityType",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.UpdateData(
                table: "DialogActivityType",
                keyColumn: "Id",
                keyValue: 1,
                column: "Name",
                value: "DialogCreated");

            migrationBuilder.UpdateData(
                table: "DialogActivityType",
                keyColumn: "Id",
                keyValue: 2,
                column: "Name",
                value: "DialogClosed");

            migrationBuilder.UpdateData(
                table: "DialogActivityType",
                keyColumn: "Id",
                keyValue: 4,
                column: "Name",
                value: "TransmissionOpened");

            migrationBuilder.UpdateData(
                table: "DialogActivityType",
                keyColumn: "Id",
                keyValue: 5,
                column: "Name",
                value: "PaymentMade");

            migrationBuilder.InsertData(
                table: "DialogActivityType",
                columns: new[] { "Id", "Name" },
                values: new object[] { 6, "SignatureProvided" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "DialogActivityType",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.UpdateData(
                table: "DialogActivityType",
                keyColumn: "Id",
                keyValue: 1,
                column: "Name",
                value: "Submission");

            migrationBuilder.UpdateData(
                table: "DialogActivityType",
                keyColumn: "Id",
                keyValue: 2,
                column: "Name",
                value: "Feedback");

            migrationBuilder.UpdateData(
                table: "DialogActivityType",
                keyColumn: "Id",
                keyValue: 4,
                column: "Name",
                value: "Error");

            migrationBuilder.UpdateData(
                table: "DialogActivityType",
                keyColumn: "Id",
                keyValue: 5,
                column: "Name",
                value: "Closed");

            migrationBuilder.InsertData(
                table: "DialogActivityType",
                columns: new[] { "Id", "Name" },
                values: new object[] { 7, "Forwarded" });
        }
    }
}

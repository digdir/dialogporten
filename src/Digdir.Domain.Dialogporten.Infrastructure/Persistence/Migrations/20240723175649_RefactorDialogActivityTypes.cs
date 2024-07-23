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
                value: "DialogCompleted");

            migrationBuilder.UpdateData(
                table: "DialogActivityType",
                keyColumn: "Id",
                keyValue: 3,
                column: "Name",
                value: "DialogClosed");

            migrationBuilder.UpdateData(
                table: "DialogActivityType",
                keyColumn: "Id",
                keyValue: 4,
                column: "Name",
                value: "Information");

            migrationBuilder.UpdateData(
                table: "DialogActivityType",
                keyColumn: "Id",
                keyValue: 5,
                column: "Name",
                value: "TransmissionOpened");

            migrationBuilder.UpdateData(
                table: "DialogActivityType",
                keyColumn: "Id",
                keyValue: 7,
                column: "Name",
                value: "SignatureProvided");

            migrationBuilder.InsertData(
                table: "DialogActivityType",
                columns: new[] { "Id", "Name" },
                values: new object[] { 6, "PaymentMade" });
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
                keyValue: 3,
                column: "Name",
                value: "Information");

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

            migrationBuilder.UpdateData(
                table: "DialogActivityType",
                keyColumn: "Id",
                keyValue: 7,
                column: "Name",
                value: "Forwarded");
        }
    }
}

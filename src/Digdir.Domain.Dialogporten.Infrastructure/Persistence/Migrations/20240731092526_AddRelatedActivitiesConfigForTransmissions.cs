using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Digdir.Domain.Dialogporten.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRelatedActivitiesConfigForTransmissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DialogTransmission_DialogTransmission_RelatedTransmissionId",
                table: "DialogTransmission");

            migrationBuilder.AddForeignKey(
                name: "FK_DialogTransmission_DialogTransmission_RelatedTransmissionId",
                table: "DialogTransmission",
                column: "RelatedTransmissionId",
                principalTable: "DialogTransmission",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DialogTransmission_DialogTransmission_RelatedTransmissionId",
                table: "DialogTransmission");

            migrationBuilder.AddForeignKey(
                name: "FK_DialogTransmission_DialogTransmission_RelatedTransmissionId",
                table: "DialogTransmission",
                column: "RelatedTransmissionId",
                principalTable: "DialogTransmission",
                principalColumn: "Id");
        }
    }
}

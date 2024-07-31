using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Digdir.Domain.Dialogporten.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTransmissionRelationOnActivity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "TransmissionId",
                table: "DialogActivity",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DialogActivity_TransmissionId",
                table: "DialogActivity",
                column: "TransmissionId");

            migrationBuilder.AddForeignKey(
                name: "FK_DialogActivity_DialogTransmission_TransmissionId",
                table: "DialogActivity",
                column: "TransmissionId",
                principalTable: "DialogTransmission",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DialogActivity_DialogTransmission_TransmissionId",
                table: "DialogActivity");

            migrationBuilder.DropIndex(
                name: "IX_DialogActivity_TransmissionId",
                table: "DialogActivity");

            migrationBuilder.DropColumn(
                name: "TransmissionId",
                table: "DialogActivity");
        }
    }
}

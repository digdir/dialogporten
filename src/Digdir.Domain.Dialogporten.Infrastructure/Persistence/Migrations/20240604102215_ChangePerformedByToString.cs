using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Digdir.Domain.Dialogporten.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ChangePerformedByToString : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LocalizationSet_DialogActivity_DialogActivityDescription_Ac~",
                table: "LocalizationSet");

            migrationBuilder.DropIndex(
                name: "IX_LocalizationSet_DialogActivityDescription_ActivityId",
                table: "LocalizationSet");

            migrationBuilder.DropColumn(
                name: "DialogActivityDescription_ActivityId",
                table: "LocalizationSet");

            migrationBuilder.AddColumn<string>(
                name: "PerformedBy",
                table: "DialogActivity",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PerformedBy",
                table: "DialogActivity");

            migrationBuilder.AddColumn<Guid>(
                name: "DialogActivityDescription_ActivityId",
                table: "LocalizationSet",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_LocalizationSet_DialogActivityDescription_ActivityId",
                table: "LocalizationSet",
                column: "DialogActivityDescription_ActivityId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_LocalizationSet_DialogActivity_DialogActivityDescription_Ac~",
                table: "LocalizationSet",
                column: "DialogActivityDescription_ActivityId",
                principalTable: "DialogActivity",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

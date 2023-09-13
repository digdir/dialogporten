using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Digdir.Domain.Dialogporten.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ChangeSearchTitleToSearchTagsOnDialogEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LocalizationSet_Dialog_DialogSearchTitle_DialogId",
                table: "LocalizationSet");

            migrationBuilder.DropIndex(
                name: "IX_LocalizationSet_DialogSearchTitle_DialogId",
                table: "LocalizationSet");

            migrationBuilder.DropColumn(
                name: "DialogSearchTitle_DialogId",
                table: "LocalizationSet");

            migrationBuilder.CreateTable(
                name: "DialogSearchTag",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    Value = table.Column<string>(type: "character varying(63)", maxLength: 63, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "current_timestamp at time zone 'utc'"),
                    DialogId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DialogSearchTag", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DialogSearchTag_Dialog_DialogId",
                        column: x => x.DialogId,
                        principalTable: "Dialog",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Dialog_DueAt",
                table: "Dialog",
                column: "DueAt");

            migrationBuilder.CreateIndex(
                name: "IX_Dialog_UpdatedAt",
                table: "Dialog",
                column: "UpdatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_DialogSearchTag_DialogId_Value",
                table: "DialogSearchTag",
                columns: new[] { "DialogId", "Value" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DialogSearchTag");

            migrationBuilder.DropIndex(
                name: "IX_Dialog_DueAt",
                table: "Dialog");

            migrationBuilder.DropIndex(
                name: "IX_Dialog_UpdatedAt",
                table: "Dialog");

            migrationBuilder.AddColumn<Guid>(
                name: "DialogSearchTitle_DialogId",
                table: "LocalizationSet",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_LocalizationSet_DialogSearchTitle_DialogId",
                table: "LocalizationSet",
                column: "DialogSearchTitle_DialogId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_LocalizationSet_Dialog_DialogSearchTitle_DialogId",
                table: "LocalizationSet",
                column: "DialogSearchTitle_DialogId",
                principalTable: "Dialog",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Digdir.Domain.Dialogporten.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SplitSeenLogFromActivityToSeparateEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "DialogActivity",
                keyColumn: "TypeId",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "DialogActivityType",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DropColumn(
                name: "SeenByEndUserId",
                table: "DialogActivity");

            migrationBuilder.AddColumn<Guid>(
                name: "DialogSeenLogId",
                table: "LocalizationSet",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DialogSeenRecord",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "current_timestamp at time zone 'utc'"),
                    EndUserId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    EndUserName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    DialogId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DialogSeenRecord", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DialogSeenRecord_Dialog_DialogId",
                        column: x => x.DialogId,
                        principalTable: "Dialog",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LocalizationSet_DialogSeenLogId",
                table: "LocalizationSet",
                column: "DialogSeenLogId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DialogSeenRecord_DialogId",
                table: "DialogSeenRecord",
                column: "DialogId");

            migrationBuilder.AddForeignKey(
                name: "FK_LocalizationSet_DialogSeenRecord_DialogSeenLogId",
                table: "LocalizationSet",
                column: "DialogSeenLogId",
                principalTable: "DialogSeenRecord",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LocalizationSet_DialogSeenRecord_DialogSeenLogId",
                table: "LocalizationSet");

            migrationBuilder.DropTable(
                name: "DialogSeenRecord");

            migrationBuilder.DropIndex(
                name: "IX_LocalizationSet_DialogSeenLogId",
                table: "LocalizationSet");

            migrationBuilder.DropColumn(
                name: "DialogSeenLogId",
                table: "LocalizationSet");

            migrationBuilder.AddColumn<string>(
                name: "SeenByEndUserId",
                table: "DialogActivity",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.InsertData(
                table: "DialogActivityType",
                columns: new[] { "Id", "Name" },
                values: new object[] { 6, "Seen" });
        }
    }
}

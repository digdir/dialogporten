using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Digdir.Domain.Dialogporten.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RenameFromSeenRecordToSeenLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LocalizationSet_DialogSeenRecord_DialogSeenLogId",
                table: "LocalizationSet");

            migrationBuilder.DropTable(
                name: "DialogSeenRecord");

            migrationBuilder.CreateTable(
                name: "DialogSeenLog",
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
                    table.PrimaryKey("PK_DialogSeenLog", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DialogSeenLog_Dialog_DialogId",
                        column: x => x.DialogId,
                        principalTable: "Dialog",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DialogSeenLog_DialogId",
                table: "DialogSeenLog",
                column: "DialogId");

            migrationBuilder.AddForeignKey(
                name: "FK_LocalizationSet_DialogSeenLog_DialogSeenLogId",
                table: "LocalizationSet",
                column: "DialogSeenLogId",
                principalTable: "DialogSeenLog",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LocalizationSet_DialogSeenLog_DialogSeenLogId",
                table: "LocalizationSet");

            migrationBuilder.DropTable(
                name: "DialogSeenLog");

            migrationBuilder.CreateTable(
                name: "DialogSeenRecord",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    DialogId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "current_timestamp at time zone 'utc'"),
                    EndUserId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    EndUserName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
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
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Digdir.Domain.Dialogporten.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTableDialogContent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LocalizationSet_Dialog_DialogBody_DialogId",
                table: "LocalizationSet");

            migrationBuilder.DropForeignKey(
                name: "FK_LocalizationSet_Dialog_DialogId",
                table: "LocalizationSet");

            migrationBuilder.DropForeignKey(
                name: "FK_LocalizationSet_Dialog_DialogSenderName_DialogId",
                table: "LocalizationSet");

            migrationBuilder.DropIndex(
                name: "IX_LocalizationSet_DialogBody_DialogId",
                table: "LocalizationSet");

            migrationBuilder.DropIndex(
                name: "IX_LocalizationSet_DialogId",
                table: "LocalizationSet");

            migrationBuilder.DropColumn(
                name: "DialogBody_DialogId",
                table: "LocalizationSet");

            migrationBuilder.DropColumn(
                name: "DialogId",
                table: "LocalizationSet");

            migrationBuilder.RenameColumn(
                name: "DialogSenderName_DialogId",
                table: "LocalizationSet",
                newName: "DialogContentId");

            migrationBuilder.RenameIndex(
                name: "IX_LocalizationSet_DialogSenderName_DialogId",
                table: "LocalizationSet",
                newName: "IX_LocalizationSet_DialogContentId");

            migrationBuilder.CreateTable(
                name: "DialogContentType",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Required = table.Column<bool>(type: "boolean", nullable: false),
                    RenderAsHtml = table.Column<bool>(type: "boolean", nullable: false),
                    OutputInList = table.Column<bool>(type: "boolean", nullable: false),
                    MaxLength = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DialogContentType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DialogContent",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "current_timestamp at time zone 'utc'"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "current_timestamp at time zone 'utc'"),
                    DialogId = table.Column<Guid>(type: "uuid", nullable: false),
                    TypeId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DialogContent", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DialogContent_DialogContentType_TypeId",
                        column: x => x.TypeId,
                        principalTable: "DialogContentType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DialogContent_Dialog_DialogId",
                        column: x => x.DialogId,
                        principalTable: "Dialog",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "DialogContentType",
                columns: new[] { "Id", "MaxLength", "Name", "OutputInList", "RenderAsHtml", "Required" },
                values: new object[,]
                {
                    { 1, 255, "Title", true, false, true },
                    { 2, 255, "SenderName", true, false, false },
                    { 3, 255, "Summary", true, false, true },
                    { 4, 1023, "AdditionalInfo", false, true, false }
                });

            migrationBuilder.CreateIndex(
                name: "IX_DialogContent_DialogId_TypeId",
                table: "DialogContent",
                columns: new[] { "DialogId", "TypeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DialogContent_TypeId",
                table: "DialogContent",
                column: "TypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_LocalizationSet_DialogContent_DialogContentId",
                table: "LocalizationSet",
                column: "DialogContentId",
                principalTable: "DialogContent",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LocalizationSet_DialogContent_DialogContentId",
                table: "LocalizationSet");

            migrationBuilder.DropTable(
                name: "DialogContent");

            migrationBuilder.DropTable(
                name: "DialogContentType");

            migrationBuilder.RenameColumn(
                name: "DialogContentId",
                table: "LocalizationSet",
                newName: "DialogSenderName_DialogId");

            migrationBuilder.RenameIndex(
                name: "IX_LocalizationSet_DialogContentId",
                table: "LocalizationSet",
                newName: "IX_LocalizationSet_DialogSenderName_DialogId");

            migrationBuilder.AddColumn<Guid>(
                name: "DialogBody_DialogId",
                table: "LocalizationSet",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DialogId",
                table: "LocalizationSet",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_LocalizationSet_DialogBody_DialogId",
                table: "LocalizationSet",
                column: "DialogBody_DialogId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LocalizationSet_DialogId",
                table: "LocalizationSet",
                column: "DialogId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_LocalizationSet_Dialog_DialogBody_DialogId",
                table: "LocalizationSet",
                column: "DialogBody_DialogId",
                principalTable: "Dialog",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LocalizationSet_Dialog_DialogId",
                table: "LocalizationSet",
                column: "DialogId",
                principalTable: "Dialog",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LocalizationSet_Dialog_DialogSenderName_DialogId",
                table: "LocalizationSet",
                column: "DialogSenderName_DialogId",
                principalTable: "Dialog",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

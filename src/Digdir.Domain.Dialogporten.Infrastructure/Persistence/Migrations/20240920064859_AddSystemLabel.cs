using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Digdir.Domain.Dialogporten.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSystemLabel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SystemLabel",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemLabel", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DialogEndUserContext",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "current_timestamp at time zone 'utc'"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "current_timestamp at time zone 'utc'"),
                    DialogId = table.Column<Guid>(type: "uuid", nullable: true),
                    SystemLabelId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DialogEndUserContext", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DialogEndUserContext_Dialog_DialogId",
                        column: x => x.DialogId,
                        principalTable: "Dialog",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DialogEndUserContext_SystemLabel_SystemLabelId",
                        column: x => x.SystemLabelId,
                        principalTable: "SystemLabel",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "SystemLabel",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Default" },
                    { 2, "Trash" },
                    { 3, "Archive" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_DialogEndUserContext_DialogId",
                table: "DialogEndUserContext",
                column: "DialogId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DialogEndUserContext_SystemLabelId",
                table: "DialogEndUserContext",
                column: "SystemLabelId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DialogEndUserContext");

            migrationBuilder.DropTable(
                name: "SystemLabel");
        }
    }
}

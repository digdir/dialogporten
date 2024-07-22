using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Digdir.Domain.Dialogporten.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddActor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LocalizationSet_DialogSeenLog_DialogSeenLogId",
                table: "LocalizationSet");

            migrationBuilder.DropIndex(
                name: "IX_LocalizationSet_DialogSeenLogId",
                table: "LocalizationSet");

            migrationBuilder.DropColumn(
                name: "DialogSeenLogId",
                table: "LocalizationSet");

            migrationBuilder.DropColumn(
                name: "EndUserId",
                table: "DialogSeenLog");

            migrationBuilder.DropColumn(
                name: "EndUserName",
                table: "DialogSeenLog");

            migrationBuilder.DropColumn(
                name: "PerformedBy",
                table: "DialogActivity");

            migrationBuilder.AddColumn<bool>(
                name: "IsViaServiceOwner",
                table: "DialogSeenLog",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SeenById",
                table: "DialogSeenLog",
                type: "uuid",
                nullable: false);

            migrationBuilder.AddColumn<Guid>(
                name: "PerformedById",
                table: "DialogActivity",
                type: "uuid",
                nullable: false);

            migrationBuilder.CreateTable(
                name: "DialogActorType",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DialogActorType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DialogActor",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    ActorId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    ActorTypeId = table.Column<int>(type: "integer", nullable: false),
                    ActorName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DialogActor", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DialogActor_DialogActorType_ActorTypeId",
                        column: x => x.ActorTypeId,
                        principalTable: "DialogActorType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "DialogActorType",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "PartyRepresentative" },
                    { 2, "ServiceOwner" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_DialogSeenLog_SeenById",
                table: "DialogSeenLog",
                column: "SeenById");

            migrationBuilder.CreateIndex(
                name: "IX_DialogActivity_PerformedById",
                table: "DialogActivity",
                column: "PerformedById");

            migrationBuilder.CreateIndex(
                name: "IX_DialogActor_ActorTypeId",
                table: "DialogActor",
                column: "ActorTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_DialogActivity_DialogActor_PerformedById",
                table: "DialogActivity",
                column: "PerformedById",
                principalTable: "DialogActor",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DialogSeenLog_DialogActor_SeenById",
                table: "DialogSeenLog",
                column: "SeenById",
                principalTable: "DialogActor",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DialogActivity_DialogActor_PerformedById",
                table: "DialogActivity");

            migrationBuilder.DropForeignKey(
                name: "FK_DialogSeenLog_DialogActor_SeenById",
                table: "DialogSeenLog");

            migrationBuilder.DropTable(
                name: "DialogActor");

            migrationBuilder.DropTable(
                name: "DialogActorType");

            migrationBuilder.DropIndex(
                name: "IX_DialogSeenLog_SeenById",
                table: "DialogSeenLog");

            migrationBuilder.DropIndex(
                name: "IX_DialogActivity_PerformedById",
                table: "DialogActivity");

            migrationBuilder.DropColumn(
                name: "IsViaServiceOwner",
                table: "DialogSeenLog");

            migrationBuilder.DropColumn(
                name: "SeenById",
                table: "DialogSeenLog");

            migrationBuilder.DropColumn(
                name: "PerformedById",
                table: "DialogActivity");

            migrationBuilder.AddColumn<Guid>(
                name: "DialogSeenLogId",
                table: "LocalizationSet",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EndUserId",
                table: "DialogSeenLog",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "EndUserName",
                table: "DialogSeenLog",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PerformedBy",
                table: "DialogActivity",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_LocalizationSet_DialogSeenLogId",
                table: "LocalizationSet",
                column: "DialogSeenLogId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_LocalizationSet_DialogSeenLog_DialogSeenLogId",
                table: "LocalizationSet",
                column: "DialogSeenLogId",
                principalTable: "DialogSeenLog",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

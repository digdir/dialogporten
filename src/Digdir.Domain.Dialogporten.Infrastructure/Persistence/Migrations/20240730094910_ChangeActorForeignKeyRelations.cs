using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Digdir.Domain.Dialogporten.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ChangeActorForeignKeyRelations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DialogActivity_DialogActor_PerformedById",
                table: "DialogActivity");

            migrationBuilder.DropForeignKey(
                name: "FK_DialogSeenLog_DialogActor_SeenById",
                table: "DialogSeenLog");

            migrationBuilder.DropForeignKey(
                name: "FK_DialogTransmission_DialogActor_SenderId",
                table: "DialogTransmission");

            migrationBuilder.DropTable(
                name: "DialogActor");

            migrationBuilder.DropIndex(
                name: "IX_DialogTransmission_SenderId",
                table: "DialogTransmission");

            migrationBuilder.DropIndex(
                name: "IX_DialogSeenLog_SeenById",
                table: "DialogSeenLog");

            migrationBuilder.DropIndex(
                name: "IX_DialogActivity_PerformedById",
                table: "DialogActivity");

            migrationBuilder.DropColumn(
                name: "SenderId",
                table: "DialogTransmission");

            migrationBuilder.DropColumn(
                name: "SeenById",
                table: "DialogSeenLog");

            migrationBuilder.DropColumn(
                name: "PerformedById",
                table: "DialogActivity");

            migrationBuilder.CreateTable(
                name: "Actor",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    ActorId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    ActorTypeId = table.Column<int>(type: "integer", nullable: false),
                    ActorName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Discriminator = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ActivityId = table.Column<Guid>(type: "uuid", nullable: true),
                    DialogSeenLogId = table.Column<Guid>(type: "uuid", nullable: true),
                    TransmissionId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Actor", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Actor_DialogActivity_ActivityId",
                        column: x => x.ActivityId,
                        principalTable: "DialogActivity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Actor_DialogActorType_ActorTypeId",
                        column: x => x.ActorTypeId,
                        principalTable: "DialogActorType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Actor_DialogSeenLog_DialogSeenLogId",
                        column: x => x.DialogSeenLogId,
                        principalTable: "DialogSeenLog",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Actor_DialogTransmission_TransmissionId",
                        column: x => x.TransmissionId,
                        principalTable: "DialogTransmission",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Actor_ActivityId",
                table: "Actor",
                column: "ActivityId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Actor_ActorTypeId",
                table: "Actor",
                column: "ActorTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Actor_DialogSeenLogId",
                table: "Actor",
                column: "DialogSeenLogId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Actor_TransmissionId",
                table: "Actor",
                column: "TransmissionId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Actor");

            migrationBuilder.AddColumn<Guid>(
                name: "SenderId",
                table: "DialogTransmission",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "SeenById",
                table: "DialogSeenLog",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "PerformedById",
                table: "DialogActivity",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "DialogActor",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    ActorTypeId = table.Column<int>(type: "integer", nullable: false),
                    ActorId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
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

            migrationBuilder.CreateIndex(
                name: "IX_DialogTransmission_SenderId",
                table: "DialogTransmission",
                column: "SenderId");

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

            migrationBuilder.AddForeignKey(
                name: "FK_DialogTransmission_DialogActor_SenderId",
                table: "DialogTransmission",
                column: "SenderId",
                principalTable: "DialogActor",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

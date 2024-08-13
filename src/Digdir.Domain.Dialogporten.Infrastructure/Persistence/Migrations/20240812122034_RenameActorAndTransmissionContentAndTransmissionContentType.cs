using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Digdir.Domain.Dialogporten.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RenameActorAndTransmissionContentAndTransmissionContentType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Actor_DialogActorType_ActorTypeId",
                table: "Actor");

            migrationBuilder.DropForeignKey(
                name: "FK_LocalizationSet_TransmissionContent_TransmissionContentId",
                table: "LocalizationSet");

            migrationBuilder.DropForeignKey(
                name: "FK_TransmissionContent_DialogTransmission_TransmissionId",
                table: "TransmissionContent");

            migrationBuilder.DropForeignKey(
                name: "FK_TransmissionContent_TransmissionContentType_TypeId",
                table: "TransmissionContent");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TransmissionContentType",
                table: "TransmissionContentType");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TransmissionContent",
                table: "TransmissionContent");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DialogActorType",
                table: "DialogActorType");

            migrationBuilder.RenameTable(
                name: "TransmissionContentType",
                newName: "DialogTransmissionContentType");

            migrationBuilder.RenameTable(
                name: "TransmissionContent",
                newName: "DialogTransmissionContent");

            migrationBuilder.RenameTable(
                name: "DialogActorType",
                newName: "ActorType");

            migrationBuilder.RenameIndex(
                name: "IX_TransmissionContent_TypeId",
                table: "DialogTransmissionContent",
                newName: "IX_DialogTransmissionContent_TypeId");

            migrationBuilder.RenameIndex(
                name: "IX_TransmissionContent_TransmissionId_TypeId",
                table: "DialogTransmissionContent",
                newName: "IX_DialogTransmissionContent_TransmissionId_TypeId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DialogTransmissionContentType",
                table: "DialogTransmissionContentType",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DialogTransmissionContent",
                table: "DialogTransmissionContent",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ActorType",
                table: "ActorType",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Actor_ActorType_ActorTypeId",
                table: "Actor",
                column: "ActorTypeId",
                principalTable: "ActorType",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DialogTransmissionContent_DialogTransmissionContentType_Typ~",
                table: "DialogTransmissionContent",
                column: "TypeId",
                principalTable: "DialogTransmissionContentType",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DialogTransmissionContent_DialogTransmission_TransmissionId",
                table: "DialogTransmissionContent",
                column: "TransmissionId",
                principalTable: "DialogTransmission",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LocalizationSet_DialogTransmissionContent_TransmissionConte~",
                table: "LocalizationSet",
                column: "TransmissionContentId",
                principalTable: "DialogTransmissionContent",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Actor_ActorType_ActorTypeId",
                table: "Actor");

            migrationBuilder.DropForeignKey(
                name: "FK_DialogTransmissionContent_DialogTransmissionContentType_Typ~",
                table: "DialogTransmissionContent");

            migrationBuilder.DropForeignKey(
                name: "FK_DialogTransmissionContent_DialogTransmission_TransmissionId",
                table: "DialogTransmissionContent");

            migrationBuilder.DropForeignKey(
                name: "FK_LocalizationSet_DialogTransmissionContent_TransmissionConte~",
                table: "LocalizationSet");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DialogTransmissionContentType",
                table: "DialogTransmissionContentType");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DialogTransmissionContent",
                table: "DialogTransmissionContent");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ActorType",
                table: "ActorType");

            migrationBuilder.RenameTable(
                name: "DialogTransmissionContentType",
                newName: "TransmissionContentType");

            migrationBuilder.RenameTable(
                name: "DialogTransmissionContent",
                newName: "TransmissionContent");

            migrationBuilder.RenameTable(
                name: "ActorType",
                newName: "DialogActorType");

            migrationBuilder.RenameIndex(
                name: "IX_DialogTransmissionContent_TypeId",
                table: "TransmissionContent",
                newName: "IX_TransmissionContent_TypeId");

            migrationBuilder.RenameIndex(
                name: "IX_DialogTransmissionContent_TransmissionId_TypeId",
                table: "TransmissionContent",
                newName: "IX_TransmissionContent_TransmissionId_TypeId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TransmissionContentType",
                table: "TransmissionContentType",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TransmissionContent",
                table: "TransmissionContent",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DialogActorType",
                table: "DialogActorType",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Actor_DialogActorType_ActorTypeId",
                table: "Actor",
                column: "ActorTypeId",
                principalTable: "DialogActorType",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_LocalizationSet_TransmissionContent_TransmissionContentId",
                table: "LocalizationSet",
                column: "TransmissionContentId",
                principalTable: "TransmissionContent",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TransmissionContent_DialogTransmission_TransmissionId",
                table: "TransmissionContent",
                column: "TransmissionId",
                principalTable: "DialogTransmission",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TransmissionContent_TransmissionContentType_TypeId",
                table: "TransmissionContent",
                column: "TypeId",
                principalTable: "TransmissionContentType",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}

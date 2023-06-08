using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Digdir.Domain.Dialogporten.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateActivityAndDialogelement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DialogActivity_DialogElement_RelatedDialogElementInternalId",
                table: "DialogActivity");

            migrationBuilder.DropColumn(
                name: "ContentSchema",
                table: "DialogElementUrl");

            migrationBuilder.DropColumn(
                name: "Url",
                table: "DialogElement");

            migrationBuilder.RenameColumn(
                name: "ContentType",
                table: "DialogElementUrl",
                newName: "ContentTypeHint");

            migrationBuilder.RenameColumn(
                name: "RelatedDialogElementInternalId",
                table: "DialogActivity",
                newName: "RelatedActivityInternalId");

            migrationBuilder.RenameColumn(
                name: "RelatedDialogElementId",
                table: "DialogActivity",
                newName: "RelatedActivityId");

            migrationBuilder.RenameIndex(
                name: "IX_DialogActivity_RelatedDialogElementInternalId",
                table: "DialogActivity",
                newName: "IX_DialogActivity_RelatedActivityInternalId");

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "DialogElement",
                type: "character varying(1023)",
                maxLength: 1023,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ExtendedType",
                table: "DialogActivity",
                type: "character varying(1023)",
                maxLength: 1023,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255,
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DialogElementId",
                table: "DialogActivity",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "DialogElementInternalId",
                table: "DialogActivity",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "IX_DialogActivity_DialogElementInternalId",
                table: "DialogActivity",
                column: "DialogElementInternalId");

            migrationBuilder.AddForeignKey(
                name: "FK_DialogActivity_DialogActivity_RelatedActivityInternalId",
                table: "DialogActivity",
                column: "RelatedActivityInternalId",
                principalTable: "DialogActivity",
                principalColumn: "InternalId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DialogActivity_DialogElement_DialogElementInternalId",
                table: "DialogActivity",
                column: "DialogElementInternalId",
                principalTable: "DialogElement",
                principalColumn: "InternalId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DialogActivity_DialogActivity_RelatedActivityInternalId",
                table: "DialogActivity");

            migrationBuilder.DropForeignKey(
                name: "FK_DialogActivity_DialogElement_DialogElementInternalId",
                table: "DialogActivity");

            migrationBuilder.DropIndex(
                name: "IX_DialogActivity_DialogElementInternalId",
                table: "DialogActivity");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "DialogElement");

            migrationBuilder.DropColumn(
                name: "DialogElementId",
                table: "DialogActivity");

            migrationBuilder.DropColumn(
                name: "DialogElementInternalId",
                table: "DialogActivity");

            migrationBuilder.RenameColumn(
                name: "ContentTypeHint",
                table: "DialogElementUrl",
                newName: "ContentType");

            migrationBuilder.RenameColumn(
                name: "RelatedActivityInternalId",
                table: "DialogActivity",
                newName: "RelatedDialogElementInternalId");

            migrationBuilder.RenameColumn(
                name: "RelatedActivityId",
                table: "DialogActivity",
                newName: "RelatedDialogElementId");

            migrationBuilder.RenameIndex(
                name: "IX_DialogActivity_RelatedActivityInternalId",
                table: "DialogActivity",
                newName: "IX_DialogActivity_RelatedDialogElementInternalId");

            migrationBuilder.AddColumn<string>(
                name: "ContentSchema",
                table: "DialogElementUrl",
                type: "character varying(1023)",
                maxLength: 1023,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Url",
                table: "DialogElement",
                type: "character varying(1023)",
                maxLength: 1023,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "ExtendedType",
                table: "DialogActivity",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(1023)",
                oldMaxLength: 1023,
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_DialogActivity_DialogElement_RelatedDialogElementInternalId",
                table: "DialogActivity",
                column: "RelatedDialogElementInternalId",
                principalTable: "DialogElement",
                principalColumn: "InternalId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

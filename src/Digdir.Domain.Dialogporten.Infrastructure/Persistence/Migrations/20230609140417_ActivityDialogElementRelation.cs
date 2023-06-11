using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Digdir.Domain.Dialogporten.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ActivityDialogElementRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DialogActivity_DialogActivity_RelatedActivityInternalId",
                table: "DialogActivity");

            migrationBuilder.DropForeignKey(
                name: "FK_DialogActivity_DialogElement_DialogElementInternalId",
                table: "DialogActivity");

            migrationBuilder.DropForeignKey(
                name: "FK_DialogElement_DialogElement_RelatedDialogElementInternalId",
                table: "DialogElement");

            migrationBuilder.DropIndex(
                name: "IX_DialogElement_RelatedDialogElementInternalId",
                table: "DialogElement");

            migrationBuilder.DropIndex(
                name: "IX_DialogActivity_DialogElementInternalId",
                table: "DialogActivity");

            migrationBuilder.DropIndex(
                name: "IX_DialogActivity_RelatedActivityInternalId",
                table: "DialogActivity");

            migrationBuilder.DropColumn(
                name: "RelatedDialogElementInternalId",
                table: "DialogElement");

            migrationBuilder.DropColumn(
                name: "DialogElementInternalId",
                table: "DialogActivity");

            migrationBuilder.DropColumn(
                name: "RelatedActivityInternalId",
                table: "DialogActivity");

            migrationBuilder.DropColumn(
                name: "RelatedDialogElementId",
                table: "DialogElement");

            migrationBuilder.DropColumn(
                name: "RelatedActivityId",
                table: "DialogActivity");

            migrationBuilder.DropColumn(
                name: "DialogElementId",
                table: "DialogActivity");

            migrationBuilder.AddColumn<Guid>(
                name: "RelatedDialogElementId",
                table: "DialogElement",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RelatedActivityId",
                table: "DialogActivity",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DialogElementId",
                table: "DialogActivity",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DialogElement_RelatedDialogElementId",
                table: "DialogElement",
                column: "RelatedDialogElementId");

            migrationBuilder.CreateIndex(
                name: "IX_DialogActivity_DialogElementId",
                table: "DialogActivity",
                column: "DialogElementId");

            migrationBuilder.CreateIndex(
                name: "IX_DialogActivity_RelatedActivityId",
                table: "DialogActivity",
                column: "RelatedActivityId");

            migrationBuilder.AddForeignKey(
                name: "FK_DialogActivity_DialogActivity_RelatedActivityId",
                table: "DialogActivity",
                column: "RelatedActivityId",
                principalTable: "DialogActivity",
                principalColumn: "InternalId");

            migrationBuilder.AddForeignKey(
                name: "FK_DialogActivity_DialogElement_DialogElementId",
                table: "DialogActivity",
                column: "DialogElementId",
                principalTable: "DialogElement",
                principalColumn: "InternalId");

            migrationBuilder.AddForeignKey(
                name: "FK_DialogElement_DialogElement_RelatedDialogElementId",
                table: "DialogElement",
                column: "RelatedDialogElementId",
                principalTable: "DialogElement",
                principalColumn: "InternalId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DialogActivity_DialogActivity_RelatedActivityId",
                table: "DialogActivity");

            migrationBuilder.DropForeignKey(
                name: "FK_DialogActivity_DialogElement_DialogElementId",
                table: "DialogActivity");

            migrationBuilder.DropForeignKey(
                name: "FK_DialogElement_DialogElement_RelatedDialogElementId",
                table: "DialogElement");

            migrationBuilder.DropIndex(
                name: "IX_DialogElement_RelatedDialogElementId",
                table: "DialogElement");

            migrationBuilder.DropIndex(
                name: "IX_DialogActivity_DialogElementId",
                table: "DialogActivity");

            migrationBuilder.DropIndex(
                name: "IX_DialogActivity_RelatedActivityId",
                table: "DialogActivity");

            migrationBuilder.AlterColumn<Guid>(
                name: "RelatedDialogElementId",
                table: "DialogElement",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AddColumn<long>(
                name: "RelatedDialogElementInternalId",
                table: "DialogElement",
                type: "bigint",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "RelatedActivityId",
                table: "DialogActivity",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "DialogElementId",
                table: "DialogActivity",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AddColumn<long>(
                name: "DialogElementInternalId",
                table: "DialogActivity",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "RelatedActivityInternalId",
                table: "DialogActivity",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DialogElement_RelatedDialogElementInternalId",
                table: "DialogElement",
                column: "RelatedDialogElementInternalId");

            migrationBuilder.CreateIndex(
                name: "IX_DialogActivity_DialogElementInternalId",
                table: "DialogActivity",
                column: "DialogElementInternalId");

            migrationBuilder.CreateIndex(
                name: "IX_DialogActivity_RelatedActivityInternalId",
                table: "DialogActivity",
                column: "RelatedActivityInternalId");

            migrationBuilder.AddForeignKey(
                name: "FK_DialogActivity_DialogActivity_RelatedActivityInternalId",
                table: "DialogActivity",
                column: "RelatedActivityInternalId",
                principalTable: "DialogActivity",
                principalColumn: "InternalId");

            migrationBuilder.AddForeignKey(
                name: "FK_DialogActivity_DialogElement_DialogElementInternalId",
                table: "DialogActivity",
                column: "DialogElementInternalId",
                principalTable: "DialogElement",
                principalColumn: "InternalId");

            migrationBuilder.AddForeignKey(
                name: "FK_DialogElement_DialogElement_RelatedDialogElementInternalId",
                table: "DialogElement",
                column: "RelatedDialogElementInternalId",
                principalTable: "DialogElement",
                principalColumn: "InternalId");
        }
    }
}

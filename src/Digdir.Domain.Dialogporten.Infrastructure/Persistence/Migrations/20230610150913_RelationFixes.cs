using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Digdir.Domain.Dialogporten.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RelationFixes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.RenameColumn(
                name: "RelatedDialogElementId",
                table: "DialogElement",
                newName: "RelatedDialogElementInternalId");

            migrationBuilder.RenameIndex(
                name: "IX_DialogElement_RelatedDialogElementId",
                table: "DialogElement",
                newName: "IX_DialogElement_RelatedDialogElementInternalId");

            migrationBuilder.RenameColumn(
                name: "RelatedActivityId",
                table: "DialogActivity",
                newName: "RelatedActivityInternalId");

            migrationBuilder.RenameColumn(
                name: "DialogElementId",
                table: "DialogActivity",
                newName: "DialogElementInternalId");

            migrationBuilder.RenameIndex(
                name: "IX_DialogActivity_RelatedActivityId",
                table: "DialogActivity",
                newName: "IX_DialogActivity_RelatedActivityInternalId");

            migrationBuilder.RenameIndex(
                name: "IX_DialogActivity_DialogElementId",
                table: "DialogActivity",
                newName: "IX_DialogActivity_DialogElementInternalId");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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

            migrationBuilder.RenameColumn(
                name: "RelatedDialogElementInternalId",
                table: "DialogElement",
                newName: "RelatedDialogElementId");

            migrationBuilder.RenameIndex(
                name: "IX_DialogElement_RelatedDialogElementInternalId",
                table: "DialogElement",
                newName: "IX_DialogElement_RelatedDialogElementId");

            migrationBuilder.RenameColumn(
                name: "RelatedActivityInternalId",
                table: "DialogActivity",
                newName: "RelatedActivityId");

            migrationBuilder.RenameColumn(
                name: "DialogElementInternalId",
                table: "DialogActivity",
                newName: "DialogElementId");

            migrationBuilder.RenameIndex(
                name: "IX_DialogActivity_RelatedActivityInternalId",
                table: "DialogActivity",
                newName: "IX_DialogActivity_RelatedActivityId");

            migrationBuilder.RenameIndex(
                name: "IX_DialogActivity_DialogElementInternalId",
                table: "DialogActivity",
                newName: "IX_DialogActivity_DialogElementId");

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
    }
}

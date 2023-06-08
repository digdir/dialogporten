using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Digdir.Domain.Dialogporten.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class NullableRelations : Migration
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

            migrationBuilder.AlterColumn<long>(
                name: "RelatedDialogElementInternalId",
                table: "DialogElement",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<long>(
                name: "RelatedActivityInternalId",
                table: "DialogActivity",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<long>(
                name: "DialogElementInternalId",
                table: "DialogActivity",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

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

            migrationBuilder.AlterColumn<long>(
                name: "RelatedDialogElementInternalId",
                table: "DialogElement",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "RelatedActivityInternalId",
                table: "DialogActivity",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "DialogElementInternalId",
                table: "DialogActivity",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

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
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DialogElement_DialogElement_RelatedDialogElementInternalId",
                table: "DialogElement",
                column: "RelatedDialogElementInternalId",
                principalTable: "DialogElement",
                principalColumn: "InternalId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

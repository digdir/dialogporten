using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Digdir.Domain.Dialogporten.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ChangeLocalizationSetToDependentEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@$"DELETE FROM ""LocalizationSet""");

            migrationBuilder.DropForeignKey(
                name: "FK_Dialog_LocalizationSet_BodyId",
                table: "Dialog");

            migrationBuilder.DropForeignKey(
                name: "FK_Dialog_LocalizationSet_SearchTitleId",
                table: "Dialog");

            migrationBuilder.DropForeignKey(
                name: "FK_Dialog_LocalizationSet_SenderNameId",
                table: "Dialog");

            migrationBuilder.DropForeignKey(
                name: "FK_Dialog_LocalizationSet_TitleId",
                table: "Dialog");

            migrationBuilder.DropForeignKey(
                name: "FK_DialogActivity_LocalizationSet_DescriptionId",
                table: "DialogActivity");

            migrationBuilder.DropForeignKey(
                name: "FK_DialogActivity_LocalizationSet_PerformedById",
                table: "DialogActivity");

            migrationBuilder.DropForeignKey(
                name: "FK_DialogElement_LocalizationSet_DisplayNameId",
                table: "DialogElement");

            migrationBuilder.DropForeignKey(
                name: "FK_DialogGuiAction_LocalizationSet_TitleId",
                table: "DialogGuiAction");

            migrationBuilder.DropIndex(
                name: "IX_DialogGuiAction_TitleId",
                table: "DialogGuiAction");

            migrationBuilder.DropIndex(
                name: "IX_DialogElement_DisplayNameId",
                table: "DialogElement");

            migrationBuilder.DropIndex(
                name: "IX_DialogActivity_DescriptionId",
                table: "DialogActivity");

            migrationBuilder.DropIndex(
                name: "IX_DialogActivity_PerformedById",
                table: "DialogActivity");

            migrationBuilder.DropIndex(
                name: "IX_Dialog_BodyId",
                table: "Dialog");

            migrationBuilder.DropIndex(
                name: "IX_Dialog_SearchTitleId",
                table: "Dialog");

            migrationBuilder.DropIndex(
                name: "IX_Dialog_SenderNameId",
                table: "Dialog");

            migrationBuilder.DropIndex(
                name: "IX_Dialog_TitleId",
                table: "Dialog");

            migrationBuilder.DropColumn(
                name: "TitleId",
                table: "DialogGuiAction");

            migrationBuilder.DropColumn(
                name: "DisplayNameId",
                table: "DialogElement");

            migrationBuilder.DropColumn(
                name: "DescriptionId",
                table: "DialogActivity");

            migrationBuilder.DropColumn(
                name: "PerformedById",
                table: "DialogActivity");

            migrationBuilder.DropColumn(
                name: "BodyId",
                table: "Dialog");

            migrationBuilder.DropColumn(
                name: "SearchTitleId",
                table: "Dialog");

            migrationBuilder.DropColumn(
                name: "SenderNameId",
                table: "Dialog");

            migrationBuilder.DropColumn(
                name: "TitleId",
                table: "Dialog");

            migrationBuilder.AddColumn<Guid>(
                name: "ActivityId",
                table: "LocalizationSet",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DialogActivityDescription_ActivityId",
                table: "LocalizationSet",
                type: "uuid",
                nullable: true);

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

            migrationBuilder.AddColumn<Guid>(
                name: "DialogSearchTitle_DialogId",
                table: "LocalizationSet",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DialogSenderName_DialogId",
                table: "LocalizationSet",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "LocalizationSet",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "ElementId",
                table: "LocalizationSet",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "GuiActionId",
                table: "LocalizationSet",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_LocalizationSet_ActivityId",
                table: "LocalizationSet",
                column: "ActivityId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LocalizationSet_DialogActivityDescription_ActivityId",
                table: "LocalizationSet",
                column: "DialogActivityDescription_ActivityId",
                unique: true);

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

            migrationBuilder.CreateIndex(
                name: "IX_LocalizationSet_DialogSearchTitle_DialogId",
                table: "LocalizationSet",
                column: "DialogSearchTitle_DialogId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LocalizationSet_DialogSenderName_DialogId",
                table: "LocalizationSet",
                column: "DialogSenderName_DialogId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LocalizationSet_ElementId",
                table: "LocalizationSet",
                column: "ElementId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LocalizationSet_GuiActionId",
                table: "LocalizationSet",
                column: "GuiActionId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_LocalizationSet_DialogActivity_ActivityId",
                table: "LocalizationSet",
                column: "ActivityId",
                principalTable: "DialogActivity",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LocalizationSet_DialogActivity_DialogActivityDescription_Ac~",
                table: "LocalizationSet",
                column: "DialogActivityDescription_ActivityId",
                principalTable: "DialogActivity",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LocalizationSet_DialogElement_ElementId",
                table: "LocalizationSet",
                column: "ElementId",
                principalTable: "DialogElement",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LocalizationSet_DialogGuiAction_GuiActionId",
                table: "LocalizationSet",
                column: "GuiActionId",
                principalTable: "DialogGuiAction",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

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
                name: "FK_LocalizationSet_Dialog_DialogSearchTitle_DialogId",
                table: "LocalizationSet",
                column: "DialogSearchTitle_DialogId",
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LocalizationSet_DialogActivity_ActivityId",
                table: "LocalizationSet");

            migrationBuilder.DropForeignKey(
                name: "FK_LocalizationSet_DialogActivity_DialogActivityDescription_Ac~",
                table: "LocalizationSet");

            migrationBuilder.DropForeignKey(
                name: "FK_LocalizationSet_DialogElement_ElementId",
                table: "LocalizationSet");

            migrationBuilder.DropForeignKey(
                name: "FK_LocalizationSet_DialogGuiAction_GuiActionId",
                table: "LocalizationSet");

            migrationBuilder.DropForeignKey(
                name: "FK_LocalizationSet_Dialog_DialogBody_DialogId",
                table: "LocalizationSet");

            migrationBuilder.DropForeignKey(
                name: "FK_LocalizationSet_Dialog_DialogId",
                table: "LocalizationSet");

            migrationBuilder.DropForeignKey(
                name: "FK_LocalizationSet_Dialog_DialogSearchTitle_DialogId",
                table: "LocalizationSet");

            migrationBuilder.DropForeignKey(
                name: "FK_LocalizationSet_Dialog_DialogSenderName_DialogId",
                table: "LocalizationSet");

            migrationBuilder.DropIndex(
                name: "IX_LocalizationSet_ActivityId",
                table: "LocalizationSet");

            migrationBuilder.DropIndex(
                name: "IX_LocalizationSet_DialogActivityDescription_ActivityId",
                table: "LocalizationSet");

            migrationBuilder.DropIndex(
                name: "IX_LocalizationSet_DialogBody_DialogId",
                table: "LocalizationSet");

            migrationBuilder.DropIndex(
                name: "IX_LocalizationSet_DialogId",
                table: "LocalizationSet");

            migrationBuilder.DropIndex(
                name: "IX_LocalizationSet_DialogSearchTitle_DialogId",
                table: "LocalizationSet");

            migrationBuilder.DropIndex(
                name: "IX_LocalizationSet_DialogSenderName_DialogId",
                table: "LocalizationSet");

            migrationBuilder.DropIndex(
                name: "IX_LocalizationSet_ElementId",
                table: "LocalizationSet");

            migrationBuilder.DropIndex(
                name: "IX_LocalizationSet_GuiActionId",
                table: "LocalizationSet");

            migrationBuilder.DropColumn(
                name: "ActivityId",
                table: "LocalizationSet");

            migrationBuilder.DropColumn(
                name: "DialogActivityDescription_ActivityId",
                table: "LocalizationSet");

            migrationBuilder.DropColumn(
                name: "DialogBody_DialogId",
                table: "LocalizationSet");

            migrationBuilder.DropColumn(
                name: "DialogId",
                table: "LocalizationSet");

            migrationBuilder.DropColumn(
                name: "DialogSearchTitle_DialogId",
                table: "LocalizationSet");

            migrationBuilder.DropColumn(
                name: "DialogSenderName_DialogId",
                table: "LocalizationSet");

            migrationBuilder.DropColumn(
                name: "Discriminator",
                table: "LocalizationSet");

            migrationBuilder.DropColumn(
                name: "ElementId",
                table: "LocalizationSet");

            migrationBuilder.DropColumn(
                name: "GuiActionId",
                table: "LocalizationSet");

            migrationBuilder.AddColumn<Guid>(
                name: "TitleId",
                table: "DialogGuiAction",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "DisplayNameId",
                table: "DialogElement",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "DescriptionId",
                table: "DialogActivity",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "PerformedById",
                table: "DialogActivity",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "BodyId",
                table: "Dialog",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "SearchTitleId",
                table: "Dialog",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "SenderNameId",
                table: "Dialog",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TitleId",
                table: "Dialog",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_DialogGuiAction_TitleId",
                table: "DialogGuiAction",
                column: "TitleId");

            migrationBuilder.CreateIndex(
                name: "IX_DialogElement_DisplayNameId",
                table: "DialogElement",
                column: "DisplayNameId");

            migrationBuilder.CreateIndex(
                name: "IX_DialogActivity_DescriptionId",
                table: "DialogActivity",
                column: "DescriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_DialogActivity_PerformedById",
                table: "DialogActivity",
                column: "PerformedById");

            migrationBuilder.CreateIndex(
                name: "IX_Dialog_BodyId",
                table: "Dialog",
                column: "BodyId");

            migrationBuilder.CreateIndex(
                name: "IX_Dialog_SearchTitleId",
                table: "Dialog",
                column: "SearchTitleId");

            migrationBuilder.CreateIndex(
                name: "IX_Dialog_SenderNameId",
                table: "Dialog",
                column: "SenderNameId");

            migrationBuilder.CreateIndex(
                name: "IX_Dialog_TitleId",
                table: "Dialog",
                column: "TitleId");

            migrationBuilder.AddForeignKey(
                name: "FK_Dialog_LocalizationSet_BodyId",
                table: "Dialog",
                column: "BodyId",
                principalTable: "LocalizationSet",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Dialog_LocalizationSet_SearchTitleId",
                table: "Dialog",
                column: "SearchTitleId",
                principalTable: "LocalizationSet",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Dialog_LocalizationSet_SenderNameId",
                table: "Dialog",
                column: "SenderNameId",
                principalTable: "LocalizationSet",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Dialog_LocalizationSet_TitleId",
                table: "Dialog",
                column: "TitleId",
                principalTable: "LocalizationSet",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DialogActivity_LocalizationSet_DescriptionId",
                table: "DialogActivity",
                column: "DescriptionId",
                principalTable: "LocalizationSet",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DialogActivity_LocalizationSet_PerformedById",
                table: "DialogActivity",
                column: "PerformedById",
                principalTable: "LocalizationSet",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DialogElement_LocalizationSet_DisplayNameId",
                table: "DialogElement",
                column: "DisplayNameId",
                principalTable: "LocalizationSet",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DialogGuiAction_LocalizationSet_TitleId",
                table: "DialogGuiAction",
                column: "TitleId",
                principalTable: "LocalizationSet",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Digdir.Domain.Dialogporten.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPromptAndHttpMethodToGuiAction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "DialogGuiActionPrompt_GuiActionId",
                table: "LocalizationSet",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "HttpMethodId",
                table: "DialogGuiAction",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_LocalizationSet_DialogGuiActionPrompt_GuiActionId",
                table: "LocalizationSet",
                column: "DialogGuiActionPrompt_GuiActionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DialogGuiAction_HttpMethodId",
                table: "DialogGuiAction",
                column: "HttpMethodId");

            migrationBuilder.AddForeignKey(
                name: "FK_DialogGuiAction_HttpVerb_HttpMethodId",
                table: "DialogGuiAction",
                column: "HttpMethodId",
                principalTable: "HttpVerb",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_LocalizationSet_DialogGuiAction_DialogGuiActionPrompt_GuiAc~",
                table: "LocalizationSet",
                column: "DialogGuiActionPrompt_GuiActionId",
                principalTable: "DialogGuiAction",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DialogGuiAction_HttpVerb_HttpMethodId",
                table: "DialogGuiAction");

            migrationBuilder.DropForeignKey(
                name: "FK_LocalizationSet_DialogGuiAction_DialogGuiActionPrompt_GuiAc~",
                table: "LocalizationSet");

            migrationBuilder.DropIndex(
                name: "IX_LocalizationSet_DialogGuiActionPrompt_GuiActionId",
                table: "LocalizationSet");

            migrationBuilder.DropIndex(
                name: "IX_DialogGuiAction_HttpMethodId",
                table: "DialogGuiAction");

            migrationBuilder.DropColumn(
                name: "DialogGuiActionPrompt_GuiActionId",
                table: "LocalizationSet");

            migrationBuilder.DropColumn(
                name: "HttpMethodId",
                table: "DialogGuiAction");
        }
    }
}

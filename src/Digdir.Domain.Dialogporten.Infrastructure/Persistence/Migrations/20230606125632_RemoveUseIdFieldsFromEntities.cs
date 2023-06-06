using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Digdir.Domain.Dialogporten.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUseIdFieldsFromEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "LocalizationSet");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Localization");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "Localization");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "DialogueTokenScope");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "DialogueGuiAction");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "DialogueGuiAction");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "DialogueAttachement");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "DialogueAttachement");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "DialogueApiAction");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "DialogueApiAction");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "DialogueActivity");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Dialogue");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "Dialogue");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                table: "LocalizationSet",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                table: "Localization",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByUserId",
                table: "Localization",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                table: "DialogueTokenScope",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                table: "DialogueGuiAction",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByUserId",
                table: "DialogueGuiAction",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                table: "DialogueAttachement",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByUserId",
                table: "DialogueAttachement",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                table: "DialogueApiAction",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByUserId",
                table: "DialogueApiAction",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                table: "DialogueActivity",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                table: "Dialogue",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByUserId",
                table: "Dialogue",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }
    }
}

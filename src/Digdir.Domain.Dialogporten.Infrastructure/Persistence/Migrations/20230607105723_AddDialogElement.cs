using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Digdir.Domain.Dialogporten.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDialogElement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContentType",
                table: "DialogElement");

            migrationBuilder.DropColumn(
                name: "DetailsApiUrl",
                table: "DialogActivity");

            migrationBuilder.DropColumn(
                name: "DetailsGuiUrl",
                table: "DialogActivity");

            migrationBuilder.RenameColumn(
                name: "Resource",
                table: "DialogGuiAction",
                newName: "AuthorizationAttribute");

            migrationBuilder.RenameColumn(
                name: "SizeInBytes",
                table: "DialogElement",
                newName: "RelatedDialogElementInternalId");

            migrationBuilder.RenameColumn(
                name: "Resource",
                table: "DialogElement",
                newName: "AuthorizationAttribute");

            migrationBuilder.RenameColumn(
                name: "Resource",
                table: "DialogApiAction",
                newName: "AuthorizationAttribute");

            migrationBuilder.RenameColumn(
                name: "ServiceResourceIdentifier",
                table: "Dialog",
                newName: "ServiceResource");

            migrationBuilder.AddColumn<Guid>(
                name: "RelatedDialogElementId",
                table: "DialogElement",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RelatedDialogElementId",
                table: "DialogActivity",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "RelatedDialogElementInternalId",
                table: "DialogActivity",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateTable(
                name: "DialogElementUrl",
                columns: table => new
                {
                    InternalId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "current_timestamp at time zone 'utc'"),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "current_timestamp at time zone 'utc'"),
                    ConsumerTypeId = table.Column<int>(type: "integer", nullable: false),
                    Url = table.Column<string>(type: "character varying(1023)", maxLength: 1023, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    ContentSchema = table.Column<string>(type: "character varying(1023)", maxLength: 1023, nullable: true),
                    DialogElementInternalId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DialogElementUrl", x => x.InternalId);
                    table.ForeignKey(
                        name: "FK_DialogElementUrl_DialogElement_DialogElementInternalId",
                        column: x => x.DialogElementInternalId,
                        principalTable: "DialogElement",
                        principalColumn: "InternalId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_DialogElement_RelatedDialogElementInternalId",
                table: "DialogElement",
                column: "RelatedDialogElementInternalId");

            migrationBuilder.CreateIndex(
                name: "IX_DialogActivity_RelatedDialogElementInternalId",
                table: "DialogActivity",
                column: "RelatedDialogElementInternalId");

            migrationBuilder.CreateIndex(
                name: "IX_DialogElementUrl_DialogElementInternalId",
                table: "DialogElementUrl",
                column: "DialogElementInternalId");

            migrationBuilder.CreateIndex(
                name: "IX_DialogElementUrl_Id",
                table: "DialogElementUrl",
                column: "Id",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_DialogActivity_DialogElement_RelatedDialogElementInternalId",
                table: "DialogActivity",
                column: "RelatedDialogElementInternalId",
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DialogActivity_DialogElement_RelatedDialogElementInternalId",
                table: "DialogActivity");

            migrationBuilder.DropForeignKey(
                name: "FK_DialogElement_DialogElement_RelatedDialogElementInternalId",
                table: "DialogElement");

            migrationBuilder.DropTable(
                name: "DialogElementUrl");

            migrationBuilder.DropIndex(
                name: "IX_DialogElement_RelatedDialogElementInternalId",
                table: "DialogElement");

            migrationBuilder.DropIndex(
                name: "IX_DialogActivity_RelatedDialogElementInternalId",
                table: "DialogActivity");

            migrationBuilder.DropColumn(
                name: "RelatedDialogElementId",
                table: "DialogElement");

            migrationBuilder.DropColumn(
                name: "RelatedDialogElementId",
                table: "DialogActivity");

            migrationBuilder.DropColumn(
                name: "RelatedDialogElementInternalId",
                table: "DialogActivity");

            migrationBuilder.RenameColumn(
                name: "AuthorizationAttribute",
                table: "DialogGuiAction",
                newName: "Resource");

            migrationBuilder.RenameColumn(
                name: "RelatedDialogElementInternalId",
                table: "DialogElement",
                newName: "SizeInBytes");

            migrationBuilder.RenameColumn(
                name: "AuthorizationAttribute",
                table: "DialogElement",
                newName: "Resource");

            migrationBuilder.RenameColumn(
                name: "AuthorizationAttribute",
                table: "DialogApiAction",
                newName: "Resource");

            migrationBuilder.RenameColumn(
                name: "ServiceResource",
                table: "Dialog",
                newName: "ServiceResourceIdentifier");

            migrationBuilder.AddColumn<string>(
                name: "ContentType",
                table: "DialogElement",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DetailsApiUrl",
                table: "DialogActivity",
                type: "character varying(1023)",
                maxLength: 1023,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DetailsGuiUrl",
                table: "DialogActivity",
                type: "character varying(1023)",
                maxLength: 1023,
                nullable: true);
        }
    }
}

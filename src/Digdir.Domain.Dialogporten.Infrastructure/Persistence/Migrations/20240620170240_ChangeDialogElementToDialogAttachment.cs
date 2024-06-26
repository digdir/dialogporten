using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Digdir.Domain.Dialogporten.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ChangeDialogElementToDialogAttachment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DialogActivity_DialogElement_DialogElementId",
                table: "DialogActivity");

            migrationBuilder.DropForeignKey(
                name: "FK_DialogApiAction_DialogElement_DialogElementId",
                table: "DialogApiAction");

            migrationBuilder.DropForeignKey(
                name: "FK_LocalizationSet_DialogElement_ElementId",
                table: "LocalizationSet");

            migrationBuilder.DropTable(
                name: "DialogElementUrl");

            migrationBuilder.DropTable(
                name: "DialogElementUrlConsumerType");

            migrationBuilder.DropTable(
                name: "DialogElement");

            migrationBuilder.DropIndex(
                name: "IX_DialogApiAction_DialogElementId",
                table: "DialogApiAction");

            migrationBuilder.DropIndex(
                name: "IX_DialogActivity_DialogElementId",
                table: "DialogActivity");

            migrationBuilder.DropIndex(
                name: "IX_LocalizationSet_ElementId",
                table: "LocalizationSet");

            migrationBuilder.DropColumn(
                name: "DialogElementId",
                table: "DialogApiAction");

            migrationBuilder.DropColumn(
                name: "DialogElementId",
                table: "DialogActivity");

            migrationBuilder.DropColumn(
                name: "ElementId",
                table: "LocalizationSet");

            migrationBuilder.AddColumn<Guid>(
                name: "AttachmentId",
                table: "LocalizationSet",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_LocalizationSet_AttachmentId",
                table: "LocalizationSet",
                column: "AttachmentId",
                unique: true);

            migrationBuilder.CreateTable(
                name: "DialogAttachment",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "current_timestamp at time zone 'utc'"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "current_timestamp at time zone 'utc'"),
                    DialogId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DialogAttachment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DialogAttachment_Dialog_DialogId",
                        column: x => x.DialogId,
                        principalTable: "Dialog",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DialogAttachmentUrlConsumerType",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DialogAttachmentUrlConsumerType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DialogAttachmentUrl",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "current_timestamp at time zone 'utc'"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "current_timestamp at time zone 'utc'"),
                    MediaType = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Url = table.Column<string>(type: "character varying(1023)", maxLength: 1023, nullable: false),
                    ConsumerTypeId = table.Column<int>(type: "integer", nullable: false),
                    DialogAttachmentId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DialogAttachmentUrl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DialogAttachmentUrl_DialogAttachmentUrlConsumerType_Consume~",
                        column: x => x.ConsumerTypeId,
                        principalTable: "DialogAttachmentUrlConsumerType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DialogAttachmentUrl_DialogAttachment_DialogAttachmentId",
                        column: x => x.DialogAttachmentId,
                        principalTable: "DialogAttachment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "DialogAttachmentUrlConsumerType",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Gui" },
                    { 2, "Api" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_DialogAttachment_DialogId",
                table: "DialogAttachment",
                column: "DialogId");

            migrationBuilder.CreateIndex(
                name: "IX_DialogAttachmentUrl_ConsumerTypeId",
                table: "DialogAttachmentUrl",
                column: "ConsumerTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_DialogAttachmentUrl_DialogAttachmentId",
                table: "DialogAttachmentUrl",
                column: "DialogAttachmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_LocalizationSet_DialogAttachment_AttachmentId",
                table: "LocalizationSet",
                column: "AttachmentId",
                principalTable: "DialogAttachment",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LocalizationSet_DialogAttachment_AttachmentId",
                table: "LocalizationSet");

            migrationBuilder.DropTable(
                name: "DialogAttachmentUrl");

            migrationBuilder.DropTable(
                name: "DialogAttachmentUrlConsumerType");

            migrationBuilder.DropTable(
                name: "DialogAttachment");

            migrationBuilder.RenameColumn(
                name: "AttachmentId",
                table: "LocalizationSet",
                newName: "ElementId");

            migrationBuilder.RenameIndex(
                name: "IX_LocalizationSet_AttachmentId",
                table: "LocalizationSet",
                newName: "IX_LocalizationSet_ElementId");

            migrationBuilder.AddColumn<Guid>(
                name: "DialogElementId",
                table: "DialogApiAction",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DialogElementId",
                table: "DialogActivity",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DialogElement",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    DialogId = table.Column<Guid>(type: "uuid", nullable: false),
                    RelatedDialogElementId = table.Column<Guid>(type: "uuid", nullable: true),
                    AuthorizationAttribute = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "current_timestamp at time zone 'utc'"),
                    ExternalReference = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Type = table.Column<string>(type: "character varying(1023)", maxLength: 1023, nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "current_timestamp at time zone 'utc'")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DialogElement", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DialogElement_DialogElement_RelatedDialogElementId",
                        column: x => x.RelatedDialogElementId,
                        principalTable: "DialogElement",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_DialogElement_Dialog_DialogId",
                        column: x => x.DialogId,
                        principalTable: "Dialog",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DialogElementUrlConsumerType",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DialogElementUrlConsumerType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DialogElementUrl",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    ConsumerTypeId = table.Column<int>(type: "integer", nullable: false),
                    DialogElementId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "current_timestamp at time zone 'utc'"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "current_timestamp at time zone 'utc'"),
                    Url = table.Column<string>(type: "character varying(1023)", maxLength: 1023, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DialogElementUrl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DialogElementUrl_DialogElementUrlConsumerType_ConsumerTypeId",
                        column: x => x.ConsumerTypeId,
                        principalTable: "DialogElementUrlConsumerType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DialogElementUrl_DialogElement_DialogElementId",
                        column: x => x.DialogElementId,
                        principalTable: "DialogElement",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "DialogElementUrlConsumerType",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Gui" },
                    { 2, "Api" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_DialogApiAction_DialogElementId",
                table: "DialogApiAction",
                column: "DialogElementId");

            migrationBuilder.CreateIndex(
                name: "IX_DialogActivity_DialogElementId",
                table: "DialogActivity",
                column: "DialogElementId");

            migrationBuilder.CreateIndex(
                name: "IX_DialogElement_DialogId",
                table: "DialogElement",
                column: "DialogId");

            migrationBuilder.CreateIndex(
                name: "IX_DialogElement_RelatedDialogElementId",
                table: "DialogElement",
                column: "RelatedDialogElementId");

            migrationBuilder.CreateIndex(
                name: "IX_DialogElementUrl_ConsumerTypeId",
                table: "DialogElementUrl",
                column: "ConsumerTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_DialogElementUrl_DialogElementId",
                table: "DialogElementUrl",
                column: "DialogElementId");

            migrationBuilder.AddForeignKey(
                name: "FK_DialogActivity_DialogElement_DialogElementId",
                table: "DialogActivity",
                column: "DialogElementId",
                principalTable: "DialogElement",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_DialogApiAction_DialogElement_DialogElementId",
                table: "DialogApiAction",
                column: "DialogElementId",
                principalTable: "DialogElement",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_LocalizationSet_DialogElement_ElementId",
                table: "LocalizationSet",
                column: "ElementId",
                principalTable: "DialogElement",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

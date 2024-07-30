using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Digdir.Domain.Dialogporten.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTransmissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DialogAttachment_Dialog_DialogId",
                table: "DialogAttachment");

            migrationBuilder.DropForeignKey(
                name: "FK_LocalizationSet_DialogAttachment_AttachmentId",
                table: "LocalizationSet");

            migrationBuilder.DropTable(
                name: "DialogAttachmentUrl");

            migrationBuilder.DropTable(
                name: "DialogAttachmentUrlConsumerType");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DialogAttachment",
                table: "DialogAttachment");

            migrationBuilder.RenameTable(
                name: "DialogAttachment",
                newName: "Attachment");

            migrationBuilder.RenameIndex(
                name: "IX_DialogAttachment_DialogId",
                table: "Attachment",
                newName: "IX_Attachment_DialogId");

            migrationBuilder.AddColumn<Guid>(
                name: "TransmissionContentId",
                table: "LocalizationSet",
                type: "uuid",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "DialogId",
                table: "Attachment",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "Attachment",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "TransmissionId",
                table: "Attachment",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Attachment",
                table: "Attachment",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "AttachmentUrlConsumerType",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttachmentUrlConsumerType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DialogTransmissionType",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DialogTransmissionType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TransmissionContentType",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Required = table.Column<bool>(type: "boolean", nullable: false),
                    MaxLength = table.Column<int>(type: "integer", nullable: false),
                    AllowedMediaTypes = table.Column<string[]>(type: "text[]", nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransmissionContentType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AttachmentUrl",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "current_timestamp at time zone 'utc'"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "current_timestamp at time zone 'utc'"),
                    MediaType = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Url = table.Column<string>(type: "character varying(1023)", maxLength: 1023, nullable: false),
                    ConsumerTypeId = table.Column<int>(type: "integer", nullable: false),
                    AttachmentId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttachmentUrl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AttachmentUrl_AttachmentUrlConsumerType_ConsumerTypeId",
                        column: x => x.ConsumerTypeId,
                        principalTable: "AttachmentUrlConsumerType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AttachmentUrl_Attachment_AttachmentId",
                        column: x => x.AttachmentId,
                        principalTable: "Attachment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DialogTransmission",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "current_timestamp at time zone 'utc'"),
                    AuthorizationAttribute = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    SenderId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExtendedType = table.Column<string>(type: "character varying(1023)", maxLength: 1023, nullable: true),
                    TypeId = table.Column<int>(type: "integer", nullable: false),
                    DialogId = table.Column<Guid>(type: "uuid", nullable: false),
                    RelatedTransmissionId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DialogTransmission", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DialogTransmission_DialogActor_SenderId",
                        column: x => x.SenderId,
                        principalTable: "DialogActor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DialogTransmission_DialogTransmissionType_TypeId",
                        column: x => x.TypeId,
                        principalTable: "DialogTransmissionType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DialogTransmission_DialogTransmission_RelatedTransmissionId",
                        column: x => x.RelatedTransmissionId,
                        principalTable: "DialogTransmission",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DialogTransmission_Dialog_DialogId",
                        column: x => x.DialogId,
                        principalTable: "Dialog",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TransmissionContent",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "current_timestamp at time zone 'utc'"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "current_timestamp at time zone 'utc'"),
                    MediaType = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    TransmissionId = table.Column<Guid>(type: "uuid", nullable: false),
                    TypeId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransmissionContent", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TransmissionContent_DialogTransmission_TransmissionId",
                        column: x => x.TransmissionId,
                        principalTable: "DialogTransmission",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TransmissionContent_TransmissionContentType_TypeId",
                        column: x => x.TypeId,
                        principalTable: "TransmissionContentType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "AttachmentUrlConsumerType",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Gui" },
                    { 2, "Api" }
                });

            migrationBuilder.InsertData(
                table: "DialogTransmissionType",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Information" },
                    { 2, "Acceptance" },
                    { 3, "Rejection" },
                    { 4, "Request" },
                    { 5, "Alert" },
                    { 6, "Decision" },
                    { 7, "Submission" },
                    { 8, "Correction" }
                });

            migrationBuilder.InsertData(
                table: "TransmissionContentType",
                columns: new[] { "Id", "AllowedMediaTypes", "MaxLength", "Name", "Required" },
                values: new object[,]
                {
                    { 1, new[] { "text/plain" }, 255, "Title", true },
                    { 2, new[] { "text/plain" }, 255, "Summary", true }
                });

            migrationBuilder.CreateIndex(
                name: "IX_LocalizationSet_TransmissionContentId",
                table: "LocalizationSet",
                column: "TransmissionContentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Attachment_TransmissionId",
                table: "Attachment",
                column: "TransmissionId");

            migrationBuilder.CreateIndex(
                name: "IX_AttachmentUrl_AttachmentId",
                table: "AttachmentUrl",
                column: "AttachmentId");

            migrationBuilder.CreateIndex(
                name: "IX_AttachmentUrl_ConsumerTypeId",
                table: "AttachmentUrl",
                column: "ConsumerTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_DialogTransmission_DialogId",
                table: "DialogTransmission",
                column: "DialogId");

            migrationBuilder.CreateIndex(
                name: "IX_DialogTransmission_RelatedTransmissionId",
                table: "DialogTransmission",
                column: "RelatedTransmissionId");

            migrationBuilder.CreateIndex(
                name: "IX_DialogTransmission_SenderId",
                table: "DialogTransmission",
                column: "SenderId");

            migrationBuilder.CreateIndex(
                name: "IX_DialogTransmission_TypeId",
                table: "DialogTransmission",
                column: "TypeId");

            migrationBuilder.CreateIndex(
                name: "IX_TransmissionContent_TransmissionId_TypeId",
                table: "TransmissionContent",
                columns: new[] { "TransmissionId", "TypeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TransmissionContent_TypeId",
                table: "TransmissionContent",
                column: "TypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Attachment_DialogTransmission_TransmissionId",
                table: "Attachment",
                column: "TransmissionId",
                principalTable: "DialogTransmission",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Attachment_Dialog_DialogId",
                table: "Attachment",
                column: "DialogId",
                principalTable: "Dialog",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LocalizationSet_Attachment_AttachmentId",
                table: "LocalizationSet",
                column: "AttachmentId",
                principalTable: "Attachment",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LocalizationSet_TransmissionContent_TransmissionContentId",
                table: "LocalizationSet",
                column: "TransmissionContentId",
                principalTable: "TransmissionContent",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Attachment_DialogTransmission_TransmissionId",
                table: "Attachment");

            migrationBuilder.DropForeignKey(
                name: "FK_Attachment_Dialog_DialogId",
                table: "Attachment");

            migrationBuilder.DropForeignKey(
                name: "FK_LocalizationSet_Attachment_AttachmentId",
                table: "LocalizationSet");

            migrationBuilder.DropForeignKey(
                name: "FK_LocalizationSet_TransmissionContent_TransmissionContentId",
                table: "LocalizationSet");

            migrationBuilder.DropTable(
                name: "AttachmentUrl");

            migrationBuilder.DropTable(
                name: "TransmissionContent");

            migrationBuilder.DropTable(
                name: "AttachmentUrlConsumerType");

            migrationBuilder.DropTable(
                name: "DialogTransmission");

            migrationBuilder.DropTable(
                name: "TransmissionContentType");

            migrationBuilder.DropTable(
                name: "DialogTransmissionType");

            migrationBuilder.DropIndex(
                name: "IX_LocalizationSet_TransmissionContentId",
                table: "LocalizationSet");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Attachment",
                table: "Attachment");

            migrationBuilder.DropIndex(
                name: "IX_Attachment_TransmissionId",
                table: "Attachment");

            migrationBuilder.DropColumn(
                name: "TransmissionContentId",
                table: "LocalizationSet");

            migrationBuilder.DropColumn(
                name: "Discriminator",
                table: "Attachment");

            migrationBuilder.DropColumn(
                name: "TransmissionId",
                table: "Attachment");

            migrationBuilder.RenameTable(
                name: "Attachment",
                newName: "DialogAttachment");

            migrationBuilder.RenameIndex(
                name: "IX_Attachment_DialogId",
                table: "DialogAttachment",
                newName: "IX_DialogAttachment_DialogId");

            migrationBuilder.AlterColumn<Guid>(
                name: "DialogId",
                table: "DialogAttachment",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_DialogAttachment",
                table: "DialogAttachment",
                column: "Id");

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
                    ConsumerTypeId = table.Column<int>(type: "integer", nullable: false),
                    DialogAttachmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "current_timestamp at time zone 'utc'"),
                    MediaType = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "current_timestamp at time zone 'utc'"),
                    Url = table.Column<string>(type: "character varying(1023)", maxLength: 1023, nullable: false)
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
                name: "IX_DialogAttachmentUrl_ConsumerTypeId",
                table: "DialogAttachmentUrl",
                column: "ConsumerTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_DialogAttachmentUrl_DialogAttachmentId",
                table: "DialogAttachmentUrl",
                column: "DialogAttachmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_DialogAttachment_Dialog_DialogId",
                table: "DialogAttachment",
                column: "DialogId",
                principalTable: "Dialog",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LocalizationSet_DialogAttachment_AttachmentId",
                table: "LocalizationSet",
                column: "AttachmentId",
                principalTable: "DialogAttachment",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

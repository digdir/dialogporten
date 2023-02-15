using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Digdir.Domain.Dialogporten.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDialogueTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DialogueActivityType",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DialogueActivityType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DialogueGuiActionType",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DialogueGuiActionType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DialogueStatus",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DialogueStatus", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LocalizationSet",
                columns: table => new
                {
                    InternalId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocalizationSet", x => x.InternalId);
                });

            migrationBuilder.CreateTable(
                name: "DialogueGroup",
                columns: table => new
                {
                    InternalId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false),
                    NameInternalId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DialogueGroup", x => x.InternalId);
                    table.ForeignKey(
                        name: "FK_DialogueGroup_LocalizationSet_NameInternalId",
                        column: x => x.NameInternalId,
                        principalTable: "LocalizationSet",
                        principalColumn: "InternalId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Localization",
                columns: table => new
                {
                    InternalId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CultureCode = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    LocalizationSetId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Localization", x => x.InternalId);
                    table.ForeignKey(
                        name: "FK_Localization_LocalizationSet_LocalizationSetId",
                        column: x => x.LocalizationSetId,
                        principalTable: "LocalizationSet",
                        principalColumn: "InternalId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DialogueEntity",
                columns: table => new
                {
                    InternalId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ServiceResourceIdentifier = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Party = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ExternalReference = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ExtendedStatus = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    StatusId = table.Column<int>(type: "int", nullable: false),
                    DialogueGroupId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DialogueEntity", x => x.InternalId);
                    table.ForeignKey(
                        name: "FK_DialogueEntity_DialogueGroup_DialogueGroupId",
                        column: x => x.DialogueGroupId,
                        principalTable: "DialogueGroup",
                        principalColumn: "InternalId");
                    table.ForeignKey(
                        name: "FK_DialogueEntity_DialogueStatus_StatusId",
                        column: x => x.StatusId,
                        principalTable: "DialogueStatus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DialogueActivity",
                columns: table => new
                {
                    InternalId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TypeId = table.Column<int>(type: "int", nullable: false),
                    PerformedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ActivityExtendedType = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    DescriptionInternalId = table.Column<long>(type: "bigint", nullable: false),
                    DetailsApiUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DetailsGuiUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DialogueId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DialogueActivity", x => x.InternalId);
                    table.ForeignKey(
                        name: "FK_DialogueActivity_DialogueActivityType_TypeId",
                        column: x => x.TypeId,
                        principalTable: "DialogueActivityType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DialogueActivity_DialogueEntity_DialogueId",
                        column: x => x.DialogueId,
                        principalTable: "DialogueEntity",
                        principalColumn: "InternalId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DialogueActivity_LocalizationSet_DescriptionInternalId",
                        column: x => x.DescriptionInternalId,
                        principalTable: "LocalizationSet",
                        principalColumn: "InternalId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DialogueApiAction",
                columns: table => new
                {
                    InternalId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HttpMethod = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Resource = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    IsBackChannel = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleteAction = table.Column<bool>(type: "bit", nullable: false),
                    DocumentationUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RequestSchema = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ResponseSchema = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DialogueId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DialogueApiAction", x => x.InternalId);
                    table.ForeignKey(
                        name: "FK_DialogueApiAction_DialogueEntity_DialogueId",
                        column: x => x.DialogueId,
                        principalTable: "DialogueEntity",
                        principalColumn: "InternalId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DialogueAttachement",
                columns: table => new
                {
                    InternalId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DisplayNameInternalId = table.Column<long>(type: "bigint", nullable: false),
                    SizeInBytes = table.Column<long>(type: "bigint", nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Resource = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    DialogueId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DialogueAttachement", x => x.InternalId);
                    table.ForeignKey(
                        name: "FK_DialogueAttachement_DialogueEntity_DialogueId",
                        column: x => x.DialogueId,
                        principalTable: "DialogueEntity",
                        principalColumn: "InternalId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DialogueAttachement_LocalizationSet_DisplayNameInternalId",
                        column: x => x.DisplayNameInternalId,
                        principalTable: "LocalizationSet",
                        principalColumn: "InternalId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DialogueConfiguration",
                columns: table => new
                {
                    InternalId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VisibleFrom = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DialogueId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DialogueConfiguration", x => x.InternalId);
                    table.ForeignKey(
                        name: "FK_DialogueConfiguration_DialogueEntity_DialogueId",
                        column: x => x.DialogueId,
                        principalTable: "DialogueEntity",
                        principalColumn: "InternalId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DialogueContent",
                columns: table => new
                {
                    InternalId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DialogueId = table.Column<long>(type: "bigint", nullable: false),
                    BodyInternalId = table.Column<long>(type: "bigint", nullable: false),
                    TitleInternalId = table.Column<long>(type: "bigint", nullable: false),
                    SenderNameInternalId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DialogueContent", x => x.InternalId);
                    table.ForeignKey(
                        name: "FK_DialogueContent_DialogueEntity_DialogueId",
                        column: x => x.DialogueId,
                        principalTable: "DialogueEntity",
                        principalColumn: "InternalId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DialogueContent_LocalizationSet_BodyInternalId",
                        column: x => x.BodyInternalId,
                        principalTable: "LocalizationSet",
                        principalColumn: "InternalId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DialogueContent_LocalizationSet_SenderNameInternalId",
                        column: x => x.SenderNameInternalId,
                        principalTable: "LocalizationSet",
                        principalColumn: "InternalId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DialogueContent_LocalizationSet_TitleInternalId",
                        column: x => x.TitleInternalId,
                        principalTable: "LocalizationSet",
                        principalColumn: "InternalId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DialogueDate",
                columns: table => new
                {
                    InternalId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReadDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DialogueId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DialogueDate", x => x.InternalId);
                    table.ForeignKey(
                        name: "FK_DialogueDate_DialogueEntity_DialogueId",
                        column: x => x.DialogueId,
                        principalTable: "DialogueEntity",
                        principalColumn: "InternalId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DialogueGuiAction",
                columns: table => new
                {
                    InternalId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    TypeId = table.Column<int>(type: "int", nullable: false),
                    TitleInternalId = table.Column<long>(type: "bigint", nullable: false),
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Resource = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    IsBackChannel = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleteAction = table.Column<bool>(type: "bit", nullable: false),
                    DialogueId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DialogueGuiAction", x => x.InternalId);
                    table.ForeignKey(
                        name: "FK_DialogueGuiAction_DialogueEntity_DialogueId",
                        column: x => x.DialogueId,
                        principalTable: "DialogueEntity",
                        principalColumn: "InternalId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DialogueGuiAction_DialogueGuiActionType_TypeId",
                        column: x => x.TypeId,
                        principalTable: "DialogueGuiActionType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DialogueGuiAction_LocalizationSet_TitleInternalId",
                        column: x => x.TitleInternalId,
                        principalTable: "LocalizationSet",
                        principalColumn: "InternalId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "DialogueActivityType",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Submission" },
                    { 2, "Feedback" },
                    { 3, "Information" },
                    { 4, "Error" },
                    { 5, "Closed" },
                    { 6, "Seen" },
                    { 7, "Forwarded" }
                });

            migrationBuilder.InsertData(
                table: "DialogueGuiActionType",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Primary" },
                    { 2, "Secondary" },
                    { 3, "Tertiary" }
                });

            migrationBuilder.InsertData(
                table: "DialogueStatus",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Unspecified" },
                    { 2, "UnderProgress" },
                    { 3, "Waiting" },
                    { 4, "Signing" },
                    { 5, "Cancelled" },
                    { 6, "Completed" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_DialogueActivity_DescriptionInternalId",
                table: "DialogueActivity",
                column: "DescriptionInternalId");

            migrationBuilder.CreateIndex(
                name: "IX_DialogueActivity_DialogueId",
                table: "DialogueActivity",
                column: "DialogueId");

            migrationBuilder.CreateIndex(
                name: "IX_DialogueActivity_Id",
                table: "DialogueActivity",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DialogueActivity_TypeId",
                table: "DialogueActivity",
                column: "TypeId");

            migrationBuilder.CreateIndex(
                name: "IX_DialogueApiAction_DialogueId",
                table: "DialogueApiAction",
                column: "DialogueId");

            migrationBuilder.CreateIndex(
                name: "IX_DialogueApiAction_Id",
                table: "DialogueApiAction",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DialogueAttachement_DialogueId",
                table: "DialogueAttachement",
                column: "DialogueId");

            migrationBuilder.CreateIndex(
                name: "IX_DialogueAttachement_DisplayNameInternalId",
                table: "DialogueAttachement",
                column: "DisplayNameInternalId");

            migrationBuilder.CreateIndex(
                name: "IX_DialogueAttachement_Id",
                table: "DialogueAttachement",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DialogueConfiguration_DialogueId",
                table: "DialogueConfiguration",
                column: "DialogueId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DialogueConfiguration_Id",
                table: "DialogueConfiguration",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DialogueContent_BodyInternalId",
                table: "DialogueContent",
                column: "BodyInternalId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DialogueContent_DialogueId",
                table: "DialogueContent",
                column: "DialogueId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DialogueContent_Id",
                table: "DialogueContent",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DialogueContent_SenderNameInternalId",
                table: "DialogueContent",
                column: "SenderNameInternalId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DialogueContent_TitleInternalId",
                table: "DialogueContent",
                column: "TitleInternalId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DialogueDate_DialogueId",
                table: "DialogueDate",
                column: "DialogueId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DialogueDate_Id",
                table: "DialogueDate",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DialogueEntity_DialogueGroupId",
                table: "DialogueEntity",
                column: "DialogueGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_DialogueEntity_Id",
                table: "DialogueEntity",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DialogueEntity_StatusId",
                table: "DialogueEntity",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_DialogueGroup_Id",
                table: "DialogueGroup",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DialogueGroup_NameInternalId",
                table: "DialogueGroup",
                column: "NameInternalId");

            migrationBuilder.CreateIndex(
                name: "IX_DialogueGuiAction_DialogueId",
                table: "DialogueGuiAction",
                column: "DialogueId");

            migrationBuilder.CreateIndex(
                name: "IX_DialogueGuiAction_Id",
                table: "DialogueGuiAction",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DialogueGuiAction_TitleInternalId",
                table: "DialogueGuiAction",
                column: "TitleInternalId");

            migrationBuilder.CreateIndex(
                name: "IX_DialogueGuiAction_TypeId",
                table: "DialogueGuiAction",
                column: "TypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Localization_Id",
                table: "Localization",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Localization_LocalizationSetId",
                table: "Localization",
                column: "LocalizationSetId");

            migrationBuilder.CreateIndex(
                name: "IX_LocalizationSet_Id",
                table: "LocalizationSet",
                column: "Id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DialogueActivity");

            migrationBuilder.DropTable(
                name: "DialogueApiAction");

            migrationBuilder.DropTable(
                name: "DialogueAttachement");

            migrationBuilder.DropTable(
                name: "DialogueConfiguration");

            migrationBuilder.DropTable(
                name: "DialogueContent");

            migrationBuilder.DropTable(
                name: "DialogueDate");

            migrationBuilder.DropTable(
                name: "DialogueGuiAction");

            migrationBuilder.DropTable(
                name: "Localization");

            migrationBuilder.DropTable(
                name: "DialogueActivityType");

            migrationBuilder.DropTable(
                name: "DialogueEntity");

            migrationBuilder.DropTable(
                name: "DialogueGuiActionType");

            migrationBuilder.DropTable(
                name: "DialogueGroup");

            migrationBuilder.DropTable(
                name: "DialogueStatus");

            migrationBuilder.DropTable(
                name: "LocalizationSet");
        }
    }
}

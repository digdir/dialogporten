using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Digdir.Domain.Dialogporten.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RenameDialogueToDialog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DialogueActivity");

            migrationBuilder.DropTable(
                name: "DialogueApiAction");

            migrationBuilder.DropTable(
                name: "DialogueAttachement");

            migrationBuilder.DropTable(
                name: "DialogueGuiAction");

            migrationBuilder.DropTable(
                name: "DialogueTokenScope");

            migrationBuilder.DropTable(
                name: "DialogueActivityType");

            migrationBuilder.DropTable(
                name: "DialogueGuiActionType");

            migrationBuilder.DropTable(
                name: "Dialogue");

            migrationBuilder.DropTable(
                name: "DialogueStatus");

            migrationBuilder.CreateTable(
                name: "DialogActivityType",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DialogActivityType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DialogGuiActionType",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DialogGuiActionType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DialogStatus",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DialogStatus", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Dialog",
                columns: table => new
                {
                    InternalId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "current_timestamp at time zone 'utc'"),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "current_timestamp at time zone 'utc'"),
                    Org = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ServiceResourceIdentifier = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Party = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ExtendedStatus = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    VisibleFromUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DueAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ExpiresAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ReadAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    StatusId = table.Column<int>(type: "integer", nullable: false),
                    BodyId = table.Column<long>(type: "bigint", nullable: false),
                    TitleId = table.Column<long>(type: "bigint", nullable: false),
                    SenderNameId = table.Column<long>(type: "bigint", nullable: false),
                    SearchTitleId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dialog", x => x.InternalId);
                    table.ForeignKey(
                        name: "FK_Dialog_DialogStatus_StatusId",
                        column: x => x.StatusId,
                        principalTable: "DialogStatus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Dialog_LocalizationSet_BodyId",
                        column: x => x.BodyId,
                        principalTable: "LocalizationSet",
                        principalColumn: "InternalId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Dialog_LocalizationSet_SearchTitleId",
                        column: x => x.SearchTitleId,
                        principalTable: "LocalizationSet",
                        principalColumn: "InternalId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Dialog_LocalizationSet_SenderNameId",
                        column: x => x.SenderNameId,
                        principalTable: "LocalizationSet",
                        principalColumn: "InternalId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Dialog_LocalizationSet_TitleId",
                        column: x => x.TitleId,
                        principalTable: "LocalizationSet",
                        principalColumn: "InternalId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DialogActivity",
                columns: table => new
                {
                    InternalId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "current_timestamp at time zone 'utc'"),
                    PerformedBy = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    ExtendedType = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    DescriptionInternalId = table.Column<long>(type: "bigint", nullable: false),
                    DetailsApiUrl = table.Column<string>(type: "character varying(1023)", maxLength: 1023, nullable: true),
                    DetailsGuiUrl = table.Column<string>(type: "character varying(1023)", maxLength: 1023, nullable: true),
                    TypeId = table.Column<int>(type: "integer", nullable: false),
                    DialogId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DialogActivity", x => x.InternalId);
                    table.ForeignKey(
                        name: "FK_DialogActivity_DialogActivityType_TypeId",
                        column: x => x.TypeId,
                        principalTable: "DialogActivityType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DialogActivity_Dialog_DialogId",
                        column: x => x.DialogId,
                        principalTable: "Dialog",
                        principalColumn: "InternalId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DialogActivity_LocalizationSet_DescriptionInternalId",
                        column: x => x.DescriptionInternalId,
                        principalTable: "LocalizationSet",
                        principalColumn: "InternalId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DialogApiAction",
                columns: table => new
                {
                    InternalId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "current_timestamp at time zone 'utc'"),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "current_timestamp at time zone 'utc'"),
                    Action = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Resource = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Url = table.Column<string>(type: "character varying(1023)", maxLength: 1023, nullable: false),
                    HttpMethod = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    DocumentationUrl = table.Column<string>(type: "character varying(1023)", maxLength: 1023, nullable: true),
                    RequestSchema = table.Column<string>(type: "character varying(1023)", maxLength: 1023, nullable: true),
                    ResponseSchema = table.Column<string>(type: "character varying(1023)", maxLength: 1023, nullable: true),
                    DialogId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DialogApiAction", x => x.InternalId);
                    table.ForeignKey(
                        name: "FK_DialogApiAction_Dialog_DialogId",
                        column: x => x.DialogId,
                        principalTable: "Dialog",
                        principalColumn: "InternalId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DialogElement",
                columns: table => new
                {
                    InternalId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "current_timestamp at time zone 'utc'"),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "current_timestamp at time zone 'utc'"),
                    DisplayNameInternalId = table.Column<long>(type: "bigint", nullable: false),
                    SizeInBytes = table.Column<long>(type: "bigint", nullable: false),
                    ContentType = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Url = table.Column<string>(type: "character varying(1023)", maxLength: 1023, nullable: false),
                    Resource = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    DialogId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DialogElement", x => x.InternalId);
                    table.ForeignKey(
                        name: "FK_DialogElement_Dialog_DialogId",
                        column: x => x.DialogId,
                        principalTable: "Dialog",
                        principalColumn: "InternalId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DialogElement_LocalizationSet_DisplayNameInternalId",
                        column: x => x.DisplayNameInternalId,
                        principalTable: "LocalizationSet",
                        principalColumn: "InternalId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DialogGuiAction",
                columns: table => new
                {
                    InternalId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "current_timestamp at time zone 'utc'"),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "current_timestamp at time zone 'utc'"),
                    Action = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    TitleInternalId = table.Column<long>(type: "bigint", nullable: false),
                    Url = table.Column<string>(type: "character varying(1023)", maxLength: 1023, nullable: false),
                    Resource = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    IsBackChannel = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleteAction = table.Column<bool>(type: "boolean", nullable: false),
                    TypeId = table.Column<int>(type: "integer", nullable: false),
                    DialogId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DialogGuiAction", x => x.InternalId);
                    table.ForeignKey(
                        name: "FK_DialogGuiAction_DialogGuiActionType_TypeId",
                        column: x => x.TypeId,
                        principalTable: "DialogGuiActionType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DialogGuiAction_Dialog_DialogId",
                        column: x => x.DialogId,
                        principalTable: "Dialog",
                        principalColumn: "InternalId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DialogGuiAction_LocalizationSet_TitleInternalId",
                        column: x => x.TitleInternalId,
                        principalTable: "LocalizationSet",
                        principalColumn: "InternalId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "DialogActivityType",
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
                table: "DialogGuiActionType",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Primary" },
                    { 2, "Secondary" },
                    { 3, "Tertiary" }
                });

            migrationBuilder.InsertData(
                table: "DialogStatus",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Unspecified" },
                    { 2, "InProgress" },
                    { 3, "Waiting" },
                    { 4, "Signing" },
                    { 5, "Cancelled" },
                    { 6, "Completed" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Dialog_BodyId",
                table: "Dialog",
                column: "BodyId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Dialog_Id",
                table: "Dialog",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Dialog_SearchTitleId",
                table: "Dialog",
                column: "SearchTitleId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Dialog_SenderNameId",
                table: "Dialog",
                column: "SenderNameId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Dialog_StatusId",
                table: "Dialog",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_Dialog_TitleId",
                table: "Dialog",
                column: "TitleId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DialogActivity_DescriptionInternalId",
                table: "DialogActivity",
                column: "DescriptionInternalId");

            migrationBuilder.CreateIndex(
                name: "IX_DialogActivity_DialogId",
                table: "DialogActivity",
                column: "DialogId");

            migrationBuilder.CreateIndex(
                name: "IX_DialogActivity_Id",
                table: "DialogActivity",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DialogActivity_TypeId",
                table: "DialogActivity",
                column: "TypeId");

            migrationBuilder.CreateIndex(
                name: "IX_DialogApiAction_DialogId",
                table: "DialogApiAction",
                column: "DialogId");

            migrationBuilder.CreateIndex(
                name: "IX_DialogApiAction_Id",
                table: "DialogApiAction",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DialogElement_DialogId",
                table: "DialogElement",
                column: "DialogId");

            migrationBuilder.CreateIndex(
                name: "IX_DialogElement_DisplayNameInternalId",
                table: "DialogElement",
                column: "DisplayNameInternalId");

            migrationBuilder.CreateIndex(
                name: "IX_DialogElement_Id",
                table: "DialogElement",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DialogGuiAction_DialogId",
                table: "DialogGuiAction",
                column: "DialogId");

            migrationBuilder.CreateIndex(
                name: "IX_DialogGuiAction_Id",
                table: "DialogGuiAction",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DialogGuiAction_TitleInternalId",
                table: "DialogGuiAction",
                column: "TitleInternalId");

            migrationBuilder.CreateIndex(
                name: "IX_DialogGuiAction_TypeId",
                table: "DialogGuiAction",
                column: "TypeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DialogActivity");

            migrationBuilder.DropTable(
                name: "DialogApiAction");

            migrationBuilder.DropTable(
                name: "DialogElement");

            migrationBuilder.DropTable(
                name: "DialogGuiAction");

            migrationBuilder.DropTable(
                name: "DialogActivityType");

            migrationBuilder.DropTable(
                name: "DialogGuiActionType");

            migrationBuilder.DropTable(
                name: "Dialog");

            migrationBuilder.DropTable(
                name: "DialogStatus");

            migrationBuilder.CreateTable(
                name: "DialogueActivityType",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DialogueActivityType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DialogueGuiActionType",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DialogueGuiActionType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DialogueStatus",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DialogueStatus", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Dialogue",
                columns: table => new
                {
                    InternalId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BodyId = table.Column<long>(type: "bigint", nullable: false),
                    SearchTitleId = table.Column<long>(type: "bigint", nullable: false),
                    SenderNameId = table.Column<long>(type: "bigint", nullable: false),
                    StatusId = table.Column<int>(type: "integer", nullable: false),
                    TitleId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "current_timestamp at time zone 'utc'"),
                    DueAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ExpiresAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ExtendedStatus = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    Org = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Party = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ReadAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ServiceResourceIdentifier = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "current_timestamp at time zone 'utc'"),
                    VisibleFromUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dialogue", x => x.InternalId);
                    table.ForeignKey(
                        name: "FK_Dialogue_DialogueStatus_StatusId",
                        column: x => x.StatusId,
                        principalTable: "DialogueStatus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Dialogue_LocalizationSet_BodyId",
                        column: x => x.BodyId,
                        principalTable: "LocalizationSet",
                        principalColumn: "InternalId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Dialogue_LocalizationSet_SearchTitleId",
                        column: x => x.SearchTitleId,
                        principalTable: "LocalizationSet",
                        principalColumn: "InternalId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Dialogue_LocalizationSet_SenderNameId",
                        column: x => x.SenderNameId,
                        principalTable: "LocalizationSet",
                        principalColumn: "InternalId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Dialogue_LocalizationSet_TitleId",
                        column: x => x.TitleId,
                        principalTable: "LocalizationSet",
                        principalColumn: "InternalId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DialogueActivity",
                columns: table => new
                {
                    InternalId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DescriptionInternalId = table.Column<long>(type: "bigint", nullable: false),
                    DialogueId = table.Column<long>(type: "bigint", nullable: false),
                    TypeId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "current_timestamp at time zone 'utc'"),
                    DetailsApiUrl = table.Column<string>(type: "character varying(1023)", maxLength: 1023, nullable: true),
                    DetailsGuiUrl = table.Column<string>(type: "character varying(1023)", maxLength: 1023, nullable: true),
                    ExtendedType = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    PerformedBy = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
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
                        name: "FK_DialogueActivity_Dialogue_DialogueId",
                        column: x => x.DialogueId,
                        principalTable: "Dialogue",
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
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DialogueId = table.Column<long>(type: "bigint", nullable: false),
                    Action = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "current_timestamp at time zone 'utc'"),
                    DocumentationUrl = table.Column<string>(type: "character varying(1023)", maxLength: 1023, nullable: true),
                    HttpMethod = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    RequestSchema = table.Column<string>(type: "character varying(1023)", maxLength: 1023, nullable: true),
                    Resource = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    ResponseSchema = table.Column<string>(type: "character varying(1023)", maxLength: 1023, nullable: true),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "current_timestamp at time zone 'utc'"),
                    Url = table.Column<string>(type: "character varying(1023)", maxLength: 1023, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DialogueApiAction", x => x.InternalId);
                    table.ForeignKey(
                        name: "FK_DialogueApiAction_Dialogue_DialogueId",
                        column: x => x.DialogueId,
                        principalTable: "Dialogue",
                        principalColumn: "InternalId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DialogueAttachement",
                columns: table => new
                {
                    InternalId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DialogueId = table.Column<long>(type: "bigint", nullable: false),
                    DisplayNameInternalId = table.Column<long>(type: "bigint", nullable: false),
                    ContentType = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "current_timestamp at time zone 'utc'"),
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    Resource = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    SizeInBytes = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "current_timestamp at time zone 'utc'"),
                    Url = table.Column<string>(type: "character varying(1023)", maxLength: 1023, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DialogueAttachement", x => x.InternalId);
                    table.ForeignKey(
                        name: "FK_DialogueAttachement_Dialogue_DialogueId",
                        column: x => x.DialogueId,
                        principalTable: "Dialogue",
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
                name: "DialogueGuiAction",
                columns: table => new
                {
                    InternalId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DialogueId = table.Column<long>(type: "bigint", nullable: false),
                    TitleInternalId = table.Column<long>(type: "bigint", nullable: false),
                    TypeId = table.Column<int>(type: "integer", nullable: false),
                    Action = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "current_timestamp at time zone 'utc'"),
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    IsBackChannel = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleteAction = table.Column<bool>(type: "boolean", nullable: false),
                    Resource = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "current_timestamp at time zone 'utc'"),
                    Url = table.Column<string>(type: "character varying(1023)", maxLength: 1023, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DialogueGuiAction", x => x.InternalId);
                    table.ForeignKey(
                        name: "FK_DialogueGuiAction_DialogueGuiActionType_TypeId",
                        column: x => x.TypeId,
                        principalTable: "DialogueGuiActionType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DialogueGuiAction_Dialogue_DialogueId",
                        column: x => x.DialogueId,
                        principalTable: "Dialogue",
                        principalColumn: "InternalId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DialogueGuiAction_LocalizationSet_TitleInternalId",
                        column: x => x.TitleInternalId,
                        principalTable: "LocalizationSet",
                        principalColumn: "InternalId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DialogueTokenScope",
                columns: table => new
                {
                    InternalId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DialogueId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "current_timestamp at time zone 'utc'"),
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    Value = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DialogueTokenScope", x => x.InternalId);
                    table.ForeignKey(
                        name: "FK_DialogueTokenScope_Dialogue_DialogueId",
                        column: x => x.DialogueId,
                        principalTable: "Dialogue",
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
                    { 2, "InProgress" },
                    { 3, "Waiting" },
                    { 4, "Signing" },
                    { 5, "Cancelled" },
                    { 6, "Completed" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Dialogue_BodyId",
                table: "Dialogue",
                column: "BodyId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Dialogue_Id",
                table: "Dialogue",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Dialogue_SearchTitleId",
                table: "Dialogue",
                column: "SearchTitleId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Dialogue_SenderNameId",
                table: "Dialogue",
                column: "SenderNameId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Dialogue_StatusId",
                table: "Dialogue",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_Dialogue_TitleId",
                table: "Dialogue",
                column: "TitleId",
                unique: true);

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
                name: "IX_DialogueTokenScope_DialogueId_Value",
                table: "DialogueTokenScope",
                columns: new[] { "DialogueId", "Value" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DialogueTokenScope_Id",
                table: "DialogueTokenScope",
                column: "Id",
                unique: true);
        }
    }
}

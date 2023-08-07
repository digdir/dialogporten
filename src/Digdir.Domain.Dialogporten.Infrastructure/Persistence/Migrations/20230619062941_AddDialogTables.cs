using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Digdir.Domain.Dialogporten.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDialogTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                name: "DialogGuiActionPriority",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DialogGuiActionPriority", x => x.Id);
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
                name: "LocalizationSet",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "current_timestamp at time zone 'utc'")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocalizationSet", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OutboxMessage",
                columns: table => new
                {
                    EventId = table.Column<Guid>(type: "uuid", nullable: false),
                    EventType = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    EventPayload = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutboxMessage", x => x.EventId);
                });

            migrationBuilder.CreateTable(
                name: "Dialog",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "current_timestamp at time zone 'utc'"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "current_timestamp at time zone 'utc'"),
                    Org = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ServiceResource = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Party = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ExtendedStatus = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    VisibleFrom = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DueAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ReadAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    StatusId = table.Column<int>(type: "integer", nullable: false),
                    BodyId = table.Column<Guid>(type: "uuid", nullable: false),
                    TitleId = table.Column<Guid>(type: "uuid", nullable: false),
                    SenderNameId = table.Column<Guid>(type: "uuid", nullable: false),
                    SearchTitleId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dialog", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Dialog_DialogStatus_StatusId",
                        column: x => x.StatusId,
                        principalTable: "DialogStatus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Dialog_LocalizationSet_BodyId",
                        column: x => x.BodyId,
                        principalTable: "LocalizationSet",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Dialog_LocalizationSet_SearchTitleId",
                        column: x => x.SearchTitleId,
                        principalTable: "LocalizationSet",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Dialog_LocalizationSet_SenderNameId",
                        column: x => x.SenderNameId,
                        principalTable: "LocalizationSet",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Dialog_LocalizationSet_TitleId",
                        column: x => x.TitleId,
                        principalTable: "LocalizationSet",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Localization",
                columns: table => new
                {
                    CultureCode = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    LocalizationSetId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "current_timestamp at time zone 'utc'"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "current_timestamp at time zone 'utc'"),
                    Value = table.Column<string>(type: "character varying(4095)", maxLength: 4095, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Localization", x => new { x.LocalizationSetId, x.CultureCode });
                    table.ForeignKey(
                        name: "FK_Localization_LocalizationSet_LocalizationSetId",
                        column: x => x.LocalizationSetId,
                        principalTable: "LocalizationSet",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OutboxMessageConsumer",
                columns: table => new
                {
                    EventId = table.Column<Guid>(type: "uuid", nullable: false),
                    ConsumerName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutboxMessageConsumer", x => new { x.EventId, x.ConsumerName });
                    table.ForeignKey(
                        name: "FK_OutboxMessageConsumer_OutboxMessage_EventId",
                        column: x => x.EventId,
                        principalTable: "OutboxMessage",
                        principalColumn: "EventId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DialogElement",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "current_timestamp at time zone 'utc'"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "current_timestamp at time zone 'utc'"),
                    AuthorizationAttribute = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Type = table.Column<string>(type: "character varying(1023)", maxLength: 1023, nullable: true),
                    DialogId = table.Column<Guid>(type: "uuid", nullable: false),
                    DisplayNameId = table.Column<Guid>(type: "uuid", nullable: false),
                    RelatedDialogElementId = table.Column<Guid>(type: "uuid", nullable: true)
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
                    table.ForeignKey(
                        name: "FK_DialogElement_LocalizationSet_DisplayNameId",
                        column: x => x.DisplayNameId,
                        principalTable: "LocalizationSet",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DialogGuiAction",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "current_timestamp at time zone 'utc'"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "current_timestamp at time zone 'utc'"),
                    Action = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Url = table.Column<string>(type: "character varying(1023)", maxLength: 1023, nullable: false),
                    AuthorizationAttribute = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    IsBackChannel = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleteAction = table.Column<bool>(type: "boolean", nullable: false),
                    PriorityId = table.Column<int>(type: "integer", nullable: false),
                    TitleId = table.Column<Guid>(type: "uuid", nullable: false),
                    DialogId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DialogGuiAction", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DialogGuiAction_DialogGuiActionPriority_PriorityId",
                        column: x => x.PriorityId,
                        principalTable: "DialogGuiActionPriority",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DialogGuiAction_Dialog_DialogId",
                        column: x => x.DialogId,
                        principalTable: "Dialog",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DialogGuiAction_LocalizationSet_TitleId",
                        column: x => x.TitleId,
                        principalTable: "LocalizationSet",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DialogActivity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "current_timestamp at time zone 'utc'"),
                    ExtendedType = table.Column<string>(type: "character varying(1023)", maxLength: 1023, nullable: true),
                    TypeId = table.Column<int>(type: "integer", nullable: false),
                    DescriptionId = table.Column<Guid>(type: "uuid", nullable: false),
                    PerformedById = table.Column<Guid>(type: "uuid", nullable: false),
                    DialogId = table.Column<Guid>(type: "uuid", nullable: false),
                    RelatedActivityId = table.Column<Guid>(type: "uuid", nullable: true),
                    DialogElementId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DialogActivity", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DialogActivity_DialogActivityType_TypeId",
                        column: x => x.TypeId,
                        principalTable: "DialogActivityType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DialogActivity_DialogActivity_RelatedActivityId",
                        column: x => x.RelatedActivityId,
                        principalTable: "DialogActivity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_DialogActivity_DialogElement_DialogElementId",
                        column: x => x.DialogElementId,
                        principalTable: "DialogElement",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_DialogActivity_Dialog_DialogId",
                        column: x => x.DialogId,
                        principalTable: "Dialog",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DialogActivity_LocalizationSet_DescriptionId",
                        column: x => x.DescriptionId,
                        principalTable: "LocalizationSet",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DialogActivity_LocalizationSet_PerformedById",
                        column: x => x.PerformedById,
                        principalTable: "LocalizationSet",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DialogApiAction",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "current_timestamp at time zone 'utc'"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "current_timestamp at time zone 'utc'"),
                    Action = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    AuthorizationAttribute = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    DialogId = table.Column<Guid>(type: "uuid", nullable: false),
                    DialogElementId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DialogApiAction", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DialogApiAction_DialogElement_DialogElementId",
                        column: x => x.DialogElementId,
                        principalTable: "DialogElement",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DialogApiAction_Dialog_DialogId",
                        column: x => x.DialogId,
                        principalTable: "Dialog",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DialogElementUrl",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "current_timestamp at time zone 'utc'"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "current_timestamp at time zone 'utc'"),
                    Url = table.Column<string>(type: "character varying(1023)", maxLength: 1023, nullable: false),
                    MimeType = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    ConsumerTypeId = table.Column<int>(type: "integer", nullable: false),
                    DialogElementId = table.Column<Guid>(type: "uuid", nullable: false)
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

            migrationBuilder.CreateTable(
                name: "DialogApiActionEndpoint",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "current_timestamp at time zone 'utc'"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "current_timestamp at time zone 'utc'"),
                    Version = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Url = table.Column<string>(type: "character varying(1023)", maxLength: 1023, nullable: false),
                    HttpMethod = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    DocumentationUrl = table.Column<string>(type: "character varying(1023)", maxLength: 1023, nullable: true),
                    RequestSchema = table.Column<string>(type: "character varying(1023)", maxLength: 1023, nullable: true),
                    ResponseSchema = table.Column<string>(type: "character varying(1023)", maxLength: 1023, nullable: true),
                    Deprecated = table.Column<bool>(type: "boolean", nullable: false),
                    SunsetAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ActionId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DialogApiActionEndpoint", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DialogApiActionEndpoint_DialogApiAction_ActionId",
                        column: x => x.ActionId,
                        principalTable: "DialogApiAction",
                        principalColumn: "Id",
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
                table: "DialogElementUrlConsumerType",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Gui" },
                    { 2, "Api" }
                });

            migrationBuilder.InsertData(
                table: "DialogGuiActionPriority",
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
                name: "IX_Dialog_StatusId",
                table: "Dialog",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_Dialog_TitleId",
                table: "Dialog",
                column: "TitleId");

            migrationBuilder.CreateIndex(
                name: "IX_DialogActivity_DescriptionId",
                table: "DialogActivity",
                column: "DescriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_DialogActivity_DialogElementId",
                table: "DialogActivity",
                column: "DialogElementId");

            migrationBuilder.CreateIndex(
                name: "IX_DialogActivity_DialogId",
                table: "DialogActivity",
                column: "DialogId");

            migrationBuilder.CreateIndex(
                name: "IX_DialogActivity_PerformedById",
                table: "DialogActivity",
                column: "PerformedById");

            migrationBuilder.CreateIndex(
                name: "IX_DialogActivity_RelatedActivityId",
                table: "DialogActivity",
                column: "RelatedActivityId");

            migrationBuilder.CreateIndex(
                name: "IX_DialogActivity_TypeId",
                table: "DialogActivity",
                column: "TypeId");

            migrationBuilder.CreateIndex(
                name: "IX_DialogApiAction_DialogElementId",
                table: "DialogApiAction",
                column: "DialogElementId");

            migrationBuilder.CreateIndex(
                name: "IX_DialogApiAction_DialogId",
                table: "DialogApiAction",
                column: "DialogId");

            migrationBuilder.CreateIndex(
                name: "IX_DialogApiActionEndpoint_ActionId",
                table: "DialogApiActionEndpoint",
                column: "ActionId");

            migrationBuilder.CreateIndex(
                name: "IX_DialogElement_DialogId",
                table: "DialogElement",
                column: "DialogId");

            migrationBuilder.CreateIndex(
                name: "IX_DialogElement_DisplayNameId",
                table: "DialogElement",
                column: "DisplayNameId");

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

            migrationBuilder.CreateIndex(
                name: "IX_DialogGuiAction_DialogId",
                table: "DialogGuiAction",
                column: "DialogId");

            migrationBuilder.CreateIndex(
                name: "IX_DialogGuiAction_PriorityId",
                table: "DialogGuiAction",
                column: "PriorityId");

            migrationBuilder.CreateIndex(
                name: "IX_DialogGuiAction_TitleId",
                table: "DialogGuiAction",
                column: "TitleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DialogActivity");

            migrationBuilder.DropTable(
                name: "DialogApiActionEndpoint");

            migrationBuilder.DropTable(
                name: "DialogElementUrl");

            migrationBuilder.DropTable(
                name: "DialogGuiAction");

            migrationBuilder.DropTable(
                name: "Localization");

            migrationBuilder.DropTable(
                name: "OutboxMessageConsumer");

            migrationBuilder.DropTable(
                name: "DialogActivityType");

            migrationBuilder.DropTable(
                name: "DialogApiAction");

            migrationBuilder.DropTable(
                name: "DialogElementUrlConsumerType");

            migrationBuilder.DropTable(
                name: "DialogGuiActionPriority");

            migrationBuilder.DropTable(
                name: "OutboxMessage");

            migrationBuilder.DropTable(
                name: "DialogElement");

            migrationBuilder.DropTable(
                name: "Dialog");

            migrationBuilder.DropTable(
                name: "DialogStatus");

            migrationBuilder.DropTable(
                name: "LocalizationSet");
        }
    }
}

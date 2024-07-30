using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Digdir.Domain.Dialogporten.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                name: "DialogActorType",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DialogActorType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DialogContentType",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Required = table.Column<bool>(type: "boolean", nullable: false),
                    OutputInList = table.Column<bool>(type: "boolean", nullable: false),
                    MaxLength = table.Column<int>(type: "integer", nullable: false),
                    AllowedMediaTypes = table.Column<string[]>(type: "text[]", nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DialogContentType", x => x.Id);
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
                name: "DialogUserType",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DialogUserType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HttpVerb",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HttpVerb", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OutboxMessage",
                columns: table => new
                {
                    EventId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "current_timestamp at time zone 'utc'"),
                    EventType = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    EventPayload = table.Column<string>(type: "jsonb", nullable: false),
                    CorrelationId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false, defaultValueSql: "gen_random_uuid()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutboxMessage", x => x.EventId);
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
                name: "Dialog",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    Revision = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "current_timestamp at time zone 'utc'"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "current_timestamp at time zone 'utc'"),
                    Deleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Org = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false, collation: "C"),
                    ServiceResource = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false, collation: "C"),
                    ServiceResourceType = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Party = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false, collation: "C"),
                    Progress = table.Column<int>(type: "integer", nullable: true),
                    ExtendedStatus = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    ExternalReference = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    VisibleFrom = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DueAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    StatusId = table.Column<int>(type: "integer", nullable: false)
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
                name: "DialogActivity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "current_timestamp at time zone 'utc'"),
                    ExtendedType = table.Column<string>(type: "character varying(1023)", maxLength: 1023, nullable: true),
                    TypeId = table.Column<int>(type: "integer", nullable: false),
                    DialogId = table.Column<Guid>(type: "uuid", nullable: false),
                    RelatedActivityId = table.Column<Guid>(type: "uuid", nullable: true)
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
                        name: "FK_DialogActivity_Dialog_DialogId",
                        column: x => x.DialogId,
                        principalTable: "Dialog",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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
                    DialogId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DialogApiAction", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DialogApiAction_Dialog_DialogId",
                        column: x => x.DialogId,
                        principalTable: "Dialog",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DialogContent",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "current_timestamp at time zone 'utc'"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "current_timestamp at time zone 'utc'"),
                    MediaType = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    DialogId = table.Column<Guid>(type: "uuid", nullable: false),
                    TypeId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DialogContent", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DialogContent_DialogContentType_TypeId",
                        column: x => x.TypeId,
                        principalTable: "DialogContentType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DialogContent_Dialog_DialogId",
                        column: x => x.DialogId,
                        principalTable: "Dialog",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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
                    IsDeleteDialogAction = table.Column<bool>(type: "boolean", nullable: false),
                    PriorityId = table.Column<int>(type: "integer", nullable: false),
                    HttpMethodId = table.Column<int>(type: "integer", nullable: false),
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
                        name: "FK_DialogGuiAction_HttpVerb_HttpMethodId",
                        column: x => x.HttpMethodId,
                        principalTable: "HttpVerb",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DialogSearchTag",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    Value = table.Column<string>(type: "character varying(63)", maxLength: 63, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "current_timestamp at time zone 'utc'"),
                    DialogId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DialogSearchTag", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DialogSearchTag_Dialog_DialogId",
                        column: x => x.DialogId,
                        principalTable: "Dialog",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DialogSeenLog",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "current_timestamp at time zone 'utc'"),
                    IsViaServiceOwner = table.Column<bool>(type: "boolean", nullable: true),
                    DialogId = table.Column<Guid>(type: "uuid", nullable: false),
                    EndUserTypeId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DialogSeenLog", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DialogSeenLog_DialogUserType_EndUserTypeId",
                        column: x => x.EndUserTypeId,
                        principalTable: "DialogUserType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DialogSeenLog_Dialog_DialogId",
                        column: x => x.DialogId,
                        principalTable: "Dialog",
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
                    ExtendedType = table.Column<string>(type: "character varying(1023)", maxLength: 1023, nullable: true),
                    TypeId = table.Column<int>(type: "integer", nullable: false),
                    DialogId = table.Column<Guid>(type: "uuid", nullable: false),
                    RelatedTransmissionId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DialogTransmission", x => x.Id);
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
                name: "DialogApiActionEndpoint",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "current_timestamp at time zone 'utc'"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "current_timestamp at time zone 'utc'"),
                    Version = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Url = table.Column<string>(type: "character varying(1023)", maxLength: 1023, nullable: false),
                    DocumentationUrl = table.Column<string>(type: "character varying(1023)", maxLength: 1023, nullable: true),
                    RequestSchema = table.Column<string>(type: "character varying(1023)", maxLength: 1023, nullable: true),
                    ResponseSchema = table.Column<string>(type: "character varying(1023)", maxLength: 1023, nullable: true),
                    Deprecated = table.Column<bool>(type: "boolean", nullable: false),
                    SunsetAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    HttpMethodId = table.Column<int>(type: "integer", nullable: false),
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
                    table.ForeignKey(
                        name: "FK_DialogApiActionEndpoint_HttpVerb_HttpMethodId",
                        column: x => x.HttpMethodId,
                        principalTable: "HttpVerb",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Actor",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    ActorId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    ActorTypeId = table.Column<int>(type: "integer", nullable: false),
                    ActorName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Discriminator = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ActivityId = table.Column<Guid>(type: "uuid", nullable: true),
                    DialogSeenLogId = table.Column<Guid>(type: "uuid", nullable: true),
                    TransmissionId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Actor", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Actor_DialogActivity_ActivityId",
                        column: x => x.ActivityId,
                        principalTable: "DialogActivity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Actor_DialogActorType_ActorTypeId",
                        column: x => x.ActorTypeId,
                        principalTable: "DialogActorType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Actor_DialogSeenLog_DialogSeenLogId",
                        column: x => x.DialogSeenLogId,
                        principalTable: "DialogSeenLog",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Actor_DialogTransmission_TransmissionId",
                        column: x => x.TransmissionId,
                        principalTable: "DialogTransmission",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Attachment",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "current_timestamp at time zone 'utc'"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "current_timestamp at time zone 'utc'"),
                    Discriminator = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    DialogId = table.Column<Guid>(type: "uuid", nullable: true),
                    TransmissionId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Attachment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Attachment_DialogTransmission_TransmissionId",
                        column: x => x.TransmissionId,
                        principalTable: "DialogTransmission",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Attachment_Dialog_DialogId",
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
                name: "LocalizationSet",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "current_timestamp at time zone 'utc'"),
                    Discriminator = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    DialogGuiActionPrompt_GuiActionId = table.Column<Guid>(type: "uuid", nullable: true),
                    GuiActionId = table.Column<Guid>(type: "uuid", nullable: true),
                    ActivityId = table.Column<Guid>(type: "uuid", nullable: true),
                    AttachmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    DialogContentId = table.Column<Guid>(type: "uuid", nullable: true),
                    TransmissionContentId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocalizationSet", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LocalizationSet_Attachment_AttachmentId",
                        column: x => x.AttachmentId,
                        principalTable: "Attachment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LocalizationSet_DialogActivity_ActivityId",
                        column: x => x.ActivityId,
                        principalTable: "DialogActivity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LocalizationSet_DialogContent_DialogContentId",
                        column: x => x.DialogContentId,
                        principalTable: "DialogContent",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LocalizationSet_DialogGuiAction_DialogGuiActionPrompt_GuiAc~",
                        column: x => x.DialogGuiActionPrompt_GuiActionId,
                        principalTable: "DialogGuiAction",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LocalizationSet_DialogGuiAction_GuiActionId",
                        column: x => x.GuiActionId,
                        principalTable: "DialogGuiAction",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LocalizationSet_TransmissionContent_TransmissionContentId",
                        column: x => x.TransmissionContentId,
                        principalTable: "TransmissionContent",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Localization",
                columns: table => new
                {
                    LanguageCode = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    LocalizationSetId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "current_timestamp at time zone 'utc'"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "current_timestamp at time zone 'utc'"),
                    Value = table.Column<string>(type: "character varying(4095)", maxLength: 4095, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Localization", x => new { x.LocalizationSetId, x.LanguageCode });
                    table.ForeignKey(
                        name: "FK_Localization_LocalizationSet_LocalizationSetId",
                        column: x => x.LocalizationSetId,
                        principalTable: "LocalizationSet",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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
                table: "DialogActivityType",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "DialogCreated" },
                    { 2, "DialogClosed" },
                    { 3, "Information" },
                    { 4, "TransmissionOpened" },
                    { 5, "PaymentMade" },
                    { 6, "SignatureProvided" }
                });

            migrationBuilder.InsertData(
                table: "DialogActorType",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "PartyRepresentative" },
                    { 2, "ServiceOwner" }
                });

            migrationBuilder.InsertData(
                table: "DialogContentType",
                columns: new[] { "Id", "AllowedMediaTypes", "MaxLength", "Name", "OutputInList", "Required" },
                values: new object[,]
                {
                    { 1, new[] { "text/plain" }, 255, "Title", true, true },
                    { 2, new[] { "text/plain" }, 255, "SenderName", true, false },
                    { 3, new[] { "text/plain" }, 255, "Summary", true, true },
                    { 4, new[] { "text/html", "text/plain", "text/markdown" }, 1023, "AdditionalInfo", false, false },
                    { 5, new[] { "text/plain" }, 20, "ExtendedStatus", true, false },
                    { 6, new[] { "application/vnd.dialogporten.frontchannelembed+json;type=markdown" }, 1023, "MainContentReference", false, false }
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
                    { 1, "New" },
                    { 2, "InProgress" },
                    { 3, "Signing" },
                    { 4, "Processing" },
                    { 5, "RequiresAttention" },
                    { 6, "Completed" }
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
                table: "DialogUserType",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 0, "Unknown" },
                    { 1, "Person" },
                    { 2, "LegacySystemUser" },
                    { 3, "SystemUser" },
                    { 4, "ServiceOwner" },
                    { 5, "ServiceOwnerOnBehalfOfPerson" }
                });

            migrationBuilder.InsertData(
                table: "HttpVerb",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "GET" },
                    { 2, "POST" },
                    { 3, "PUT" },
                    { 4, "PATCH" },
                    { 5, "DELETE" },
                    { 6, "HEAD" },
                    { 7, "OPTIONS" },
                    { 8, "TRACE" },
                    { 9, "CONNECT" }
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
                name: "IX_Actor_ActivityId",
                table: "Actor",
                column: "ActivityId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Actor_ActorTypeId",
                table: "Actor",
                column: "ActorTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Actor_DialogSeenLogId",
                table: "Actor",
                column: "DialogSeenLogId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Actor_TransmissionId",
                table: "Actor",
                column: "TransmissionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Attachment_DialogId",
                table: "Attachment",
                column: "DialogId");

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
                name: "IX_Dialog_CreatedAt",
                table: "Dialog",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Dialog_DueAt",
                table: "Dialog",
                column: "DueAt");

            migrationBuilder.CreateIndex(
                name: "IX_Dialog_Org",
                table: "Dialog",
                column: "Org");

            migrationBuilder.CreateIndex(
                name: "IX_Dialog_Party",
                table: "Dialog",
                column: "Party");

            migrationBuilder.CreateIndex(
                name: "IX_Dialog_ServiceResource",
                table: "Dialog",
                column: "ServiceResource");

            migrationBuilder.CreateIndex(
                name: "IX_Dialog_StatusId",
                table: "Dialog",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_Dialog_UpdatedAt",
                table: "Dialog",
                column: "UpdatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_DialogActivity_DialogId",
                table: "DialogActivity",
                column: "DialogId");

            migrationBuilder.CreateIndex(
                name: "IX_DialogActivity_RelatedActivityId",
                table: "DialogActivity",
                column: "RelatedActivityId");

            migrationBuilder.CreateIndex(
                name: "IX_DialogActivity_TypeId",
                table: "DialogActivity",
                column: "TypeId");

            migrationBuilder.CreateIndex(
                name: "IX_DialogApiAction_DialogId",
                table: "DialogApiAction",
                column: "DialogId");

            migrationBuilder.CreateIndex(
                name: "IX_DialogApiActionEndpoint_ActionId",
                table: "DialogApiActionEndpoint",
                column: "ActionId");

            migrationBuilder.CreateIndex(
                name: "IX_DialogApiActionEndpoint_HttpMethodId",
                table: "DialogApiActionEndpoint",
                column: "HttpMethodId");

            migrationBuilder.CreateIndex(
                name: "IX_DialogContent_DialogId_TypeId",
                table: "DialogContent",
                columns: new[] { "DialogId", "TypeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DialogContent_TypeId",
                table: "DialogContent",
                column: "TypeId");

            migrationBuilder.CreateIndex(
                name: "IX_DialogGuiAction_DialogId",
                table: "DialogGuiAction",
                column: "DialogId");

            migrationBuilder.CreateIndex(
                name: "IX_DialogGuiAction_HttpMethodId",
                table: "DialogGuiAction",
                column: "HttpMethodId");

            migrationBuilder.CreateIndex(
                name: "IX_DialogGuiAction_PriorityId",
                table: "DialogGuiAction",
                column: "PriorityId");

            migrationBuilder.CreateIndex(
                name: "IX_DialogSearchTag_DialogId_Value",
                table: "DialogSearchTag",
                columns: new[] { "DialogId", "Value" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DialogSeenLog_DialogId",
                table: "DialogSeenLog",
                column: "DialogId");

            migrationBuilder.CreateIndex(
                name: "IX_DialogSeenLog_EndUserTypeId",
                table: "DialogSeenLog",
                column: "EndUserTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_DialogTransmission_DialogId",
                table: "DialogTransmission",
                column: "DialogId");

            migrationBuilder.CreateIndex(
                name: "IX_DialogTransmission_RelatedTransmissionId",
                table: "DialogTransmission",
                column: "RelatedTransmissionId");

            migrationBuilder.CreateIndex(
                name: "IX_DialogTransmission_TypeId",
                table: "DialogTransmission",
                column: "TypeId");

            migrationBuilder.CreateIndex(
                name: "IX_LocalizationSet_ActivityId",
                table: "LocalizationSet",
                column: "ActivityId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LocalizationSet_AttachmentId",
                table: "LocalizationSet",
                column: "AttachmentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LocalizationSet_DialogContentId",
                table: "LocalizationSet",
                column: "DialogContentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LocalizationSet_DialogGuiActionPrompt_GuiActionId",
                table: "LocalizationSet",
                column: "DialogGuiActionPrompt_GuiActionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LocalizationSet_GuiActionId",
                table: "LocalizationSet",
                column: "GuiActionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LocalizationSet_TransmissionContentId",
                table: "LocalizationSet",
                column: "TransmissionContentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TransmissionContent_TransmissionId_TypeId",
                table: "TransmissionContent",
                columns: new[] { "TransmissionId", "TypeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TransmissionContent_TypeId",
                table: "TransmissionContent",
                column: "TypeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Actor");

            migrationBuilder.DropTable(
                name: "AttachmentUrl");

            migrationBuilder.DropTable(
                name: "DialogApiActionEndpoint");

            migrationBuilder.DropTable(
                name: "DialogSearchTag");

            migrationBuilder.DropTable(
                name: "Localization");

            migrationBuilder.DropTable(
                name: "OutboxMessageConsumer");

            migrationBuilder.DropTable(
                name: "DialogActorType");

            migrationBuilder.DropTable(
                name: "DialogSeenLog");

            migrationBuilder.DropTable(
                name: "AttachmentUrlConsumerType");

            migrationBuilder.DropTable(
                name: "DialogApiAction");

            migrationBuilder.DropTable(
                name: "LocalizationSet");

            migrationBuilder.DropTable(
                name: "OutboxMessage");

            migrationBuilder.DropTable(
                name: "DialogUserType");

            migrationBuilder.DropTable(
                name: "Attachment");

            migrationBuilder.DropTable(
                name: "DialogActivity");

            migrationBuilder.DropTable(
                name: "DialogContent");

            migrationBuilder.DropTable(
                name: "DialogGuiAction");

            migrationBuilder.DropTable(
                name: "TransmissionContent");

            migrationBuilder.DropTable(
                name: "DialogActivityType");

            migrationBuilder.DropTable(
                name: "DialogContentType");

            migrationBuilder.DropTable(
                name: "DialogGuiActionPriority");

            migrationBuilder.DropTable(
                name: "HttpVerb");

            migrationBuilder.DropTable(
                name: "DialogTransmission");

            migrationBuilder.DropTable(
                name: "TransmissionContentType");

            migrationBuilder.DropTable(
                name: "DialogTransmissionType");

            migrationBuilder.DropTable(
                name: "Dialog");

            migrationBuilder.DropTable(
                name: "DialogStatus");
        }
    }
}

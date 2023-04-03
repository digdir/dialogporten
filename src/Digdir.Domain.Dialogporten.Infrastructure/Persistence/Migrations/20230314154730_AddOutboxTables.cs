using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Digdir.Domain.Dialogporten.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddOutboxTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OutboxMessage",
                columns: table => new
                {
                    EventId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    NumberOfAttempts = table.Column<int>(type: "integer", nullable: false),
                    Content = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Error = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    LastAttemptedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutboxMessage", x => x.EventId);
                });

            migrationBuilder.CreateTable(
                name: "OutboxMessageConsumer",
                columns: table => new
                {
                    EventId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutboxMessageConsumer", x => new { x.EventId, x.Name });
                    table.ForeignKey(
                        name: "FK_OutboxMessageConsumer_OutboxMessage_EventId",
                        column: x => x.EventId,
                        principalTable: "OutboxMessage",
                        principalColumn: "EventId",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OutboxMessageConsumer");

            migrationBuilder.DropTable(
                name: "OutboxMessage");
        }
    }
}

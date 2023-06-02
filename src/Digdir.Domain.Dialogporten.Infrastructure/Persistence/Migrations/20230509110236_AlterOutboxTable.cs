using System;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Digdir.Domain.Dialogporten.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AlterOutboxTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Content",
                table: "OutboxMessage");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                table: "OutboxMessage");

            migrationBuilder.DropColumn(
                name: "Error",
                table: "OutboxMessage");

            migrationBuilder.DropColumn(
                name: "LastAttemptedAtUtc",
                table: "OutboxMessage");

            migrationBuilder.DropColumn(
                name: "NumberOfAttempts",
                table: "OutboxMessage");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "OutboxMessage");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "OutboxMessageConsumer",
                newName: "ConsumerName");

            migrationBuilder.RenameColumn(
                name: "Type",
                table: "OutboxMessage",
                newName: "EventType");

            migrationBuilder.AddColumn<JsonDocument>(
                name: "EventPayload",
                table: "OutboxMessage",
                type: "jsonb",
                nullable: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EventPayload",
                table: "OutboxMessage");

            migrationBuilder.RenameColumn(
                name: "ConsumerName",
                table: "OutboxMessageConsumer",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "EventType",
                table: "OutboxMessage",
                newName: "Type");

            migrationBuilder.AddColumn<string>(
                name: "Content",
                table: "OutboxMessage",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedAtUtc",
                table: "OutboxMessage",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<string>(
                name: "Error",
                table: "OutboxMessage",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastAttemptedAtUtc",
                table: "OutboxMessage",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NumberOfAttempts",
                table: "OutboxMessage",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "OutboxMessage",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}

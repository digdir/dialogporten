using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Digdir.Domain.Dialogporten.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateMassTransit830 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddForeignKey(
                name: "FK_MassTransitOutboxMessage_MassTransitInboxState_InboxMessage~",
                table: "MassTransitOutboxMessage",
                columns: new[] { "InboxMessageId", "InboxConsumerId" },
                principalTable: "MassTransitInboxState",
                principalColumns: new[] { "MessageId", "ConsumerId" });

            migrationBuilder.AddForeignKey(
                name: "FK_MassTransitOutboxMessage_MassTransitOutboxState_OutboxId",
                table: "MassTransitOutboxMessage",
                column: "OutboxId",
                principalTable: "MassTransitOutboxState",
                principalColumn: "OutboxId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MassTransitOutboxMessage_MassTransitInboxState_InboxMessage~",
                table: "MassTransitOutboxMessage");

            migrationBuilder.DropForeignKey(
                name: "FK_MassTransitOutboxMessage_MassTransitOutboxState_OutboxId",
                table: "MassTransitOutboxMessage");
        }
    }
}

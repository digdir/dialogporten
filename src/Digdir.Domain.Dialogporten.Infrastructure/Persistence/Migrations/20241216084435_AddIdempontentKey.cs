using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Digdir.Domain.Dialogporten.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddIdempontentKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IdempotentIdIdempotent",
                table: "Dialog",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IdempotentIdOrg",
                table: "Dialog",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "IdempotentId",
                columns: table => new
                {
                    Org = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Idempotent = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IdempotentId", x => new { x.Idempotent, x.Org });
                });

            migrationBuilder.CreateIndex(
                name: "IX_Dialog_IdempotentIdIdempotent_IdempotentIdOrg",
                table: "Dialog",
                columns: new[] { "IdempotentIdIdempotent", "IdempotentIdOrg" });

            migrationBuilder.AddForeignKey(
                name: "FK_Dialog_IdempotentId_IdempotentIdIdempotent_IdempotentIdOrg",
                table: "Dialog",
                columns: new[] { "IdempotentIdIdempotent", "IdempotentIdOrg" },
                principalTable: "IdempotentId",
                principalColumns: new[] { "Idempotent", "Org" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Dialog_IdempotentId_IdempotentIdIdempotent_IdempotentIdOrg",
                table: "Dialog");

            migrationBuilder.DropTable(
                name: "IdempotentId");

            migrationBuilder.DropIndex(
                name: "IX_Dialog_IdempotentIdIdempotent_IdempotentIdOrg",
                table: "Dialog");

            migrationBuilder.DropColumn(
                name: "IdempotentIdIdempotent",
                table: "Dialog");

            migrationBuilder.DropColumn(
                name: "IdempotentIdOrg",
                table: "Dialog");
        }
    }
}

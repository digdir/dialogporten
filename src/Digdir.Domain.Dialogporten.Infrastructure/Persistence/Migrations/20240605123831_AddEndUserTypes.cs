using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Digdir.Domain.Dialogporten.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddEndUserTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EndUserTypeId",
                table: "DialogSeenLog",
                type: "integer",
                nullable: false,
                defaultValue: 0);

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

            migrationBuilder.CreateIndex(
                name: "IX_DialogSeenLog_EndUserTypeId",
                table: "DialogSeenLog",
                column: "EndUserTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_DialogSeenLog_DialogUserType_EndUserTypeId",
                table: "DialogSeenLog",
                column: "EndUserTypeId",
                principalTable: "DialogUserType",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DialogSeenLog_DialogUserType_EndUserTypeId",
                table: "DialogSeenLog");

            migrationBuilder.DropTable(
                name: "DialogUserType");

            migrationBuilder.DropIndex(
                name: "IX_DialogSeenLog_EndUserTypeId",
                table: "DialogSeenLog");

            migrationBuilder.DropColumn(
                name: "EndUserTypeId",
                table: "DialogSeenLog");
        }
    }
}

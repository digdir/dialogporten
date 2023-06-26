using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Digdir.Domain.Dialogporten.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddHttpVerbTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HttpMethod",
                table: "DialogApiActionEndpoint");

            migrationBuilder.AddColumn<int>(
                name: "HttpMethodId",
                table: "DialogApiActionEndpoint",
                type: "integer",
                nullable: false,
                defaultValue: 1);

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

            migrationBuilder.CreateIndex(
                name: "IX_DialogApiActionEndpoint_HttpMethodId",
                table: "DialogApiActionEndpoint",
                column: "HttpMethodId");

            migrationBuilder.AddForeignKey(
                name: "FK_DialogApiActionEndpoint_HttpVerb_HttpMethodId",
                table: "DialogApiActionEndpoint",
                column: "HttpMethodId",
                principalTable: "HttpVerb",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DialogApiActionEndpoint_HttpVerb_HttpMethodId",
                table: "DialogApiActionEndpoint");

            migrationBuilder.DropTable(
                name: "HttpVerb");

            migrationBuilder.DropIndex(
                name: "IX_DialogApiActionEndpoint_HttpMethodId",
                table: "DialogApiActionEndpoint");

            migrationBuilder.DropColumn(
                name: "HttpMethodId",
                table: "DialogApiActionEndpoint");

            migrationBuilder.AddColumn<string>(
                name: "HttpMethod",
                table: "DialogApiActionEndpoint",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");
        }
    }
}

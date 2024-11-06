using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Digdir.Domain.Dialogporten.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddResourcePolicyMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ResourcePolicyMetadata",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    Resource = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    MinimumSecurityLevel = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "current_timestamp at time zone 'utc'"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "current_timestamp at time zone 'utc'")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResourcePolicyMetadata", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ResourcePolicyMetadata_Resource",
                table: "ResourcePolicyMetadata",
                column: "Resource",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ResourcePolicyMetadata");
        }
    }
}

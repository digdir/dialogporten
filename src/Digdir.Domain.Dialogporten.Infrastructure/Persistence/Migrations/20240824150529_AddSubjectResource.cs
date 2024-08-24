using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Digdir.Domain.Dialogporten.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSubjectResource : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SubjectResource",
                columns: table => new
                {
                    Subject = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Resource = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateIndex(
                name: "IX_SubjectResource_Resource",
                table: "SubjectResource",
                column: "Resource");

            migrationBuilder.CreateIndex(
                name: "IX_SubjectResource_Subject",
                table: "SubjectResource",
                column: "Subject");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SubjectResource");
        }
    }
}

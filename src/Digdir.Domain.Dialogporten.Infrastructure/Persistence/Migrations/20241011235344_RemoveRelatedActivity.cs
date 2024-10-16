using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Digdir.Domain.Dialogporten.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveRelatedActivity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DialogActivity_DialogActivity_RelatedActivityId",
                table: "DialogActivity");

            migrationBuilder.DropIndex(
                name: "IX_DialogActivity_RelatedActivityId",
                table: "DialogActivity");

            migrationBuilder.DropColumn(
                name: "RelatedActivityId",
                table: "DialogActivity");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "RelatedActivityId",
                table: "DialogActivity",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DialogActivity_RelatedActivityId",
                table: "DialogActivity",
                column: "RelatedActivityId");

            migrationBuilder.AddForeignKey(
                name: "FK_DialogActivity_DialogActivity_RelatedActivityId",
                table: "DialogActivity",
                column: "RelatedActivityId",
                principalTable: "DialogActivity",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}

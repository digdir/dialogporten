using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Digdir.Domain.Dialogporten.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class MakeEfAwareOfIndexAndCollationChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Remove cowboy changes 🤠
            migrationBuilder.Sql(
                """
                DROP INDEX IF EXISTS "IX_Dialog_Org";
                DROP INDEX IF EXISTS "IX_Dialog_ServiceResource";
                DROP INDEX IF EXISTS "IX_Dialog_Party";
                ALTER TABLE "Dialog" ALTER COLUMN "Org" TYPE character varying(255) USING "Org"::varchar;
                ALTER TABLE "Dialog" ALTER COLUMN "ServiceResource" TYPE character varying(255) USING "ServiceResource"::varchar;
                ALTER TABLE "Dialog" ALTER COLUMN "Party" TYPE character varying(255) USING "Party"::varchar;
                """);

            migrationBuilder.AlterColumn<string>(
                name: "ServiceResource",
                table: "Dialog",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                collation: "C",
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<string>(
                name: "Party",
                table: "Dialog",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                collation: "C",
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<string>(
                name: "Org",
                table: "Dialog",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                collation: "C",
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255);

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Dialog_Org",
                table: "Dialog");

            migrationBuilder.DropIndex(
                name: "IX_Dialog_Party",
                table: "Dialog");

            migrationBuilder.DropIndex(
                name: "IX_Dialog_ServiceResource",
                table: "Dialog");

            migrationBuilder.AlterColumn<string>(
                name: "ServiceResource",
                table: "Dialog",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255,
                oldCollation: "C");

            migrationBuilder.AlterColumn<string>(
                name: "Party",
                table: "Dialog",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255,
                oldCollation: "C");

            migrationBuilder.AlterColumn<string>(
                name: "Org",
                table: "Dialog",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255,
                oldCollation: "C");

            migrationBuilder.Sql(
                """
                ALTER TABLE "Dialog" ALTER COLUMN "Org" TYPE character varying(255) COLLATE "C" USING "Org"::varchar;
                ALTER TABLE "Dialog" ALTER COLUMN "ServiceResource" TYPE character varying(255) COLLATE "C" USING "ServiceResource"::varchar;
                ALTER TABLE "Dialog" ALTER COLUMN "Party" TYPE character varying(255) COLLATE "C" USING "Party"::varchar;
             
                CREATE INDEX IF NOT EXISTS "IX_Dialog_ServiceResource" ON "Dialog" USING btree ("ServiceResource");
                CREATE INDEX IF NOT EXISTS "IX_Dialog_Party" ON "Dialog" USING btree ("Party");
                CREATE INDEX IF NOT EXISTS "IX_Dialog_Org" ON "Dialog" USING btree ("Org");
                """);
        }
    }
}

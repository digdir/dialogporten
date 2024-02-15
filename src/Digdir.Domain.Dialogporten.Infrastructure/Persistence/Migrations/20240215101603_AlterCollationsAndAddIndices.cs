using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Digdir.Domain.Dialogporten.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AlterCollationsAndAddIndices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Change collation to "C" for faster indices.
            // Add b-tree indices for ServiceResource and Party as they are heavily used in queries.
            migrationBuilder.Sql(
                """
                 ALTER TABLE "Dialog" ALTER COLUMN "Org" TYPE character varying(255) COLLATE "C" USING "Org"::varchar;
                 ALTER TABLE "Dialog" ALTER COLUMN "ServiceResource" TYPE character varying(255) COLLATE "C" USING "ServiceResource"::varchar;
                 ALTER TABLE "Dialog" ALTER COLUMN "Party" TYPE character varying(255) COLLATE "C" USING "Party"::varchar;
             
                 CREATE INDEX IF NOT EXISTS "IX_Dialog_ServiceResource" ON "Dialog" USING btree ("ServiceResource");
                 CREATE INDEX IF NOT EXISTS "IX_Dialog_Party" ON "Dialog" USING btree ("Party");
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Revert collation to default.
            migrationBuilder.Sql(
                """
                 DROP INDEX IF EXISTS "IX_Dialog_ServiceResource";
                 DROP INDEX IF EXISTS "IX_Dialog_Party";

                 ALTER TABLE "Dialog" ALTER COLUMN "Org" TYPE character varying(255) USING "Org"::varchar;
                 ALTER TABLE "Dialog" ALTER COLUMN "ServiceResource" TYPE character varying(255) USING "ServiceResource"::varchar;
                 ALTER TABLE "Dialog" ALTER COLUMN "Party" TYPE character varying(255) USING "Party"::varchar;
               """);
        }
    }
}

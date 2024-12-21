namespace Digdir.Tool.Dialogporten.LargeDataSetGenerator;

internal static class Sql
{
    internal static string DisableAllIndexesConstraints
        => """
        BEGIN;
        
        -- drop indexes/constraints
        DROP TABLE IF EXISTS constraint_index_backup;
        CREATE TABLE IF NOT EXISTS constraint_index_backup
        AS
        SELECT 'index' type
             ,4 priority
             ,format('DROP INDEX IF EXISTS %I.%I', schemaname, indexname) drop_script
             ,indexdef create_script
        FROM pg_indexes
        LEFT JOIN pg_constraint
            ON pg_indexes.indexname = pg_constraint.conname
        WHERE schemaname = 'public'
            AND pg_constraint.oid IS NULL
        UNION SELECT 'constraint' type
            ,CASE contype
                WHEN 'p' THEN 1
                WHEN 'u' THEN 2
                ELSE 3
            END priority
                ,format('ALTER TABLE %s DROP CONSTRAINT IF EXISTS %I CASCADE', conrelid::regclass, conname) drop_script
                ,format('ALTER TABLE %s ADD CONSTRAINT %I %s', conrelid::regclass, conname, pg_get_constraintdef(c.oid)) create_script
        FROM pg_constraint c
        JOIN pg_namespace n
            ON n.oid = c.connamespace
        WHERE n.nspname = 'public';
        --     AND contype != 'p';
        
        -- Drop constraints and indexes
        DO
        $$
        DECLARE x RECORD;
        BEGIN
        FOR x IN
        SELECT drop_script
        FROM constraint_index_backup
        ORDER BY priority DESC
            LOOP
                EXECUTE x.drop_script;
            END LOOP;
        END;
        $$;
        
        COMMIT;
        """;

    internal static string ReEnableAllIndexesConstraints
        => """
        BEGIN;
        
        -- Restore constraints and indexes
        DO
        $$
            DECLARE
        x RECORD;
        BEGIN
        FOR x IN
        SELECT create_script
        FROM constraint_index_backup
        ORDER BY priority
            LOOP
                EXECUTE x.create_script;
        END LOOP;
        END;
        $$;
        
        COMMIT;
        """;

    internal static string CreateServiceResources
        => """
           DROP TABLE IF EXISTS temp_services;
           CREATE TABLE temp_services (
               Id SERIAL PRIMARY KEY,
               value TEXT
           );
           
           INSERT INTO temp_services (value)
           VALUES
               ('urn:altinn:resource:ttd-dialogporten-performance-test-01'),
               ('urn:altinn:resource:ttd-dialogporten-performance-test-02'),
               ('urn:altinn:resource:ttd-dialogporten-performance-test-03'),
               ('urn:altinn:resource:ttd-dialogporten-performance-test-04'),
               ('urn:altinn:resource:ttd-dialogporten-performance-test-05'),
               ('urn:altinn:resource:ttd-dialogporten-performance-test-06'),
               ('urn:altinn:resource:ttd-dialogporten-performance-test-07'),
               ('urn:altinn:resource:ttd-dialogporten-performance-test-08'),
               ('urn:altinn:resource:ttd-dialogporten-performance-test-09'),
               ('urn:altinn:resource:ttd-dialogporten-performance-test-10');
           """;

    internal static string CreateParties
        => """
           DROP TABLE IF EXISTS temp_parties;
            CREATE TABLE temp_parties (
                Id SERIAL PRIMARY KEY,
                value TEXT
            );
           """;

}

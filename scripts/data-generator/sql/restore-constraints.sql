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

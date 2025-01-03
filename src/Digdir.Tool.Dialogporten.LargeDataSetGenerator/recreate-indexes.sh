#!/bin/bash

# Check for a connection string argument
if [ -z "$1" ]; then
  echo "Usage: $0 <connection_string>"
  echo "Example: $0 \"postgresql://username:password@host:port/database\""
  exit 1
fi

# Connection string from the first argument
CONNECTION_STRING="$1"

# SQL command
SQL_COMMAND=$(cat <<EOF
BEGIN;

-- Restore constraints and indexes
DO
\$\$
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
\$\$;

COMMIT;
EOF
)

# Start timer
START_TIME=$(date +%s)

# Execute the SQL
echo "$SQL_COMMAND" | psql "$CONNECTION_STRING"

# End timer
END_TIME=$(date +%s)

# Calculate and print total execution time
TOTAL_TIME=$((END_TIME - START_TIME))
echo "SQL script executed in $TOTAL_TIME seconds 🚀"


-- This table will be populated by COPY command in the setup script.
-- Requires a file named 'parties.txt' in the data-generator directory.
-- This is git-ignored due to its size.
CREATE TABLE temp_parties
(
    Id SERIAL PRIMARY KEY,
    value TEXT
);

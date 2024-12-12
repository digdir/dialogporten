BEGIN;

CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE OR REPLACE FUNCTION uuid_generate_v7_from_timestamp(ts TIMESTAMPTZ, uuidSeed UUID) RETURNS UUID
AS $$
SELECT encode(
           set_bit(
               set_bit(
                   overlay(uuid_send(uuid_generate_v5(uuidSeed, to_char(ts, 'YYYY/MM/DD HH24:MM:SS')))
                       PLACING substring(int8send(floor(extract(epoch FROM ts) * 1000)::BIGINT) FROM 3)
                FROM 1 FOR 6), 52, 1), 53, 1), 'hex')::UUID;
$$ LANGUAGE SQL VOLATILE;

COMMIT;

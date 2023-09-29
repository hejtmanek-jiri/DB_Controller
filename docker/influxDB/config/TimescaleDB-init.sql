CREATE TABLE IF NOT EXISTS data (
    time TIMESTAMPTZ NOT NULL,
    D1 varchar(255) NULL,
    D2 varchar(255) NULL,
    D3 varchar(255) NULL,
    D4 varchar(255) NULL,
    Author varchar(255) NULL,
    Value DOUBLE PRECISION NOT NULL,
    Corrected_value DOUBLE PRECISION NOT NULL
);

CREATE INDEX IF NOT EXISTS d1_index ON data (D1);
CREATE INDEX IF NOT EXISTS d2_index ON data (D2);
CREATE INDEX IF NOT EXISTS d3_index ON data (D3);
CREATE INDEX IF NOT EXISTS d4_index ON data (D4);

SELECT create_hypertable('Data', 'time');
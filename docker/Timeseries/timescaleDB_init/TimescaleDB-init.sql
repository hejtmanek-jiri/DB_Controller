CREATE TABLE data (
    id BIGSERIAL,
    time TIMESTAMPTZ NOT NULL,
    D1 varchar(255) NULL,
    D2 varchar(255) NULL,
    D3 varchar(255) NULL,
    D4 varchar(255) NULL,
    Author varchar(255) NULL,
    Value DOUBLE PRECISION NOT NULL,
    Corrected_value DOUBLE PRECISION NOT NULL
);

CREATE INDEX IF NOT EXISTS idx_dim ON data (D1, D2, D3, D4);
CREATE INDEX IF NOT EXISTS idx_id ON data (id);
CREATE TABLE data (
    time TIMESTAMPTZ NOT NULL,
    D1 varchar(255) NULL,
    D2 varchar(255) NULL,
    D3 varchar(255) NULL,
    D4 varchar(255) NULL,
    Author varchar(255) NULL,
    Value DOUBLE PRECISION NOT NULL,
    Corrected_value DOUBLE PRECISION NOT NULL,
    PRIMARY KEY (time)
);

SELECT create_hypertable('data', 'time');
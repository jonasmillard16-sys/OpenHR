-- LAS: Lagen om anställningsskydd

CREATE TABLE las.accumulations (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    anstalld_id UUID NOT NULL,
    anstallningsform VARCHAR(30) NOT NULL,
    ackumulerade_dagar INTEGER NOT NULL DEFAULT 0,
    referensfonster_start DATE NOT NULL,
    referensfonster_slut DATE NOT NULL,
    status VARCHAR(30) NOT NULL DEFAULT 'UnderGrans',
    konverterings_datum DATE,
    har_foretradesratt BOOLEAN DEFAULT FALSE,
    foretradesratt_utgar DATE,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ
);

CREATE INDEX idx_las_anstalld ON las.accumulations(anstalld_id);
CREATE INDEX idx_las_status ON las.accumulations(status);

CREATE TABLE las.periods (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    accumulation_id UUID NOT NULL REFERENCES las.accumulations(id),
    start_datum DATE NOT NULL,
    slut_datum DATE NOT NULL,
    antal_dagar INTEGER NOT NULL,
    anstallnings_id VARCHAR(100)
);

CREATE TABLE las.events (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    accumulation_id UUID NOT NULL REFERENCES las.accumulations(id),
    typ VARCHAR(50) NOT NULL,
    tidpunkt TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    beskrivning TEXT
);

-- Scheduling: Schema, instämpling, bemanningsöversikt

CREATE TABLE scheduling.schedules (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    enhet_id UUID NOT NULL,
    namn VARCHAR(200) NOT NULL,
    typ VARCHAR(20) NOT NULL CHECK (typ IN ('Grundschema', 'Periodschema', 'Operativt')),
    period_start DATE NOT NULL,
    period_slut DATE,
    cykel_langd_veckor INTEGER,
    status VARCHAR(20) NOT NULL DEFAULT 'Utkast',
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ
);

CREATE TABLE scheduling.scheduled_shifts (
    id UUID NOT NULL DEFAULT uuid_generate_v4(),
    schema_id UUID NOT NULL REFERENCES scheduling.schedules(id),
    anstalld_id UUID NOT NULL,
    datum DATE NOT NULL,
    pass_typ VARCHAR(20) NOT NULL CHECK (pass_typ IN ('Dag', 'Kvall', 'Natt', 'Jour', 'Beredskap', 'Delat')),
    planerad_start TIME NOT NULL,
    planerad_slut TIME NOT NULL,
    rast INTERVAL NOT NULL DEFAULT '60 minutes',
    faktisk_start TIME,
    faktisk_slut TIME,
    status VARCHAR(20) NOT NULL DEFAULT 'Planerad',
    ob_kategori VARCHAR(20),
    PRIMARY KEY (id, datum)
) PARTITION BY RANGE (datum);

-- Partitioner per månad (rullande)
CREATE TABLE scheduling.scheduled_shifts_2026_03 PARTITION OF scheduling.scheduled_shifts FOR VALUES FROM ('2026-03-01') TO ('2026-04-01');
CREATE TABLE scheduling.scheduled_shifts_2026_04 PARTITION OF scheduling.scheduled_shifts FOR VALUES FROM ('2026-04-01') TO ('2026-05-01');
CREATE TABLE scheduling.scheduled_shifts_2026_05 PARTITION OF scheduling.scheduled_shifts FOR VALUES FROM ('2026-05-01') TO ('2026-06-01');
CREATE TABLE scheduling.scheduled_shifts_2026_06 PARTITION OF scheduling.scheduled_shifts FOR VALUES FROM ('2026-06-01') TO ('2026-07-01');

CREATE INDEX idx_shifts_anstalld_datum ON scheduling.scheduled_shifts(anstalld_id, datum);
CREATE INDEX idx_shifts_schema ON scheduling.scheduled_shifts(schema_id);

-- Stämplingshändelser (kom-och-gå)
CREATE TABLE scheduling.time_clock_events (
    id UUID NOT NULL DEFAULT uuid_generate_v4(),
    anstalld_id UUID NOT NULL,
    typ VARCHAR(10) NOT NULL CHECK (typ IN ('In', 'Ut', 'Raststart', 'Rastslut')),
    tidpunkt TIMESTAMPTZ NOT NULL,
    kalla VARCHAR(20) NOT NULL CHECK (kalla IN ('Webbterminal', 'PWA', 'Manuell', 'Integration')),
    ip_adress INET,
    latitud DOUBLE PRECISION,
    longitud DOUBLE PRECISION,
    ar_offline_stampling BOOLEAN DEFAULT FALSE,
    synkad_vid TIMESTAMPTZ,
    kopplat_pass_id UUID,
    datum DATE NOT NULL GENERATED ALWAYS AS (tidpunkt::date) STORED,
    PRIMARY KEY (id, datum)
) PARTITION BY RANGE (datum);

CREATE TABLE scheduling.time_clock_events_2026_03 PARTITION OF scheduling.time_clock_events FOR VALUES FROM ('2026-03-01') TO ('2026-04-01');
CREATE TABLE scheduling.time_clock_events_2026_04 PARTITION OF scheduling.time_clock_events FOR VALUES FROM ('2026-04-01') TO ('2026-05-01');
CREATE TABLE scheduling.time_clock_events_2026_05 PARTITION OF scheduling.time_clock_events FOR VALUES FROM ('2026-05-01') TO ('2026-06-01');
CREATE TABLE scheduling.time_clock_events_2026_06 PARTITION OF scheduling.time_clock_events FOR VALUES FROM ('2026-06-01') TO ('2026-07-01');

CREATE INDEX idx_clock_events_anstalld ON scheduling.time_clock_events(anstalld_id, tidpunkt);

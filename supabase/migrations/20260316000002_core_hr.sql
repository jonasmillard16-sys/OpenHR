-- Core HR: Personalregister, organisation, anställningar

-- Organisationsstruktur (trädstruktur)
CREATE TABLE core_hr.organization_units (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    namn VARCHAR(200) NOT NULL,
    typ VARCHAR(50) NOT NULL CHECK (typ IN ('Region', 'Forvaltning', 'Verksamhet', 'Enhet', 'Avdelning', 'Sektion')),
    overordnad_enhet_id UUID REFERENCES core_hr.organization_units(id),
    kostnadsstalle VARCHAR(20) NOT NULL,
    cfar_kod VARCHAR(10),
    chef_id UUID,  -- FK till employees, sätts efter employees skapats
    giltig_fran DATE NOT NULL,
    giltig_till DATE,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ,
    created_by VARCHAR(100),
    updated_by VARCHAR(100)
);

CREATE INDEX idx_org_units_parent ON core_hr.organization_units(overordnad_enhet_id);
CREATE INDEX idx_org_units_kostnadsstalle ON core_hr.organization_units(kostnadsstalle);

-- Anställda
CREATE TABLE core_hr.employees (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    -- Personnummer krypterat med pgcrypto
    personnummer_encrypted BYTEA NOT NULL,
    personnummer_hash VARCHAR(64) NOT NULL UNIQUE, -- SHA-256 för snabb sökning
    fornamn VARCHAR(100) NOT NULL,
    efternamn VARCHAR(100) NOT NULL,
    mellan_namn VARCHAR(100),

    -- Kontakt
    epost VARCHAR(200),
    telefon VARCHAR(30),
    gatuadress VARCHAR(200),
    postnummer VARCHAR(10),
    ort VARCHAR(100),
    land VARCHAR(50) DEFAULT 'Sverige',

    -- Bankuppgifter (krypterade)
    clearingnummer_encrypted BYTEA,
    kontonummer_encrypted BYTEA,

    -- Skatteuppgifter
    skattetabell INTEGER CHECK (skattetabell BETWEEN 29 AND 40),
    skattekolumn INTEGER CHECK (skattekolumn BETWEEN 1 AND 6),
    kommun VARCHAR(50),
    kommunal_skattesats DECIMAL(5,2),
    har_kyrkoavgift BOOLEAN DEFAULT FALSE,
    kyrkoavgiftssats DECIMAL(5,3),
    har_jamkning BOOLEAN DEFAULT FALSE,
    jamkning_belopp DECIMAL(12,2),

    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ,
    created_by VARCHAR(100),
    updated_by VARCHAR(100)
);

CREATE INDEX idx_employees_personnummer_hash ON core_hr.employees(personnummer_hash);
CREATE INDEX idx_employees_namn ON core_hr.employees USING gin ((fornamn || ' ' || efternamn) gin_trgm_ops);

-- Anställningar
CREATE TABLE core_hr.employments (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    anstalld_id UUID NOT NULL REFERENCES core_hr.employees(id),
    enhet_id UUID NOT NULL REFERENCES core_hr.organization_units(id),
    anstallningsform VARCHAR(30) NOT NULL CHECK (anstallningsform IN ('Tillsvidare', 'Vikariat', 'Provanstallning', 'SAVA', 'Timavlonad', 'Sasongsanstallning')),
    kollektivavtal VARCHAR(10) NOT NULL CHECK (kollektivavtal IN ('AB', 'HOK', 'MBA', 'PAN', 'None')),
    manadslon DECIMAL(12,2) NOT NULL CHECK (manadslon >= 0),
    sysselsattningsgrad DECIMAL(5,2) NOT NULL CHECK (sysselsattningsgrad BETWEEN 0 AND 100),
    start_datum DATE NOT NULL,
    slut_datum DATE,
    besta_kod VARCHAR(10),
    aid_kod VARCHAR(10),
    befattningstitel VARCHAR(200),

    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ,
    created_by VARCHAR(100),
    updated_by VARCHAR(100),

    CONSTRAINT chk_employment_dates CHECK (slut_datum IS NULL OR slut_datum >= start_datum)
);

CREATE INDEX idx_employments_anstalld ON core_hr.employments(anstalld_id);
CREATE INDEX idx_employments_enhet ON core_hr.employments(enhet_id);
CREATE INDEX idx_employments_dates ON core_hr.employments(start_datum, slut_datum);
CREATE INDEX idx_employments_form ON core_hr.employments(anstallningsform);

-- Kollektivavtal (versionerad konfiguration i JSONB)
CREATE TABLE core_hr.collective_agreements (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    kod VARCHAR(10) NOT NULL,
    version VARCHAR(20) NOT NULL,
    giltig_fran DATE NOT NULL,
    giltig_till DATE,
    regler JSONB NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    UNIQUE(kod, version)
);

-- FK: Chef i organization_units refererar employees
ALTER TABLE core_hr.organization_units
    ADD CONSTRAINT fk_org_unit_chef FOREIGN KEY (chef_id) REFERENCES core_hr.employees(id);

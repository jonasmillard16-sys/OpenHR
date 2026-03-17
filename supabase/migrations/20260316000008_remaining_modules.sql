-- Salary Review, Travel, Recruitment, Integration Hub

-- === Salary Review ===
CREATE TABLE salary_review.rounds (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    namn VARCHAR(200) NOT NULL,
    ar INTEGER NOT NULL,
    avtalsomrade VARCHAR(10) NOT NULL,
    total_budget DECIMAL(14,2) NOT NULL,
    fordelad_budget DECIMAL(14,2) DEFAULT 0,
    status VARCHAR(30) NOT NULL DEFAULT 'Planering',
    ikrafttradande_datum DATE NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE salary_review.proposals (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    round_id UUID NOT NULL REFERENCES salary_review.rounds(id),
    anstalld_id UUID NOT NULL,
    nuvarande_lon DECIMAL(12,2) NOT NULL,
    foreslagen_lon DECIMAL(12,2) NOT NULL,
    okning DECIMAL(12,2) NOT NULL,
    okning_procent DECIMAL(6,2),
    motivering TEXT
);

-- === Travel & Expenses ===
CREATE TABLE travel.claims (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    anstalld_id UUID NOT NULL,
    beskrivning VARCHAR(500),
    rese_datum DATE NOT NULL,
    status VARCHAR(20) NOT NULL DEFAULT 'Utkast',
    total_belopp DECIMAL(12,2) DEFAULT 0,
    hela_dagar INTEGER,
    halva_dagar INTEGER,
    traktamente DECIMAL(12,2),
    korda_mil DECIMAL(10,2),
    milersattning DECIMAL(12,2),
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE travel.expense_items (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    claim_id UUID NOT NULL REFERENCES travel.claims(id),
    beskrivning VARCHAR(500) NOT NULL,
    belopp DECIMAL(12,2) NOT NULL,
    kvitto_bild_id VARCHAR(500)
);

-- === Recruitment ===
CREATE TABLE recruitment.vacancies (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    enhet_id UUID NOT NULL,
    titel VARCHAR(200) NOT NULL,
    beskrivning TEXT,
    anstallningsform VARCHAR(30) NOT NULL,
    lonespann_min DECIMAL(12,2),
    lonespann_max DECIMAL(12,2),
    sista_ansoknings_dag DATE NOT NULL,
    status VARCHAR(20) NOT NULL DEFAULT 'Utkast',
    publicerad_externt BOOLEAN DEFAULT FALSE,
    publicerad_platsbanken BOOLEAN DEFAULT FALSE,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE recruitment.applications (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    vacancy_id UUID NOT NULL REFERENCES recruitment.vacancies(id),
    namn VARCHAR(200) NOT NULL,
    epost VARCHAR(200) NOT NULL,
    cv_fil_id VARCHAR(500),
    status VARCHAR(30) NOT NULL DEFAULT 'Mottagen',
    poang INTEGER,
    inkom_vid TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- === Integration Hub ===
CREATE TABLE integration_hub.outbox_messages (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    destination VARCHAR(100) NOT NULL,
    message_type VARCHAR(100) NOT NULL,
    payload JSONB NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    processed_at TIMESTAMPTZ,
    retry_count INTEGER DEFAULT 0,
    max_retries INTEGER DEFAULT 5,
    next_retry_at TIMESTAMPTZ,
    last_error TEXT,
    status VARCHAR(20) NOT NULL DEFAULT 'Pending'
);

CREATE INDEX idx_outbox_pending ON integration_hub.outbox_messages(status, next_retry_at) WHERE status = 'Pending';

CREATE TABLE integration_hub.integration_logs (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    system_name VARCHAR(100) NOT NULL,
    operation VARCHAR(100) NOT NULL,
    direction VARCHAR(10) NOT NULL CHECK (direction IN ('In', 'Ut')),
    status VARCHAR(20) NOT NULL,
    payload_summary TEXT,
    error_message TEXT,
    tidpunkt TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    duration_ms INTEGER
);

CREATE INDEX idx_integration_logs_system ON integration_hub.integration_logs(system_name, tidpunkt);

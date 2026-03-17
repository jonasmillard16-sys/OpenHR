-- Case Management: Ärenden, workflows, frånvaro

CREATE TABLE case_mgmt.cases (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    typ VARCHAR(30) NOT NULL CHECK (typ IN ('Franvaro', 'Anstallningsandring', 'Lonandring', 'Omplacering', 'Rehabilitering', 'LAS')),
    anstalld_id UUID NOT NULL,
    status VARCHAR(30) NOT NULL DEFAULT 'Oppnad',
    beskrivning TEXT,
    aktuellt_steg VARCHAR(100),
    tilldelad_till UUID,
    slutford_vid TIMESTAMPTZ,
    franvaro_data JSONB,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ
);

CREATE INDEX idx_cases_anstalld ON case_mgmt.cases(anstalld_id);
CREATE INDEX idx_cases_status ON case_mgmt.cases(status);
CREATE INDEX idx_cases_typ ON case_mgmt.cases(typ);

CREATE TABLE case_mgmt.case_approvals (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    case_id UUID NOT NULL REFERENCES case_mgmt.cases(id),
    steg VARCHAR(100) NOT NULL,
    godkannare_id UUID NOT NULL,
    status VARCHAR(20) NOT NULL DEFAULT 'Vantar',
    beslut_vid TIMESTAMPTZ,
    kommentar TEXT
);

CREATE INDEX idx_approvals_case ON case_mgmt.case_approvals(case_id);
CREATE INDEX idx_approvals_godkannare ON case_mgmt.case_approvals(godkannare_id, status);

CREATE TABLE case_mgmt.case_comments (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    case_id UUID NOT NULL REFERENCES case_mgmt.cases(id),
    forfattare_id UUID NOT NULL,
    text TEXT NOT NULL,
    skapad_vid TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE case_mgmt.workflow_definitions (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    arende_typ VARCHAR(30) NOT NULL UNIQUE,
    steg JSONB NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

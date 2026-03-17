-- HälsoSAM: Rehabilitering och sjukfrånvarobevakning

CREATE TABLE halsosam.rehab_cases (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    anstalld_id UUID NOT NULL,
    trigger_typ VARCHAR(50) NOT NULL,
    status VARCHAR(30) NOT NULL DEFAULT 'Signal',
    arendeagare_hr UUID,
    skapad_vid TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    rehab_plan TEXT,
    uppfoljning_14_dagar TIMESTAMPTZ,
    uppfoljning_90_dagar TIMESTAMPTZ,
    uppfoljning_180_dagar TIMESTAMPTZ,
    uppfoljning_365_dagar TIMESTAMPTZ
);

CREATE INDEX idx_rehab_anstalld ON halsosam.rehab_cases(anstalld_id);
CREATE INDEX idx_rehab_status ON halsosam.rehab_cases(status);

CREATE TABLE halsosam.rehab_notes (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    rehab_case_id UUID NOT NULL REFERENCES halsosam.rehab_cases(id),
    text TEXT NOT NULL,
    forfattare_id UUID NOT NULL,
    skapad_vid TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- Läkarintygslagring (metadata - filen i Blob Storage)
CREATE TABLE halsosam.medical_certificates (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    rehab_case_id UUID NOT NULL REFERENCES halsosam.rehab_cases(id),
    anstalld_id UUID NOT NULL,
    fil_referens VARCHAR(500) NOT NULL,
    uppladdad_av UUID NOT NULL,
    uppladdad_vid TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    giltig_fran DATE,
    giltig_till DATE,
    gallrings_datum DATE NOT NULL  -- GDPR: rehabärende + 2 år
);

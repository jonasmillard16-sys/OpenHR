-- Payroll: Löneberäkning, skattetabeller

-- Skattetabeller (laddas årligen från Skatteverket)
CREATE TABLE payroll.tax_tables (
    id SERIAL PRIMARY KEY,
    ar INTEGER NOT NULL,
    tabellnummer INTEGER NOT NULL,
    kolumn INTEGER NOT NULL CHECK (kolumn BETWEEN 1 AND 6),
    UNIQUE(ar, tabellnummer, kolumn)
);

CREATE TABLE payroll.tax_table_rows (
    id SERIAL PRIMARY KEY,
    tax_table_id INTEGER NOT NULL REFERENCES payroll.tax_tables(id) ON DELETE CASCADE,
    inkomst_fran DECIMAL(12,2) NOT NULL,
    inkomst_till DECIMAL(12,2) NOT NULL,
    skattebelopp DECIMAL(12,2) NOT NULL
);

CREATE INDEX idx_tax_rows_table ON payroll.tax_table_rows(tax_table_id);
CREATE INDEX idx_tax_rows_inkomst ON payroll.tax_table_rows(tax_table_id, inkomst_fran);

-- Lönearter
CREATE TABLE payroll.salary_codes (
    kod VARCHAR(10) PRIMARY KEY,
    benamning VARCHAR(200) NOT NULL,
    skattekategori VARCHAR(30) NOT NULL,
    ar_semestergrundande BOOLEAN DEFAULT FALSE,
    ar_pensionsgrundande BOOLEAN DEFAULT FALSE,
    ar_ob_grundande BOOLEAN DEFAULT FALSE,
    agi_faltkod VARCHAR(10),
    ar_avdrag BOOLEAN DEFAULT FALSE,
    ar_aktiv BOOLEAN DEFAULT TRUE
);

-- Lönekörningar
CREATE TABLE payroll.payroll_runs (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    ar INTEGER NOT NULL,
    manad INTEGER NOT NULL CHECK (manad BETWEEN 1 AND 12),
    status VARCHAR(20) NOT NULL DEFAULT 'Skapad',
    startad_vid TIMESTAMPTZ,
    avslutad_vid TIMESTAMPTZ,
    startad_av VARCHAR(100),
    godkand_av VARCHAR(100),
    antal_anstallda INTEGER DEFAULT 0,
    total_brutto DECIMAL(14,2) DEFAULT 0,
    total_netto DECIMAL(14,2) DEFAULT 0,
    total_skatt DECIMAL(14,2) DEFAULT 0,
    total_arbetsgivaravgifter DECIMAL(14,2) DEFAULT 0,
    ar_retroaktiv BOOLEAN DEFAULT FALSE,
    retroaktivt_for_period VARCHAR(7),
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    version INTEGER DEFAULT 1
) PARTITION BY RANGE (ar, manad);

-- Skapa partitioner för 2025-2027
CREATE TABLE payroll.payroll_runs_2025 PARTITION OF payroll.payroll_runs FOR VALUES FROM (2025, 1) TO (2026, 1);
CREATE TABLE payroll.payroll_runs_2026 PARTITION OF payroll.payroll_runs FOR VALUES FROM (2026, 1) TO (2027, 1);
CREATE TABLE payroll.payroll_runs_2027 PARTITION OF payroll.payroll_runs FOR VALUES FROM (2027, 1) TO (2028, 1);

-- Löneresultat per anställd (partitionerat per år/månad)
CREATE TABLE payroll.payroll_results (
    id UUID NOT NULL DEFAULT uuid_generate_v4(),
    kornings_id UUID NOT NULL,
    anstalld_id UUID NOT NULL,
    anstallnings_id UUID NOT NULL,
    ar INTEGER NOT NULL,
    manad INTEGER NOT NULL,
    manadslon DECIMAL(12,2),
    sysselsattningsgrad DECIMAL(5,2),
    kollektivavtal VARCHAR(10),
    brutto DECIMAL(12,2) DEFAULT 0,
    skatteplikt_brutto DECIMAL(12,2) DEFAULT 0,
    skatt DECIMAL(12,2) DEFAULT 0,
    netto DECIMAL(12,2) DEFAULT 0,
    arbetsgivaravgifter DECIMAL(12,2) DEFAULT 0,
    arbetsgivaravgift_sats DECIMAL(6,4),
    semesterlon DECIMAL(12,2) DEFAULT 0,
    semestertillagg DECIMAL(12,2) DEFAULT 0,
    semesterdagar_intjanade INTEGER DEFAULT 0,
    semesterdagar_uttagna INTEGER DEFAULT 0,
    pensionsgrundande DECIMAL(12,2) DEFAULT 0,
    pensionsavgift DECIMAL(12,2) DEFAULT 0,
    ob_tillagg DECIMAL(12,2) DEFAULT 0,
    overtidstillagg DECIMAL(12,2) DEFAULT 0,
    sjuklon DECIMAL(12,2) DEFAULT 0,
    karensavdrag DECIMAL(12,2) DEFAULT 0,
    loneutmatning DECIMAL(12,2) DEFAULT 0,
    fackavgift DECIMAL(12,2) DEFAULT 0,
    ovriga_avdrag DECIMAL(12,2) DEFAULT 0,
    PRIMARY KEY (id, ar, manad)
) PARTITION BY RANGE (ar, manad);

CREATE TABLE payroll.payroll_results_2025 PARTITION OF payroll.payroll_results FOR VALUES FROM (2025, 1) TO (2026, 1);
CREATE TABLE payroll.payroll_results_2026 PARTITION OF payroll.payroll_results FOR VALUES FROM (2026, 1) TO (2027, 1);
CREATE TABLE payroll.payroll_results_2027 PARTITION OF payroll.payroll_results FOR VALUES FROM (2027, 1) TO (2028, 1);

CREATE INDEX idx_payroll_results_anstalld ON payroll.payroll_results(anstalld_id, ar, manad);
CREATE INDEX idx_payroll_results_korning ON payroll.payroll_results(kornings_id);

-- Löneresultatrader (detaljer)
CREATE TABLE payroll.payroll_result_lines (
    id UUID NOT NULL DEFAULT uuid_generate_v4(),
    result_id UUID NOT NULL,
    ar INTEGER NOT NULL,
    manad INTEGER NOT NULL,
    loneart_kod VARCHAR(10) NOT NULL,
    benamning VARCHAR(200),
    antal DECIMAL(10,4),
    sats DECIMAL(12,2),
    belopp DECIMAL(12,2) NOT NULL,
    skattekategori VARCHAR(30),
    ar_semestergrundande BOOLEAN DEFAULT FALSE,
    ar_pensionsgrundande BOOLEAN DEFAULT FALSE,
    kostnadsstalle VARCHAR(20),
    projekt VARCHAR(20),
    agi_faltkod VARCHAR(10),
    PRIMARY KEY (id, ar, manad)
) PARTITION BY RANGE (ar, manad);

CREATE TABLE payroll.payroll_result_lines_2025 PARTITION OF payroll.payroll_result_lines FOR VALUES FROM (2025, 1) TO (2026, 1);
CREATE TABLE payroll.payroll_result_lines_2026 PARTITION OF payroll.payroll_result_lines FOR VALUES FROM (2026, 1) TO (2027, 1);
CREATE TABLE payroll.payroll_result_lines_2027 PARTITION OF payroll.payroll_result_lines FOR VALUES FROM (2027, 1) TO (2028, 1);

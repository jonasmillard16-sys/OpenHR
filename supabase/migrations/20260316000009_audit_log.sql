-- Audit: Gemensam revisionslogg (7 års retention)

CREATE TABLE audit.logs (
    id UUID NOT NULL DEFAULT uuid_generate_v4(),
    tidpunkt TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    schema_namn VARCHAR(50) NOT NULL,
    tabell_namn VARCHAR(100) NOT NULL,
    operation VARCHAR(10) NOT NULL CHECK (operation IN ('INSERT', 'UPDATE', 'DELETE')),
    anvandare VARCHAR(100),
    andamal VARCHAR(200),  -- GDPR: ändamål med åtkomsten
    gamla_varden JSONB,
    nya_varden JSONB,
    ip_adress INET,
    datum DATE NOT NULL GENERATED ALWAYS AS (tidpunkt::date) STORED,
    PRIMARY KEY (id, datum)
) PARTITION BY RANGE (datum);

-- Rullande partitioner per månad
CREATE TABLE audit.logs_2026_03 PARTITION OF audit.logs FOR VALUES FROM ('2026-03-01') TO ('2026-04-01');
CREATE TABLE audit.logs_2026_04 PARTITION OF audit.logs FOR VALUES FROM ('2026-04-01') TO ('2026-05-01');
CREATE TABLE audit.logs_2026_05 PARTITION OF audit.logs FOR VALUES FROM ('2026-05-01') TO ('2026-06-01');
CREATE TABLE audit.logs_2026_06 PARTITION OF audit.logs FOR VALUES FROM ('2026-06-01') TO ('2026-07-01');

CREATE INDEX idx_audit_logs_tabell ON audit.logs(schema_namn, tabell_namn, tidpunkt);
CREATE INDEX idx_audit_logs_anvandare ON audit.logs(anvandare, tidpunkt);

-- Audit trigger function
CREATE OR REPLACE FUNCTION audit.log_changes()
RETURNS TRIGGER AS $$
BEGIN
    INSERT INTO audit.logs (schema_namn, tabell_namn, operation, anvandare, gamla_varden, nya_varden)
    VALUES (
        TG_TABLE_SCHEMA,
        TG_TABLE_NAME,
        TG_OP,
        current_user,
        CASE WHEN TG_OP IN ('UPDATE', 'DELETE') THEN to_jsonb(OLD) ELSE NULL END,
        CASE WHEN TG_OP IN ('INSERT', 'UPDATE') THEN to_jsonb(NEW) ELSE NULL END
    );
    RETURN COALESCE(NEW, OLD);
END;
$$ LANGUAGE plpgsql;

-- Aktivera audit på känsliga tabeller
CREATE TRIGGER audit_employees
    AFTER INSERT OR UPDATE OR DELETE ON core_hr.employees
    FOR EACH ROW EXECUTE FUNCTION audit.log_changes();

CREATE TRIGGER audit_employments
    AFTER INSERT OR UPDATE OR DELETE ON core_hr.employments
    FOR EACH ROW EXECUTE FUNCTION audit.log_changes();

CREATE TRIGGER audit_payroll_results
    AFTER INSERT OR UPDATE OR DELETE ON payroll.payroll_results
    FOR EACH ROW EXECUTE FUNCTION audit.log_changes();

-- RegionHR: Schema-per-module isolation
-- Varje modul äger sitt eget PostgreSQL-schema

-- Aktivera extensions
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS "pgcrypto";
CREATE EXTENSION IF NOT EXISTS "pg_trgm";   -- Fuzzy text search

-- Skapa modulscheman
CREATE SCHEMA IF NOT EXISTS core_hr;
CREATE SCHEMA IF NOT EXISTS payroll;
CREATE SCHEMA IF NOT EXISTS scheduling;
CREATE SCHEMA IF NOT EXISTS case_mgmt;
CREATE SCHEMA IF NOT EXISTS las;
CREATE SCHEMA IF NOT EXISTS halsosam;
CREATE SCHEMA IF NOT EXISTS salary_review;
CREATE SCHEMA IF NOT EXISTS travel;
CREATE SCHEMA IF NOT EXISTS recruitment;
CREATE SCHEMA IF NOT EXISTS integration_hub;

-- Audit log (gemensam)
CREATE SCHEMA IF NOT EXISTS audit;

COMMENT ON SCHEMA core_hr IS 'Personalregister, organisation, anställningar';
COMMENT ON SCHEMA payroll IS 'Löneberäkning, skattetabeller, kollektivavtal';
COMMENT ON SCHEMA scheduling IS 'Schema, instämpling, bemanningsöversikt';
COMMENT ON SCHEMA case_mgmt IS 'Ärendehantering, workflows, frånvaro';
COMMENT ON SCHEMA las IS 'LAS-uppföljning, konvertering, turordning';
COMMENT ON SCHEMA halsosam IS 'Rehabilitering, sjukfrånvarobevakning';
COMMENT ON SCHEMA salary_review IS 'Löneöversynsrundor';
COMMENT ON SCHEMA travel IS 'Resor, utlägg, traktamente';
COMMENT ON SCHEMA recruitment IS 'Rekrytering, vakanser, ansökningar';
COMMENT ON SCHEMA integration_hub IS 'Integrationer, outbox, adaptrar';
COMMENT ON SCHEMA audit IS 'Revisionsloggar';

-- Row Level Security (RLS) för GDPR-efterlevnad
-- I produktion: kopplat till Azure AD-roller via JWT claims

-- Aktivera RLS på känsliga tabeller
ALTER TABLE core_hr.employees ENABLE ROW LEVEL SECURITY;
ALTER TABLE core_hr.employments ENABLE ROW LEVEL SECURITY;
ALTER TABLE payroll.payroll_results ENABLE ROW LEVEL SECURITY;
ALTER TABLE halsosam.rehab_cases ENABLE ROW LEVEL SECURITY;
ALTER TABLE halsosam.medical_certificates ENABLE ROW LEVEL SECURITY;

-- Policy: Anställd ser sin egen data
CREATE POLICY employees_own_data ON core_hr.employees
    FOR SELECT
    USING (
        id = auth.uid()
        OR auth.jwt() ->> 'role' IN ('HR-admin', 'Loneadmin', 'Systemadmin')
    );

-- Policy: Chef ser sin enhets anställda
CREATE POLICY employments_unit_access ON core_hr.employments
    FOR SELECT
    USING (
        anstalld_id = auth.uid()
        OR enhet_id IN (
            SELECT id FROM core_hr.organization_units WHERE chef_id = auth.uid()
        )
        OR auth.jwt() ->> 'role' IN ('HR-admin', 'Loneadmin', 'Systemadmin')
    );

-- Policy: Löneresultat - enbart egen + löneadmin
CREATE POLICY payroll_results_access ON payroll.payroll_results
    FOR SELECT
    USING (
        anstalld_id = auth.uid()
        OR auth.jwt() ->> 'role' IN ('Loneadmin', 'Systemadmin')
    );

-- Policy: Läkarintyg - strikt åtkomst
CREATE POLICY medical_certs_access ON halsosam.medical_certificates
    FOR SELECT
    USING (
        anstalld_id = auth.uid()
        OR id IN (
            SELECT mc.id FROM halsosam.medical_certificates mc
            JOIN halsosam.rehab_cases rc ON mc.rehab_case_id = rc.id
            WHERE rc.arendeagare_hr = auth.uid()
        )
        OR auth.jwt() ->> 'role' = 'Systemadmin'
    );

-- Policy: Rehabärenden - ärendeägare + anställd
CREATE POLICY rehab_cases_access ON halsosam.rehab_cases
    FOR SELECT
    USING (
        anstalld_id = auth.uid()
        OR arendeagare_hr = auth.uid()
        OR auth.jwt() ->> 'role' IN ('HR-admin', 'Systemadmin')
    );

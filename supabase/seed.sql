-- Seed data för utvecklingsmiljö
-- OBS: Använd INTE verkliga personnummer

-- Testorganisation
INSERT INTO core_hr.organization_units (id, namn, typ, kostnadsstalle, giltig_fran) VALUES
    ('a0000000-0000-0000-0000-000000000001', 'Västra Götalandsregionen', 'Region', '0000', '2020-01-01'),
    ('a0000000-0000-0000-0000-000000000002', 'Sahlgrenska Universitetssjukhuset', 'Forvaltning', '1000', '2020-01-01'),
    ('a0000000-0000-0000-0000-000000000003', 'Medicinkliniken', 'Verksamhet', '1100', '2020-01-01'),
    ('a0000000-0000-0000-0000-000000000004', 'Avdelning 32', 'Avdelning', '1132', '2020-01-01');

UPDATE core_hr.organization_units SET overordnad_enhet_id = 'a0000000-0000-0000-0000-000000000001' WHERE id = 'a0000000-0000-0000-0000-000000000002';
UPDATE core_hr.organization_units SET overordnad_enhet_id = 'a0000000-0000-0000-0000-000000000002' WHERE id = 'a0000000-0000-0000-0000-000000000003';
UPDATE core_hr.organization_units SET overordnad_enhet_id = 'a0000000-0000-0000-0000-000000000003' WHERE id = 'a0000000-0000-0000-0000-000000000004';

-- Standardlönearter
INSERT INTO payroll.salary_codes (kod, benamning, skattekategori, ar_semestergrundande, ar_pensionsgrundande, agi_faltkod) VALUES
    ('1100', 'Månadslön', 'Skattepliktig', true, true, '011'),
    ('1310', 'OB-tillägg', 'Skattepliktig', true, true, '011'),
    ('1410', 'Enkel övertid', 'Skattepliktig', false, true, '011'),
    ('1420', 'Kvalificerad övertid', 'Skattepliktig', false, true, '011'),
    ('2700', 'Semesterlön', 'Skattepliktig', false, true, '011'),
    ('2710', 'Semesterlöneavdrag', 'Skattepliktig', false, false, '011'),
    ('2720', 'Semestertillägg', 'Skattepliktig', false, true, '011'),
    ('3001', 'Karensavdrag', 'Skattepliktig', false, false, '011'),
    ('3010', 'Sjuklön dag 2-14', 'Skattepliktig', true, true, '011'),
    ('5100', 'Inrikes traktamente', 'Traktamente', false, false, '050'),
    ('5200', 'Milersättning', 'Milersattning', false, false, '051');

-- Workflow-definitioner
INSERT INTO case_mgmt.workflow_definitions (arende_typ, steg) VALUES
    ('Franvaro', '[{"namn": "Inskickat", "ordning": 1, "godkannare_roll": "Anstalld"}, {"namn": "Chefsgodkannande", "ordning": 2, "godkannare_roll": "Chef"}, {"namn": "Verkstallande", "ordning": 3, "godkannare_roll": "System"}]'),
    ('Lonandring', '[{"namn": "Inskickat", "ordning": 1, "godkannare_roll": "Chef"}, {"namn": "HR-granskning", "ordning": 2, "godkannare_roll": "HR"}, {"namn": "Lonegodkannande", "ordning": 3, "godkannare_roll": "Loneadmin"}, {"namn": "Verkstallande", "ordning": 4, "godkannare_roll": "System"}]'),
    ('Anstallningsandring', '[{"namn": "Inskickat", "ordning": 1, "godkannare_roll": "Chef"}, {"namn": "HR-granskning", "ordning": 2, "godkannare_roll": "HR"}, {"namn": "Verkstallande", "ordning": 3, "godkannare_roll": "System"}]');

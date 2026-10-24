-- Synthetic demo data for docker compose only; integration tests seed their own data.
-- No real personal data: names are placeholders and dates of birth are left empty.
SET NOCOUNT ON;

IF EXISTS (SELECT 1 FROM Schools WHERE SchoolName IN ('Maple Grove Demo School', 'Cedar Ridge Demo School'))
BEGIN
    PRINT 'Demo data already present; skipping.';
    RETURN;
END;

DECLARE @mapleGrove INT, @cedarRidge INT;

-- A null threshold means the default of 10 applies.
INSERT INTO Schools (SchoolName, Active, AbsenceAlertThreshold, TimeZone)
VALUES ('Maple Grove Demo School', 1, NULL, 'America/Toronto');
SET @mapleGrove = SCOPE_IDENTITY();

INSERT INTO Schools (SchoolName, Active, AbsenceAlertThreshold, TimeZone)
VALUES ('Cedar Ridge Demo School', 1, 5, 'America/Vancouver');
SET @cedarRidge = SCOPE_IDENTITY();

-- Breaks between terms belong to the school year; the summer gap belongs to no year.
INSERT INTO SchoolTerms (SchoolID, SchoolYear, TermName, StartDate, EndDate) VALUES
    (@mapleGrove, '2025-2026', 'Fall 2025',          '2025-09-02', '2025-12-19'),
    (@mapleGrove, '2025-2026', 'Winter 2026',        '2026-01-05', '2026-03-13'),
    (@mapleGrove, '2025-2026', 'Spring 2026',        '2026-03-23', '2026-06-26'),
    (@mapleGrove, '2026-2027', 'Fall 2026',          '2026-09-08', '2026-12-18'),
    (@mapleGrove, '2026-2027', 'Winter 2027',        '2027-01-04', '2027-03-12'),
    (@mapleGrove, '2026-2027', 'Spring 2027',        '2027-03-22', '2027-06-25'),
    (@cedarRidge, '2025-2026', 'Semester 1 2025-26', '2025-09-02', '2026-01-30'),
    (@cedarRidge, '2025-2026', 'Semester 2 2025-26', '2026-02-09', '2026-06-26'),
    (@cedarRidge, '2026-2027', 'Semester 1 2026-27', '2026-09-08', '2027-01-29'),
    (@cedarRidge, '2026-2027', 'Semester 2 2026-27', '2027-02-08', '2027-06-25');

INSERT INTO Students (SchoolID, FirstName, LastName, Grade, DateOfBirth, Active) VALUES
    (@mapleGrove, 'Alpha',   'Demo', '5', NULL, 1),
    (@mapleGrove, 'Bravo',   'Demo', '5', NULL, 1),
    (@mapleGrove, 'Charlie', 'Demo', '6', NULL, 1),
    (@mapleGrove, 'Delta',   'Demo', '6', NULL, 0),
    (@cedarRidge, 'Echo',    'Demo', '7', NULL, 1),
    (@cedarRidge, 'Foxtrot', 'Demo', '7', NULL, 1),
    (@cedarRidge, 'Golf',    'Demo', '8', NULL, 1);
GO

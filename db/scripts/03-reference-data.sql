-- Attendance codes (decision #24). Re-runnable: existing codes are left as they are.
INSERT INTO AttendanceCodes (CodeValue, Description, IsAbsent, IsExcused, IsActive)
SELECT v.CodeValue, v.Description, v.IsAbsent, v.IsExcused, 1
FROM (VALUES
    ('P', 'Present',         0, 0),
    ('A', 'Absent',          1, 0),
    ('E', 'Excused absence', 1, 1),
    ('L', 'Late',            0, 0)
) AS v (CodeValue, Description, IsAbsent, IsExcused)
WHERE NOT EXISTS (SELECT 1 FROM AttendanceCodes AS c WHERE c.CodeValue = v.CodeValue);
GO

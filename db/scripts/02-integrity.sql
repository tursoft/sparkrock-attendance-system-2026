-- Integrity backstops for the domain rules (decision #23) and column changes to legacy tables. Runs after schema.sql and 01-missing-tables.sql.

-- Filtered indexes require these options; sqlcmd defaults QUOTED_IDENTIFIER to OFF.
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET NOCOUNT ON;
GO

-- schema.sql leaves its primary keys and the CodeValue unique key system-named (random per database).
-- Fixed names keep the scaffolded EF model identical whichever database it is scaffolded from.
DECLARE @renames TABLE (TableName SYSNAME, ConstraintType CHAR(2), NewName SYSNAME);
INSERT INTO @renames (TableName, ConstraintType, NewName) VALUES
    ('Students',                 'PK', 'PK_Students'),
    ('AttendanceCodes',          'PK', 'PK_AttendanceCodes'),
    ('AttendanceCodes',          'UQ', 'UQ_AttendanceCodes_CodeValue'),
    ('StudentAttendance',        'PK', 'PK_StudentAttendance'),
    ('StudentAttendanceSummary', 'PK', 'PK_StudentAttendanceSummary'),
    ('StudentAlerts',            'PK', 'PK_StudentAlerts');

DECLARE @oldName NVARCHAR(400), @newName SYSNAME;
DECLARE renameCursor CURSOR LOCAL FAST_FORWARD FOR
    SELECT QUOTENAME(SCHEMA_NAME(k.schema_id)) + N'.' + QUOTENAME(k.name), r.NewName
    FROM @renames AS r
    JOIN sys.key_constraints AS k
        ON k.parent_object_id = OBJECT_ID(r.TableName) AND k.type COLLATE DATABASE_DEFAULT = r.ConstraintType
    WHERE k.name COLLATE DATABASE_DEFAULT <> r.NewName;
OPEN renameCursor;
FETCH NEXT FROM renameCursor INTO @oldName, @newName;
WHILE @@FETCH_STATUS = 0
BEGIN
    EXEC sp_rename @objname = @oldName, @newname = @newName, @objtype = 'OBJECT';
    FETCH NEXT FROM renameCursor INTO @oldName, @newName;
END;
CLOSE renameCursor;
DEALLOCATE renameCursor;
GO

-- One summary per student, school and school year (#36, BUG-10)
ALTER TABLE StudentAttendanceSummary DROP CONSTRAINT UQ_Summary;
ALTER TABLE StudentAttendanceSummary
    ADD CONSTRAINT UQ_StudentAttendanceSummary_StudentID_SchoolID_SchoolYear UNIQUE (StudentID, SchoolID, SchoolYear);
GO

-- One record per student per day (BUG-6, V13); also serves history date-range queries (BUG-15)
CREATE UNIQUE INDEX UX_StudentAttendance_StudentID_AttendDate
    ON StudentAttendance (StudentID, AttendDate);

-- At most one open alert per student, school, school year and type, even under concurrency (#37)
CREATE UNIQUE INDEX UX_StudentAlerts_Open
    ON StudentAlerts (StudentID, SchoolID, SchoolYear, AlertType)
    WHERE ResolvedDate IS NULL;

-- School-year range lookups (#30)
CREATE INDEX IX_SchoolTerms_SchoolID_SchoolYear
    ON SchoolTerms (SchoolID, SchoolYear)
    INCLUDE (StartDate, EndDate);
GO

-- Optimistic concurrency tokens (#41)
ALTER TABLE StudentAttendance ADD RowVersion ROWVERSION NOT NULL;
ALTER TABLE StudentAttendanceSummary ADD RowVersion ROWVERSION NOT NULL;
GO

ALTER TABLE Students
    ADD CONSTRAINT FK_Students_Schools FOREIGN KEY (SchoolID) REFERENCES Schools (SchoolID);

ALTER TABLE SchoolTerms
    ADD CONSTRAINT FK_SchoolTerms_Schools FOREIGN KEY (SchoolID) REFERENCES Schools (SchoolID);

ALTER TABLE StudentAttendance
    ADD CONSTRAINT FK_StudentAttendance_Schools FOREIGN KEY (SchoolID) REFERENCES Schools (SchoolID),
        CONSTRAINT FK_StudentAttendance_Students FOREIGN KEY (StudentID) REFERENCES Students (StudentID),
        CONSTRAINT FK_StudentAttendance_SchoolTerms FOREIGN KEY (TermID) REFERENCES SchoolTerms (TermID),
        -- History can't hide records with unknown codes (BUG-8)
        CONSTRAINT FK_StudentAttendance_AttendanceCodes FOREIGN KEY (AttendCode) REFERENCES AttendanceCodes (CodeValue);

ALTER TABLE StudentAttendanceSummary
    ADD CONSTRAINT FK_StudentAttendanceSummary_Schools FOREIGN KEY (SchoolID) REFERENCES Schools (SchoolID),
        CONSTRAINT FK_StudentAttendanceSummary_Students FOREIGN KEY (StudentID) REFERENCES Students (StudentID);

ALTER TABLE StudentAlerts
    ADD CONSTRAINT FK_StudentAlerts_Schools FOREIGN KEY (SchoolID) REFERENCES Schools (SchoolID),
        CONSTRAINT FK_StudentAlerts_Students FOREIGN KEY (StudentID) REFERENCES Students (StudentID);

ALTER TABLE AttendanceSubmissionLog
    ADD CONSTRAINT FK_AttendanceSubmissionLog_Schools FOREIGN KEY (SchoolID) REFERENCES Schools (SchoolID);
GO

ALTER TABLE StudentAttendance
    ADD CONSTRAINT CK_StudentAttendance_MinutesLate CHECK (MinutesLate >= 0);

ALTER TABLE SchoolTerms
    ADD CONSTRAINT CK_SchoolTerms_EndDate CHECK (EndDate >= StartDate);

-- Null means the default threshold of 10 (#59)
ALTER TABLE Schools
    ADD CONSTRAINT CK_Schools_AbsenceAlertThreshold CHECK (AbsenceAlertThreshold IS NULL OR AbsenceAlertThreshold >= 1);
GO

-- Notes keep any Unicode text instead of silently turning non-Latin-1 characters into '?' (#75)
ALTER TABLE StudentAttendance
    ALTER COLUMN Notes NVARCHAR(500) NULL;
GO

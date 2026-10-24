-- Tables the legacy code references but whose DDL is missing (MIS-1, decision #22),
-- written from the columns inferred in docs/01.legacy-analysis.md §1.
-- Foreign keys, indexes and checks are in 02-integrity.sql.

CREATE TABLE Schools (
    SchoolID              INT          IDENTITY(1,1) NOT NULL,
    SchoolName            VARCHAR(100) NOT NULL,
    Active                BIT          NOT NULL CONSTRAINT DF_Schools_Active DEFAULT 1,
    AbsenceAlertThreshold INT          NULL,
    -- IANA time zone id, e.g. 'America/Toronto' (#29)
    TimeZone              VARCHAR(64)  NOT NULL CONSTRAINT DF_Schools_TimeZone DEFAULT 'UTC',
    CONSTRAINT PK_Schools PRIMARY KEY (SchoolID)
);
GO

CREATE TABLE SchoolTerms (
    TermID     INT         IDENTITY(1,1) NOT NULL,
    SchoolID   INT         NOT NULL,
    -- 'YYYY-YYYY'; a school year spans its earliest term start to its latest term end (#30)
    SchoolYear VARCHAR(9)  NOT NULL,
    TermName   VARCHAR(50) NOT NULL,
    StartDate  DATE        NOT NULL,
    EndDate    DATE        NOT NULL,
    CONSTRAINT PK_SchoolTerms PRIMARY KEY (TermID)
);
GO

CREATE TABLE AttendanceSubmissionLog (
    SubmissionID  INT          IDENTITY(1,1) NOT NULL,
    SchoolID      INT          NOT NULL,
    SubmittedDate DATETIME     NOT NULL,
    AttendDate    DATE         NOT NULL,
    RecordCount   INT          NOT NULL,
    SubmittedBy   VARCHAR(100) NULL,
    CONSTRAINT PK_AttendanceSubmissionLog PRIMARY KEY (SubmissionID)
);
GO

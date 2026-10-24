-- Runs against master, before legacy/Database/schema.sql (which has no USE statement).
IF DB_ID(N'Attendance') IS NULL
    CREATE DATABASE Attendance;
GO

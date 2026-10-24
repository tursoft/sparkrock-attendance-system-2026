# Attendance System

A port of a legacy student attendance app (a VB6 form plus SQL Server stored procedures) to a .NET REST API, limited to three use cases ([scope](docs/0.scope.md)). `legacy/` holds the original source and is read-only. Rules, bugs and ambiguities are analysed in [docs/01.legacy-analysis.md](docs/01.legacy-analysis.md).

| Use case | Endpoint |
|----------|----------|
| Save daily attendance for a set of students | `PUT /schools/{schoolId}/attendance/{date}` |
| Student attendance history (paged, newest first) | `GET /students/{studentId}/attendance` |
| Chronic absenteeism status | `GET /students/{studentId}/absence-summary` |

## Key architecture decisions

- **DDD layers:** `Domain` (rules), `Application` (use cases, DTOs), `Infrastructure` (EF Core), `Api` (Minimal API); dependencies point inward.
- **REST:** DTO records only, RFC 7807 `ProblemDetails` errors, validation at the boundary, OpenAPI for every endpoint.
- **Database-first SQL Server:** the legacy schema plus new scripts in `db/scripts`; EF Core is scaffolded from it, and stored procedure logic is ported to LINQ.
- **Atomic writes:** one `SaveChangesAsync` (one transaction) per save; a unique `(StudentID, AttendDate)` index and `rowversion` turn races into `409`.
- **Legacy bugs fixed on purpose:** SQL and XML injection, writes to the wrong student, absences counted across years, partial saves.
- **Operations:** global exception handler, structured logs to stdout, `/health`, Mapperly mapping; stateless and configured by environment variables.
- **TDD:** unit tests for the domain and use cases; integration tests through `WebApplicationFactory` against SQL Server in Testcontainers.
- **No authentication** in this version; endpoints sit in route groups, so it can be added in one place.

## Ambiguities and how we addressed them

| ID | Ambiguity | Resolution |
|----|-----------|------------|
| AMB-1 | Missing student-list procedure; meaning of an empty grade | Out of scope: the client sends explicit student IDs |
| AMB-2 | Missing `Schools`, `SchoolTerms`, submission log, `SchoolYear()` | DDL written from the inferred columns; terms replace `SchoolYear()` |
| AMB-3 | Grade or class/homeroom grouping | Out of scope: no student lists, so no grouping |
| AMB-4 | School year: fixed September 1 or terms | Terms: a year runs from its first term start to its last term end |
| AMB-5 | Do excused absences count | Yes, every absent code counts, as before; late (`L`) is present |
| AMB-6 | Unknown or inactive codes | Rejected with `400` (the legacy app saved them as present) |
| AMB-7 | Blank code means present | No: `code` is required, and only the entries sent are saved |
| AMB-8 | Concurrent edits | A resave overwrites (last write wins); true races return `409` |
| AMB-9 | Allowed dates | Today or earlier in the school's time zone; outside every school year is `400` |
| AMB-10 | Minutes late tied to a late code | No: optional, 0–600, default 0 |
| AMB-11 | Alert lifecycle | No auto-resolve; a new alert may follow a manual resolution |
| AMB-12 | Absent/excused flags: snapshot or live | Snapshot of the code at save time |
| AMB-13 | Consumer of `sp_GetStudentAttendance` | Becomes the history and status endpoints |
| AMB-14 | Summary per student-year or per school | Per student, school and year (fixes BUG-10) |
| AMB-15 | Audit user without authentication | The `Attendance:AuditUser` setting |
| AMB-16 | Printed report | Out of scope |
| AMB-17 | Inactive students | Student must exist, be active and belong to the school, else `400` |
| AMB-18 | Time zone for "today" | New `Schools.TimeZone` column (IANA id, default `UTC`) |
| AMB-19 | Threshold per school or per year | Open: kept per school, default 10, as before |
| AMB-20 | Who maintains master data | Out of scope: seeded by SQL scripts |

## How to run

Requires Docker Desktop; the .NET SDK is only needed for tests or running outside Docker.

```bash
cp .env.example .env         # set the SQL Server password and audit user
docker compose up --build    # SQL Server, schema and demo data, then the API
dotnet test                  # unit and integration tests (Docker must be running)
```

The API runs at <http://localhost:8080>, with OpenAPI at `/openapi/v1.json` and health at `/health`. `docker compose down -v` resets the database.

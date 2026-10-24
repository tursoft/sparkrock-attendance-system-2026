# Attendance System

A port of a legacy VB6 + SQL Server attendance app to a .NET REST API, limited to [three use cases](docs/0.scope.md). `legacy/` holds the read-only original.

- `PUT /schools/{schoolId}/attendance/{date}`: save a day's attendance for a set of students
- `GET /students/{studentId}/attendance`: attendance history, paged, newest first
- `GET /students/{studentId}/absence-summary`: chronic absenteeism status

**Docs:** [analysis](docs/01.legacy-analysis.md) · [plan](docs/02.implementation-plan.md) · [decisions](docs/decisions.md) · [AI log](docs/ai-logs.md). **Status:** planned; the code is built in the plan's phases.

## Key architecture decisions

| Decision | Why |
|----------|-----|
| REST API in front of the database | Legacy clients queried SQL Server directly (SEC-5) |
| DDD layers: `Domain`, `Application`, `Infrastructure`, `Api`, dependencies pointing inward | Rules testable in isolation; no state leaks between students (BUG-1, BUG-2) |
| Minimal API with DTO records, `ProblemDetails` and OpenAPI | Explicit contract, no over-posting, no leaked internals (SEC-8) |
| Database-first SQL Server, EF Core LINQ, Mapperly | The schema stays the source of truth; parameterized queries end injection (SEC-1–3) |
| One transaction per save; unique `(StudentID, AttendDate)` and `rowversion` return `409` on races | No partial saves or duplicate rows (BUG-5, BUG-6) |
| TDD with unit tests and integration tests on Testcontainers SQL Server | Proves legacy parity and each bug fix |
| Stateless and 12-factor; lists paged (default 50, max 200); `/health`; Docker compose | Horizontal scale, bounded responses, same stack locally and in tests |
| No authentication yet; endpoints in route groups | Out of scope; it can be added in one place later |

## Ambiguities and missing details

Each was decided by the project owner; `#n` refers to the [decision log](docs/decisions.md).

| ID | Question | Resolution |
|----|----------|------------|
| AMB-1, 3, 16, 20; MIS-2, 6, 7, 11, 13, 17 | Student lists, grade grouping, reports, UI, alert handling, master data, data migration | Out of scope; master data is seeded by SQL |
| AMB-2, MIS-1, 3 | Missing tables and `SchoolYear()` function | DDL from inferred columns (#22); year from terms (#30) |
| AMB-4 | School year from September 1 or from terms | Terms: first term start to last term end (#30) |
| AMB-5 | Do excused absences count | Yes; late (`L`) counts as present (#27) |
| AMB-6 | Unknown or inactive codes | Rejected with `400` (#26) |
| AMB-7 | Blank code means present | No; `code` is required (#25) |
| AMB-8 | Concurrent edits | Last write wins; true races return `409` (#41) |
| AMB-9, MIS-16 | Allowed dates | Today or earlier in the school's time zone, no holiday check; outside every school year is `400` (#28, #31) |
| AMB-10 | Minutes late tied to a late code | No; 0–600, default 0 (#40) |
| AMB-11 | Alert lifecycle | No auto-resolve; a new alert may follow a manual resolution (#38) |
| AMB-12 | Code flags: snapshot or live | Snapshot at save time (#39) |
| AMB-13 | Consumer of `sp_GetStudentAttendance` | The two `GET` endpoints |
| AMB-14 | Summary scope | Per student, school and year (#36, #46) |
| AMB-15 | Audit user without authentication | `Attendance:AuditUser` setting (#42) |
| AMB-17 | Inactive or foreign students | Must be active and in the school, else `400` (#34) |
| AMB-18, MIS-18 | Time zone for "today" | `Schools.TimeZone`, an IANA id defaulting to `UTC` (#29) |
| MIS-8 | Attendance codes | `P`, `A`, `E` (excused), `L` (#24) |
| MIS-4, 5, 9 | Keys and indexes, SQL Server version, sample data | Proposed, **pending** (#21, #23) |
| AMB-19, MIS-10, 12, 14, 15, 19 | Threshold per year, volumes, roles, performance, privacy, localization | **Open**; the threshold stays per school, default 10 |

## How to run

Works once phase 7 of the plan is done. Needs Docker Desktop; the .NET SDK is only needed for tests.

```bash
cp .env.example .env         # SQL Server password, audit user
docker compose up --build    # SQL Server, schema, demo data, API
dotnet test                  # unit and integration tests (needs Docker)
```

The API runs at <http://localhost:8080> (OpenAPI at `/openapi/v1.json`, health at `/health`). `docker compose down -v` resets the database.

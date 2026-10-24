# Attendance System

A port of a legacy student attendance app (a VB6 form plus SQL Server stored procedures) to a .NET REST API, limited to three use cases ([scope](docs/0.scope.md)). `legacy/` holds the original source and is read-only.

| Use case | Endpoint |
|----------|----------|
| Save daily attendance for a set of students | `PUT /schools/{schoolId}/attendance/{date}` |
| Student attendance history (paged, newest first) | `GET /students/{studentId}/attendance` |
| Chronic absenteeism status | `GET /students/{studentId}/absence-summary` |

**Docs:** [legacy analysis](docs/01.legacy-analysis.md) (rules, bugs, ambiguities, missing details) · [implementation plan](docs/02.implementation-plan.md) · [decision log](docs/decisions.md) · [AI collaboration log](docs/ai-logs.md)

**Status:** planned. The code, Docker files and tests are built in the phases of the implementation plan.

## Key architecture decisions

| Decision | Why |
|----------|-----|
| **Three tiers:** clients call a REST API; only the API reaches the database | The legacy form connected straight to SQL Server, so anyone with a SQL client could bypass it (SEC-5) |
| **DDD layers:** `Domain` → `Application` → `Infrastructure` / `Api`, dependencies pointing inward | Business rules live in one testable place with no EF or HTTP in them |
| **Minimal API, DTO records, `ProblemDetails`, OpenAPI** | A small, explicit HTTP contract; DTOs prevent over-posting, and errors never leak internals (SEC-8) |
| **Database-first SQL Server + EF Core LINQ:** legacy schema plus new scripts in `db/scripts`, scaffolded | The schema stays the source of truth; parameterized LINQ removes the SQL and XML injection (SEC-1–3), and set-based queries replace the cursor (BUG-15) |
| **Atomic writes:** one `SaveChangesAsync` (one transaction) per save | The legacy procedure had no transaction, so a failure left partial saves (BUG-5) |
| **Unique `(StudentID, AttendDate)` index and `rowversion`;** races return `409` | Concurrent saves created duplicate rows (BUG-6) |
| **Per-object domain logic** instead of cursor variables | Reused variables wrote attendance to the wrong student (BUG-1, BUG-2) |
| **Mapperly** for all mapping | Compile-time generated, with no reflection, and supports `IQueryable` projections |
| **TDD:** unit tests for the domain and use cases; integration tests with `WebApplicationFactory` and Testcontainers SQL Server | Proves parity with the legacy rules and each bug fix against a real database |
| **Stateless, 12-factor:** environment configuration, logs to stdout, `/health`, Docker compose | Scales horizontally; the same stack runs locally and in tests |
| **Paging on every list query** (default 50, max 200) | Responses stay bounded |
| **No authentication** in this version; endpoints sit in route groups | Out of scope; it can be added in one place later |

## Ambiguities and how we addressed them

Each one was put to the project owner, not assumed; the decision numbers (`#n`) link to the [decision log](docs/decisions.md).

| ID | Ambiguity | Resolution |
|----|-----------|------------|
| AMB-1 | Missing student-list procedure; meaning of an empty grade | Out of scope: the client sends explicit student IDs |
| AMB-2 | Missing `Schools`, `SchoolTerms`, submission log, `SchoolYear()` | DDL written from the inferred columns; terms replace `SchoolYear()` (#22, #30) |
| AMB-3 | Grade or class/homeroom grouping | Out of scope: no student lists, so no grouping |
| AMB-4 | School year: fixed September 1 or terms | Terms: a year runs from its first term start to its last term end (#30) |
| AMB-5 | Do excused absences count | Yes, every absent code counts, as before; late (`L`) is present (#27) |
| AMB-6 | Unknown or inactive codes | Rejected with `400`; the legacy app saved them as present (#26) |
| AMB-7 | Blank code means present | No: `code` is required, and only the entries sent are saved (#25) |
| AMB-8 | Concurrent edits | A resave overwrites (last write wins); true races return `409` (#41) |
| AMB-9 | Allowed dates | Today or earlier in the school's time zone; a date outside every school year is `400` (#28, #31) |
| AMB-10 | Minutes late tied to a late code | No: optional, 0–600, default 0 (#40) |
| AMB-11 | Alert lifecycle | No auto-resolve; a new alert may follow a manual resolution (#38) |
| AMB-12 | Absent/excused flags: snapshot or live | Snapshot of the code at save time (#39) |
| AMB-13 | Consumer of `sp_GetStudentAttendance` | Becomes the history and status endpoints |
| AMB-14 | Summary per student-year or per school | Per student, school and year, fixing BUG-10; status takes an optional `schoolId` (#36, #46) |
| AMB-15 | Audit user without authentication | The `Attendance:AuditUser` setting (#42) |
| AMB-16 | Printed report | Out of scope |
| AMB-17 | Inactive students | Student must exist, be active and belong to the school, else `400` (#34) |
| AMB-18 | Time zone for "today" | New `Schools.TimeZone` column, an IANA id defaulting to `UTC` (#29) |
| AMB-19 | Threshold per school or per year | **Open:** kept per school, default 10, as before |
| AMB-20 | Who maintains master data | Out of scope: seeded by SQL scripts |

## Missing details

| ID | Missing | Resolution |
|----|---------|------------|
| MIS-1 | DDL for three referenced tables | Written from inference (#22) |
| MIS-3 | `SchoolYear()` source | Not needed: the year comes from terms (#30) |
| MIS-4, MIS-9 | Real indexes and keys; sample data | Integrity script and synthetic demo data proposed (#23, **pending**) |
| MIS-5 | SQL Server version | SQL Server 2022 proposed (#21, **pending**) |
| MIS-8 | Attendance code list | `P`, `A`, `E` (excused), `L` (#24) |
| MIS-10 | Data volumes | No batch cap (#48); paging on lists (#50); volumes **open** |
| MIS-16, MIS-18 | Holidays; time zone per school | Holidays not checked (#28); `Schools.TimeZone` (#29) |
| MIS-12, 14, 15, 19 | Roles, performance targets, privacy rules, localization | **Open** |

The rest (report layout, student list procedure, VB6 project files, data migration) is out of scope.

## How to run

Not available yet: these commands work once phase 7 of the plan is done. Docker Desktop is required; the .NET SDK is only needed for tests or for running outside Docker.

```bash
cp .env.example .env         # set the SQL Server password and audit user
docker compose up --build    # SQL Server, schema and demo data, then the API
dotnet test                  # unit and integration tests (Docker must be running)
```

The API runs at <http://localhost:8080>, with OpenAPI at `/openapi/v1.json` and health at `/health`. `docker compose down -v` resets the database.

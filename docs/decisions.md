# Decisions

Project decisions, newest last. Each entry: what was decided, why, and its status. Details live in [.claude/rules/architecture.md](../.claude/rules/architecture.md); the conversation history is in [ai-logs.md](ai-logs.md).

| # | Date | Decision | Rationale | Status |
|---|------|----------|-----------|--------|
| 1 | 2026-10-24 | `legacy/` is read-only, enforced by the `Edit(/legacy/**)` deny rule | The legacy source is the reference for the port and must stay untouched | Accepted |
| 2 | 2026-10-24 | Backend is an ASP.NET Core Minimal API exposing REST | Lightweight, modern .NET; REST best practices (ProblemDetails, OpenAPI, correct status codes) | Accepted |
| 3 | 2026-10-24 | Unit and integration tests, written test-first (TDD) | Confidence in behavior parity with the legacy system | Accepted |
| 4 | 2026-10-24 | SQL Server with database-first EF Core, queries in LINQ | The existing schema is the source of truth; LINQ keeps queries typed and parameterized | Accepted |
| 5 | 2026-10-24 | Domain-driven design: Domain / Application / Infrastructure / Api layers | Keeps business rules isolated. Scaffolded EF classes stay persistence-only in Infrastructure | Accepted |
| 6 | 2026-10-24 | Secure by default: no string-built SQL, boundary validation, no leaked internals, no secrets in source | Avoid SQL injection and common vulnerabilities | Accepted |
| 7 | 2026-10-24 | Global exception handler with ProblemDetails, explicit results for expected failures, structured `ILogger` logging | Consistent errors and diagnosable logs without scattered try/catch | Accepted |
| 8 | 2026-10-24 | Stateless, horizontally scalable API: correct DI lifetimes, async with cancellation, paginated queries | Avoid memory leaks and allow scale-out | Accepted |
| 9 | 2026-10-24 | Reuse code, but keep complexity low (no speculative abstractions) | Balance DRY against KISS/YAGNI | Accepted |
| 10 | 2026-10-24 | Atomic writes: one `SaveChangesAsync` per command or an explicit transaction, plus optimistic concurrency | Data consistency | Accepted |
| 11 | 2026-10-24 | API exchanges DTOs only, never EF entities or domain aggregates | Decouple the contract from persistence and prevent over-posting | Accepted |
| 12 | 2026-10-24 | Mapperly (`Riok.Mapperly`) for all mapping | Compile-time generated, no reflection, supports `IQueryable` projection | Accepted |
| 13 | 2026-10-24 | Dockerfile and docker compose (SQL Server + API) for local runs and tests | One-command local stack | Accepted; files created with the solution scaffold |
| 14 | 2026-10-24 | No authentication or authorization in this version | Out of scope; route groups make adding it later a one-place change | Accepted |
| 15 | 2026-10-24 | Use parallel subagents for independent work | Faster delivery. Agents get non-overlapping file scopes to avoid conflicts | Accepted |
| 16 | 2026-10-24 | Follow SOLID, YAGNI, DRY and 12-factor app principles | Maintainable, testable, cloud-ready code. Extends decision 9 | Accepted |
| 17 | 2026-10-24 | `.gitignore` ignores .NET build output, IDE files, test/coverage results, local secrets (`.env`, `*.pfx`; `.env.example` kept), the compose override, OS files and `graphify-out/` | Keep the repo clean and secrets out of source control | Accepted |
| 18 | 2026-10-24 | Scope limited to three use cases: save daily attendance, student attendance history, chronic absenteeism status ([0.scope.md](0.scope.md)) | Keep the port focused and avoid overengineering | Accepted |
| 19 | 2026-10-24 | Port the intent of the legacy rules but fix the bugs listed in [01.legacy-analysis.md](01.legacy-analysis.md), recording each deviation here | Several legacy behaviors are defects (wrong-student updates, multi-year absence counts) | Accepted |
| 20 | 2026-10-24 | Build from [02.implementation-plan.md](02.implementation-plan.md): phased, test-first, one commit per step | A shared, reviewable path from analysis to code | Accepted |
| 21 | 2026-10-24 | Stack proposal: .NET 10 LTS, SQL Server 2022 (pinned CU), built-in Minimal API validation, xUnit v3 with hand-written fakes, Testcontainers.MsSql | Current LTS releases and the fewest dependencies. Approved with the plan | Accepted |
| 22 | 2026-10-24 | Write the DDL for `Schools`, `SchoolTerms` and `AttendanceSubmissionLog` from the inferred columns (MIS-1, AMB-2) | The real DDL is not available | Accepted |
| 23 | 2026-10-24 | Schema hardening proposal: unique `(StudentID, AttendDate)`, filtered unique open alert, `rowversion`, FKs, checks; synthetic demo data; legacy procedures not deployed (BUG-4, BUG-6, MIS-4, MIS-9) | Integrity backstops for the domain rules; procedure logic is ported to LINQ. Approved with the plan | Accepted |
| 24 | 2026-10-24 | Seed attendance codes `P` Present, `A` Absent, `E` Excused absence, `L` Late (MIS-8) | Minimal set covering present, absent, excused and late | Accepted |
| 25 | 2026-10-24 | Attendance `code` is required; no implicit `P` (AMB-7, R3) | Nothing is marked present by accident | Accepted |
| 26 | 2026-10-24 | Unknown or inactive codes are rejected with `400` (AMB-6, BUG-7) | The legacy system silently stored them as present | Accepted |
| 27 | 2026-10-24 | Excused absences count toward the chronic-absence total (AMB-5, R6) | Keeps the legacy rule | Accepted |
| 28 | 2026-10-24 | Attendance dates: today or earlier in the school's time zone, back-dating allowed, no weekend or holiday checks (AMB-9, MIS-16, BUG-18) | Allows corrections without a school calendar | Accepted |
| 29 | 2026-10-24 | Per-school time zone: new `Schools.TimeZone VARCHAR(64) NOT NULL DEFAULT 'UTC'`, an IANA id (AMB-18, MIS-18) | "Today" and the school year depend on where the school is | Accepted |
| 30 | 2026-10-24 | School year from terms: new `SchoolTerms.SchoolYear` (`YYYY-YYYY`); a year spans its earliest term start to its latest term end. Replaces the September 1 rule R1 (AMB-4, MIS-3) | Schools define their own calendars; breaks between terms belong to the year | Accepted |
| 31 | 2026-10-24 | Saving a date outside every school year of the school returns `400` | No year means no summary to update | Accepted |
| 32 | 2026-10-24 | UC2/UC3 default year: the latest school year that started on or before today (R10) | In the summer the year that just ended is shown | Accepted |
| 33 | 2026-10-24 | Overlapping terms: earliest `StartDate` wins, then lowest `TermID` (BUG-14) | Deterministic instead of arbitrary | Accepted |
| 34 | 2026-10-24 | Saved students must exist, be active and belong to the school; otherwise `400` listing the entries (AMB-17, V5, SEC-4) | Closes the legacy hole that let anyone write any student's attendance | Accepted |
| 35 | 2026-10-24 | Saving for an inactive school returns `400`; a missing school returns `404` (V1) | The school exists, but the request is invalid | Accepted |
| 36 | 2026-10-24 | Absence summary per student, school and school year; the unique key changes to `(StudentID, SchoolID, SchoolYear)` by script (AMB-14, BUG-10) | A transfer no longer overwrites another school's total | Accepted |
| 37 | 2026-10-24 | At most one open `CHRONIC_ABSENCE` alert per student, school and school year (R8) | Consistent with the summary scope | Accepted |
| 38 | 2026-10-24 | Alerts never auto-resolve; a new one may follow a manually resolved one (AMB-11, BUG-11) | Keeps legacy behavior; resolution is out of scope | Accepted |
| 39 | 2026-10-24 | `IsAbsent`/`IsExcused` are snapshotted from the code at save time (AMB-12, R4) | Code changes don't rewrite history | Accepted |
| 40 | 2026-10-24 | `minutesLate` optional, default 0, range 0–600, independent of the code (AMB-10, BUG-12) | Simple, bounded, no hidden "late code" rule | Accepted |
| 41 | 2026-10-24 | Concurrency: a resave overwrites (last write wins); true races (duplicate insert, `rowversion` mismatch) return `409` (AMB-8, V12) | Keeps legacy resave behavior without duplicates or lost summary updates | Accepted |
| 42 | 2026-10-24 | Audit user from the `Attendance:AuditUser` setting until authentication exists (AMB-15, R9) | `SYSTEM_USER` would be the API's login for everyone | Accepted |
| 43 | 2026-10-24 | UC1 returns `200` with per-student results: total absences, chronic flag, alert raised | Clients see the effect of the save without another call | Accepted |
| 44 | 2026-10-24 | UC2 history covers all schools for the year; each row carries its `schoolId` | Keeps legacy behavior | Accepted |
| 45 | 2026-10-24 | UC2 is paged: default 50, capped at 200 (V11) | Architecture rule on list endpoints | Accepted |
| 46 | 2026-10-24 | UC3 takes an optional `schoolId`, defaulting to the student's current school | Follows from per-school summaries (#36) | Accepted |
| 47 | 2026-10-24 | UC3 with no summary returns `200` with 0 absences; the threshold is always the effective value (R22, BUG-13) | A student with no absences is a valid answer, not "not found" | Accepted |
| 48 | 2026-10-24 | No cap on entries per save (V4, MIS-10) | User's choice. Deviates from the boundary-validation rule; Kestrel's 30 MB body limit still bounds a request | Accepted |
| 49 | 2026-10-24 | Ambiguities and missing details are put to the user, never decided by the assistant; each answer is recorded here and linked from [01.legacy-analysis.md](01.legacy-analysis.md) | The user owns product decisions | Accepted |
| 50 | 2026-10-24 | Every query that returns a list is paged through one shared helper (`page`, `pageSize` default 50, cap 200, `totalCount`); command responses such as UC1's results are not paged | Bounded responses everywhere; one consistent contract | Accepted |
| 51 | 2026-10-24 | Tests run on Microsoft.Testing.Platform (`global.json` `test.runner`), xUnit v3 without the VSTest adapter; SDK pinned to 10.0.400 with `latestFeature` roll-forward | xUnit v3 4.x no longer supports VSTest under the .NET 10 SDK | Accepted |
| 52 | 2026-10-24 | Central package management with transitive pinning; `System.Security.Cryptography.Pkcs` pinned to 10.0.13 | `Microsoft.Data.SqlClient` 6.1.7 brings 9.0.11, which has known high and moderate vulnerabilities (NuGet audit fails the build) | Accepted |
| 53 | 2026-10-24 | Implementation agents run in parallel, each in its own git worktree and branch, merged into `master` by the main thread | One agent's unfinished code can't break another's build or tests | Accepted |
| 54 | 2026-10-24 | A `tester` subagent ([.claude/agents/tester.md](../.claude/agents/tester.md)) tests the app black-box: HTTP endpoints plus read-only `SELECT`s against the database, never source code. URLs, ports and credentials come from `.env`. It checks behavior against the scope, plan, decisions and legacy analysis (`R-n`, `V-n`, `BUG-n`). If the API isn't running it starts `docker compose`, and afterwards it stops what it started, keeping the volume. It returns a findings report | Independent verification of documented and legacy behavior, separate from the TDD suites | Accepted |
| 58 | 2026-10-24 | The `tester` subagent is strictly read-only: it never creates, edits or deletes any file, inside or outside the repo. When `.env` is missing it passes `--env-file .env.example` to `docker compose` instead of creating `.env`. Refines #54 | Testing must not change the workspace it tests | Accepted |

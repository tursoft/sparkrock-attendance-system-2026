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

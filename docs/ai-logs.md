# AI Collaboration Log

Running log of prompts given to the AI assistant (Claude Code), the assistant's suggestions/actions, and the decisions made.

---

## 2026-10-24

### 1. Initialize git repository
- **Prompt:** "git init this folder and set remote repo as https://github.com/tursoft/sparkrock-attendance-system-2026"
- **AI suggestion/action:** Ran `git init` and added `origin` pointing to the GitHub repo. Offered to make an initial commit and push.
- **Decision:** Repo initialized with remote; push deferred (see #3).

### 2. Install and enable graphify
- **Prompt:** "install graphify and enable to save token costs"
- **AI suggestion/action:**
  - Confirmed the `graphifyy` Python package was already installed (CLI not on PATH, so used `python -m graphify`).
  - Ran `graphify claude install`: added a graphify section to `CLAUDE.md` and a PreToolUse hook in `.claude/settings.json` that points the assistant to the knowledge graph before grep/find searches.
  - Ran `graphify hook install`: git post-commit/post-checkout hooks rebuild the graph automatically (AST-only, no LLM cost).
  - Built the initial graph with `graphify update .`. Noted that the AST extractor does not parse the legacy `.sql`/`.frm` files, so the graph is minimal until modern code exists.
  - Rewrote `CLAUDE.md` commands to `python -m graphify ...` so they work on this machine.
  - Added `graphify-out/` to `.gitignore` (regenerable output).
- **Decision:** Accepted. Committed as "Enable graphify knowledge graph for Claude Code".

### 3. Commit policy
- **Prompt:** "from now on, auto generate git commits for each meaningful logic piece of work, never push"
- **AI suggestion/action:** Saved this as a standing rule. Split existing work into two commits: legacy source baseline, then graphify setup.
- **Decision:** Auto-commit per logical unit; never push (user pushes manually).

### 4. AI activity log
- **Prompt:** "also log my prompts, your suggestions and my decisions in docs\ai-logs.md"
- **AI suggestion/action:** Created this file, back-filled entries 1–3, and saved logging as a standing rule for future work.
- **Decision:** Every prompt, suggestion and decision is appended here and committed along with the related work.

### 5. Commit message convention
- **Prompts:** "use best practices in git commit names, keep them short," and "use prefixes feat, fix, doc etc."
- **AI suggestion/action:** Adopted Conventional Commits (`type(scope): subject`, imperative, ≤50 chars), using the standard `docs` spelling rather than `doc`. Reworded the three existing unpushed commits to match.
- **Decision:** Accepted. All future commits use feat/fix/docs/refactor/test/chore/... prefixes with short subjects.

### 6. Restructure CLAUDE.md into rule files
- **Prompt:** "create claude.md file, keep it short and create dedicated rule files in .claude\rules folder and refer from claude.md"
- **AI suggestion/action:** Rewrote `CLAUDE.md` as a short project overview with links to three rule files: `.claude/rules/git.md` (commit policy and convention), `.claude/rules/ai-logs.md` (this log's format), and `.claude/rules/graphify.md` (moved from the generated graphify section in `CLAUDE.md`).
- **Decision:** Accepted.

### 7. No dedicated commits for the log
- **Prompt:** "do not creatge dedicated commit for ai-logs.md, keep its changes in relevant work commit"
- **AI suggestion/action:** Updated `.claude/rules/ai-logs.md`: log changes are always staged with the related work commit, never committed alone. Entries for prompts that produce no committable work stay uncommitted until the next work commit. Left the existing history as it is.
- **Decision:** Accepted.

### 8. Make legacy/ read-only
- **Prompt:** "I am developing a port of legacy app stored in folder legacy (keep this folder readonly, do not make any change on it. set deny rules in persmission file on claude)"
- **AI suggestion/action:** Added `permissions.deny: ["Edit(/legacy/**)"]` to `.claude/settings.json`. The `Edit` rule covers every file-writing tool (Edit, Write, NotebookEdit), and the leading `/` anchors it to the project root.
- **Decision:** Accepted.

### 9. Architecture rules
- **Prompt:** "store my specs in architecture rules"
- **AI suggestion/action:** Created `.claude/rules/architecture.md` to record the project specs: the project is a port of the legacy app, and `legacy/` is read-only. Linked it from `CLAUDE.md`.
- **Decision:** Accepted.

### 10. Target stack
- **Prompt:** "app will be .net minimal api, as rest with best practices"
- **AI suggestion/action:** Added a Backend section to `.claude/rules/architecture.md`: ASP.NET Core Minimal API with REST conventions (resource routes, correct status codes, ProblemDetails errors, boundary validation, DTOs separate from entities, OpenAPI, endpoints grouped per resource with `MapGroup`).
- **Decision:** Accepted.

### 11. Testing requirement
- **Prompt:** "implement both unit and integratiıon tests"
- **AI suggestion/action:** No app code exists yet, so recorded this as a rule: a Testing section in `.claude/rules/architecture.md`. Unit tests cover isolated logic. Integration tests run through `WebApplicationFactory` against a Testcontainers SQL Server, and legacy stored procedure behavior becomes test cases.
- **Decision:** Accepted (tests will be written alongside each feature).

### 12. Data access stack
- **Prompt:** "use mssql, db first appoorach, linq and entity framework"
- **AI suggestion/action:** Added a Data access section to `.claude/rules/architecture.md`:
  - SQL Server with EF Core, database-first: scaffold via `dotnet ef dbcontext scaffold`, never hand-edit generated files, extend through `partial` classes, no code-first migrations.
  - Queries in LINQ, with legacy stored procedure logic ported to LINQ.
  - Async calls, and `AsNoTracking()` for reads.
- **Decision:** Accepted.

### 13. Domain-driven design
- **Prompt:** "use domain driven apporach for implementation"
- **AI suggestion/action:** Added a Domain-driven design section to `.claude/rules/architecture.md`:
  - Layers: Domain, Application, Infrastructure, Api, with dependencies pointing inward.
  - To reconcile DDD with database-first, the scaffolded EF classes are treated as persistence models inside Infrastructure only. Repositories map them to separate, behavior-rich domain aggregates.
  - Updated the data access and testing rules to match.
- **Decision:** Accepted.

### 14. Test-driven development
- **Prompt:** "and test driven developmenyt"
- **AI suggestion/action:** Added TDD to the Testing section of `.claude/rules/architecture.md`: red-green-refactor, and no production code without a failing test first.
- **Decision:** Accepted.

### 15. Security
- **Prompt:** "avoid any sql injection or security valunerabilities"
- **AI suggestion/action:** Added a Security section to `.claude/rules/architecture.md`:
  - Parameterized EF/LINQ only, with no string-built SQL (`FromSql` rather than `FromSqlRaw` if raw SQL is ever needed).
  - Boundary validation, and DTO binding to prevent over-posting.
  - No leaked internals in error responses, and no secrets in source.
  - HTTPS/HSTS, explicit CORS, authorization by default.
  - Vulnerable-package checks, and security-focused tests.
- **Decision:** Accepted.

### 16. Error handling and logging
- **Prompt:** "add enoug error handling and logging"
- **AI suggestion/action:** Added an Error handling and logging section to `.claude/rules/architecture.md`:
  - A global `IExceptionHandler` returns `ProblemDetails` with a `traceId`.
  - Expected failures are explicit results mapped to 4xx, and domain exceptions are reserved for invariant violations. No silent catches.
  - Structured `ILogger<T>` logging with correct levels and per-request correlation, keeping secrets and personal data out of logs.
  - Error-path tests.
- **Decision:** Accepted.

### 17. Scalability, statelessness, memory
- **Prompt:** "avoid memory leaks, keep it scalable, stateless"
- **AI suggestion/action:** Added a Scalability, statelessness and resource use section to `.claude/rules/architecture.md`:
  - A stateless API that scales horizontally.
  - Correct DI lifetimes (scoped `DbContext`, no captive dependencies).
  - Leak prevention: disposal, `IHttpClientFactory`, no unbounded static caches.
  - Async throughout with `CancellationToken`.
  - Paginated, projected, N+1-free queries.
  - Caching only if distributed or bounded, plus health checks.
- **Decision:** Accepted.

### 18. Code reuse vs. complexity
- **Prompt:** "reuse codes as much as possible buyt also keep code complexisty low"
- **AI suggestion/action:** Added a Code reuse and complexity section to `.claude/rules/architecture.md`:
  - Don't repeat yourself, with shared building blocks (Result type, ProblemDetails mapping, validation filters, pagination, test fixtures).
  - Balanced with KISS/YAGNI: no speculative abstractions, small methods, low cyclomatic complexity, and duplication over the wrong abstraction.
- **Decision:** Accepted.

### 19. Atomic transactions
- **Prompt:** "use atomic transactions for consistency"
- **AI suggestion/action:** Extended Data access in `.claude/rules/architecture.md`:
  - Each write use case is atomic: one `SaveChangesAsync` per command, or an explicit transaction across multiple saves.
  - Optimistic concurrency via `rowversion`, mapped to `409 Conflict`.
  - Integration tests prove that nothing persists on mid-use-case failure.
- **Decision:** Accepted.

### 20. DTOs at the API boundary
- **Prompt:** "use dtos to exchange data in api, not ef entitites"
- **AI suggestion/action:** Already covered by the Backend and DDD rules. Made the Backend rule explicit: DTOs only, as immutable records, with no EF entities or domain aggregates crossing the API. Mapping happens in the application layer, and read queries project directly to DTOs.
- **Decision:** Accepted.

### 21. Mapperly for mapping
- **Prompt:** "use maperly for mapping"
- **AI suggestion/action:** Added a Mapping section to `.claude/rules/architecture.md`:
  - Riok.Mapperly (source-generated) for all mapping, with no AutoMapper or hand-written boilerplate.
  - One static partial mapper per boundary: Application for domain ↔ DTO, Infrastructure for persistence ↔ domain.
  - `IQueryable` projections for reads, constructor mapping so domain invariants run, and unmapped-member diagnostics as errors.
- **Decision:** Accepted.

### 22. Docker and docker compose
- **Prompt:** "implement dockerfile and dockercompose to run stack in local and test"
- **AI suggestion/action:** No .NET solution exists yet, so writing a Dockerfile now would mean guessing project paths. Recorded a Containers section in `.claude/rules/architecture.md` instead:
  - Multi-stage non-root Dockerfile.
  - Compose with a SQL Server service (health check, volume, schema init from the read-only `legacy/Database`) and the API service.
  - `.env.example` committed, and tests run via Testcontainers or compose.
  - The actual files will be created when the solution is scaffolded.
- **Decision:** Pending: the files will be implemented with the solution scaffold.

### 23. No authentication
- **Prompt:** "no authentication in this version"
- **AI suggestion/action:** Replaced the authorization-by-default security rule: no auth in this version, with endpoints kept in `MapGroup` route groups so `RequireAuthorization()` can be added in one place later. Dropped "unauthorized access" from the security test list.
- **Decision:** Accepted.

### 24. Decision log
- **Prompt:** "start to store all decisions in docs\decisions.md"
- **AI suggestion/action:** Created `docs/decisions.md` as a numbered table (date, decision, rationale, status) and back-filled decisions 1–14 from this session. Added `.claude/rules/decisions.md`: never delete rows, supersede instead, and commit with the related work. Linked it from `CLAUDE.md`.
- **Decision:** Accepted.

### 25. Parallel agents
- **Prompt:** "also use parallel agents if possible"
- **AI suggestion/action:** Added `.claude/rules/agents.md`: run independent work (legacy analysis, separate layers or features, per-aggregate tests, reviews) in concurrent subagents with non-overlapping file scopes, and keep sequential TDD steps in one thread. Linked it from `CLAUDE.md` and added decision 15.
- **Decision:** Accepted.

### 26. Engineering principles
- **Prompt:** "stick to major SWE principles like SOLID, YAGNI, DRY and 12 factor apps"
- **AI suggestion/action:** Renamed the "Code reuse and complexity" section of `.claude/rules/architecture.md` to "Engineering principles". Added SOLID (applied to the DDD layers and DI) and the relevant 12-factor items (env config, backing services, stateless processes, stdout logs, dev/prod parity). Labelled the existing DRY and KISS/YAGNI rules. Added decision 16.
- **Decision:** Accepted.

### 27. .gitignore for the solution
- **Prompt:** "add proper .gitignore file for solution"
- **AI suggestion/action:** Replaced the one-line `.gitignore` with a .NET-focused one covering:
  - Build output (`bin/`, `obj/`, publish, packages) and NuGet artifacts.
  - IDE files (Visual Studio, Rider, VS Code, keeping the shared `.vscode` configs).
  - Test results and coverage.
  - Local secrets: `.env*` except `.env.example`, `secrets.json`, certificates and keys.
  - `docker-compose.override.yml`, OS files, `.claude/settings.local.json` and `graphify-out/`.
  - Added decision 17.
- **Decision:** Accepted.

### 28. Legacy analysis
- **Prompt:** "analyze existing legacy folder and report in docs\01.legacy-analysis.md file. include what does app, how its flow, what are security issuyes, bugs and ambiquities."
- **AI suggestion/action:** Read all four legacy files and wrote `docs/01.legacy-analysis.md`: inventory (including referenced-but-missing objects), what the app does, load/save/print flows as Mermaid diagrams, 10 security issues, 26 bugs, and 20 open ambiguities, plus how the target architecture addresses them. Key findings: SQL and XML injection in the form; in the save procedure, `@ExistingID` and the code flags are not reset per cursor iteration (attendance can overwrite the wrong student), the absence recount ignores the year, and there is no transaction.
- **Decision:** Accepted, extended by #29–#31.

### 29. Business rules and entities
- **Prompt:** "detect and list bnusiness rules, entities"
- **AI suggestion/action:** Made business rules their own section (R1–R23) and added an Entities section: every column with constraints, the inferred missing tables, a Mermaid ER diagram, and candidate aggregates for the port.
- **Decision:** Accepted.

### 30. Validation rules
- **Prompt:** "validation rules"
- **AI suggestion/action:** Added a Validation rules section: what the legacy code validates (almost nothing) and proposed port rules V1–V13 with their HTTP responses.
- **Decision:** Accepted.

### 31. Missing details
- **Prompt:** "report any missing details that we may need"
- **AI suggestion/action:** Added a Missing details section (MIS-1–MIS-19) covering source and schema, data and requirements, each with a fallback assumption.
- **Decision:** Accepted.

### 32. Scope of this version
- **Prompt:** "this is the scope of this version, store in docs\0.scope.md and stick to this scope. avoid overengineering" (screenshot: save daily attendance for a set of students; retrieve attendance history for a student; return chronic absenteeism status for a student)
- **AI suggestion/action:** Created `docs/0.scope.md` with the three use cases mapped to the legacy procedures, an explicit out-of-scope list (auth, UI, printing, student lists, master-data maintenance, alert resolution, data migration), and a no-overengineering guideline. Linked it from `CLAUDE.md` and the analysis. Added decisions 18 (scope) and 19 (fix legacy bugs deliberately).
- **Decision:** Accepted.

### 33. No dedicated commits for the decision log
- **Prompt:** "aslo do not create dedicated ghit commit just for decisions.md but include it in relevant work commits"
- **AI suggestion/action:** Updated `.claude/rules/decisions.md`: never commit `docs/decisions.md` on its own (or with only `docs/ai-logs.md`); decisions with no committable work stay uncommitted until the next work commit. Matches the existing rule for the AI log.
- **Decision:** Accepted.

### 34. README
- **Prompt:** "create a shor readme.md file including key architecture decisions, list of all ambiquities and how we addressed them. also add how to run this app. keep it short and max 1 printed page"
- **AI suggestion/action:** Wrote a one-page `README.md`: the three endpoints, eight key architecture decisions, a table of all 20 ambiguities (AMB-1–AMB-20) with their resolutions taken from `docs/implementation-plan.md`, and run steps (`docker compose up`, `dotnet test`). The run steps describe the planned compose setup, since no code exists yet. Flagged that `docs/decisions.md` stops at #19 while the plan cites #20–#49.
- **Decision:** Pending user review.

### 35. Implementation plan
- **Prompt:** "now create an implementation-plan.md in docs folder based on analysis report, scope and all rules in project"
- **AI suggestion/action:** Drafted the plan: goals and endpoints, solution layout, database scripts (missing tables, integrity constraints, seed data, EF scaffold), domain/application/infrastructure/api design, API contract, a test plan with legacy parity and bug-fix cases, containers, and nine phases with commits and parallel work. The first draft picked an answer for each ambiguity itself.
- **Decision:** Accepted, but every ambiguity reopened as a question (see #36). Renamed to `docs/02.implementation-plan.md` (#39).

### 36. Ask about ambiguities; track the answers
- **Prompts:** "ask me any unclear, open questions, ambiquities. do not decide on you own", "also keep track of our decisions about such ambiquities", "and missing details"
- **AI suggestion/action:**
  - Asked seven rounds of questions with a recommended option each. The user picked:
    - Code required (#25); unknown and inactive codes rejected (#26); excused absences count (#27); dates today or earlier (#28).
    - Per student-school-year summaries (#36) and alerts (#37); UC3 with an optional `schoolId` (#46).
    - Last write wins (#41); minutes late 0–600 (#40); audit user from config (#42); alerts never auto-resolve (#38).
    - A per-school IANA time zone column (#29); the school year from a new `SchoolTerms.SchoolYear` column (#30), with dates outside every year rejected (#31), the default year being the latest started (#32), and the earliest start winning on overlap (#33).
    - Only active students of the school (#34); `400` for an inactive school (#35); the code snapshot kept (#39).
    - `200` with per-student results from UC1 (#43); history across all schools (#44), paged (#45); `200` with zeros when there is no summary (#47).
    - DDL from inference (#22); `P`/`A`/`E`/`L` codes (#24); no batch cap (#48).
  - The user declined the stack question (SQL Server version, .NET version, validation and test libraries), so it stays a pending proposal (#21), as does the schema hardening (#23).
  - Recorded decisions 20–49. Added a **Resolution** column to the analysis Ambiguities and Missing details tables, linking each to its decision or marking it out of scope or open.
  - Changed the scope guideline and `.claude/rules/decisions.md`: ambiguities are asked, never decided alone (#49).
- **Decision:** Accepted as answered. Still open: AMB-19, MIS-10 volumes, MIS-12, MIS-14, MIS-15, MIS-19; pending: #21, #23.

### 37. README revision
- **Prompt:** "also create a short readme file including key archiectural decision with why. also any ambiquities detected and how we addressed them"
- **AI suggestion/action:** A parallel session had already written and committed `README.md` (#34), so built on it instead of overwriting it. Turned the architecture bullets into a decision/why table tied to the legacy findings, added decision links to the ambiguity table, and added a missing-details summary, doc links, and a note that the run steps work only after phase 7.
- **Decision:** Accepted.

### 38. Paging
- **Prompt:** "add paging for all table queries"
- **AI suggestion/action:** Asked whether UC1's per-student results should be paged too. The user chose queries only: every list query is paged through one shared `PageRequest`/`PagedResult<T>` helper (default 50, cap 200, `totalCount`) (#50). Added paging tests to the plan.
- **Decision:** Accepted.

### 39. Rename the plan
- **Prompt:** "rename implementation-plan.md to 02.implementation-plan.md"
- **AI suggestion/action:** Renamed the file to match the numbered docs, updated the links in `docs/decisions.md`, and linked it from `CLAUDE.md` and the README.
- **Decision:** Accepted.

### 40. Compact README
- **Prompt:** "keep readme.md shorted, make it more compact"
- **AI suggestion/action:** Cut `README.md` from 84 to about 60 lines. Use cases became a three-line endpoint list, and the doc links and status share one line. The architecture table went from 12 rows to 8 (still with the why). Out-of-scope and open ambiguities are grouped into single rows, and the missing-details table is folded into the ambiguities table. Every AMB and MIS ID still appears.
- **Decision:** Pending user review.

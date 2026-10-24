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

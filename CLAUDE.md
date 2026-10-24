# Attendance System

Sparkrock interview challenge: modernize a legacy student attendance system.

- `legacy/` contains the original source: a VB6 form (`Forms/frmDailyAttendance.frm`) and the SQL schema and stored procedures (`Database/`). It is read-only.
- `docs/` contains project documentation.
- **Scope:** only the use cases in [docs/0.scope.md](docs/0.scope.md). Stick to them and avoid overengineering.
- Legacy analysis (rules, entities, validation, bugs, open questions): [docs/01.legacy-analysis.md](docs/01.legacy-analysis.md).
- Implementation plan (phases, design, API contract, tests): [docs/02.implementation-plan.md](docs/02.implementation-plan.md).
- Black-box testing: the [tester](.claude/agents/tester.md) subagent tests the running API and its data against the docs and the legacy analysis, starting and stopping the stack when needed.

## Rules

- [Architecture](.claude/rules/architecture.md): DDD layers, .NET Minimal API (REST) with DTOs mapped by Mapperly, MSSQL with database-first EF Core, LINQ and atomic transactions, TDD with unit and integration tests, secure by default (no auth in this version), global error handling and structured logging, stateless and scalable, SOLID, DRY, YAGNI and 12-factor, Docker/compose for local runs; `legacy/` is read-only
- [Parallel agents](.claude/rules/agents.md): run independent work in parallel subagents
- [Git workflow](.claude/rules/git.md): auto-commit, never push, Conventional Commits
- [AI collaboration log](.claude/rules/ai-logs.md): log prompts, suggestions and decisions
- [Decision log](.claude/rules/decisions.md): record every project decision in `docs/decisions.md`
- [Graphify](.claude/rules/graphify.md): use the knowledge graph before searching code

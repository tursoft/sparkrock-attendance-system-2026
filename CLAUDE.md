# Attendance System

Sparkrock interview challenge: modernize a legacy student attendance system.

- `legacy/` contains the original source: a VB6 form (`Forms/frmDailyAttendance.frm`) and the SQL schema and stored procedures (`Database/`). It is read-only.
- `docs/` contains project documentation.

## Rules

- [Architecture](.claude/rules/architecture.md): .NET Minimal API (REST), MSSQL with database-first EF Core and LINQ, unit and integration tests; `legacy/` is read-only
- [Git workflow](.claude/rules/git.md): auto-commit, never push, Conventional Commits
- [AI collaboration log](.claude/rules/ai-logs.md): log prompts, suggestions and decisions
- [Graphify](.claude/rules/graphify.md): use the knowledge graph before searching code

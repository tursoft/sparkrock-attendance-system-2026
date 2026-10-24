# Architecture

- This project is a port of the legacy attendance app in `legacy/` (VB6 form plus SQL Server schema and stored procedures) to a modern stack.
- `legacy/` is read-only reference material. Never create, edit, move or delete anything in it. `.claude/settings.json` enforces this with a deny rule (`Edit(/legacy/**)`).
- New code lives outside `legacy/`.

## Backend

- ASP.NET Core Minimal API exposing a REST interface.
- Follow REST best practices:
  - Resource-oriented routes: plural nouns, no verbs (`/students/{id}/attendance`)
  - Correct HTTP methods and status codes (`201 Created` with `Location`, `204 No Content`, `404`, `409`, and so on)
  - Errors returned as RFC 7807 `ProblemDetails`
  - Input validated at the boundary
  - Separate request/response DTOs, never EF/database entities
  - OpenAPI document for every endpoint
  - Group endpoints by resource with `MapGroup`, one file per resource

## Data access

- Database: Microsoft SQL Server.
- ORM: Entity Framework Core, **database-first**. The schema is the source of truth. Scaffold the `DbContext` and entities with `dotnet ef dbcontext scaffold` and never edit the generated files by hand. Put customizations in `partial` classes so a re-scaffold doesn't lose them.
- Schema changes go into SQL scripts first, then re-scaffold. No EF code-first migrations.
- Queries use LINQ over the `DbContext`. Port legacy stored procedure logic to LINQ instead of calling the procedures or writing raw SQL.
- Use async EF calls, and `AsNoTracking()` for read-only queries.

## Testing

- Every feature ships with both unit and integration tests.
- Unit tests: business logic in isolation, with dependencies faked.
- Integration tests: drive the real HTTP pipeline through `WebApplicationFactory`, against a real database (Testcontainers SQL Server) seeded per test. Assert status codes, response bodies and persisted state.
- Port legacy stored procedure behavior as test cases so the new API matches the old rules.

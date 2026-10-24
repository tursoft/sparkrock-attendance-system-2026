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

## Domain-driven design

Layered solution, with dependencies pointing inward:

- `Domain`: aggregates, entities, value objects, domain services, and repository interfaces. It holds the business rules and invariants and depends on nothing else: no EF, no ASP.NET.
- `Application`: use cases (commands and queries) that coordinate aggregates through repository interfaces. It owns the DTOs.
- `Infrastructure`: the scaffolded EF Core `DbContext` and persistence entities, plus repository implementations that map persistence entities to domain models and back.
- `Api`: Minimal API endpoints. They are thin: bind, call the application layer, map to HTTP.

Rules:

- The scaffolded EF classes are persistence models, not the domain model. Never expose them outside `Infrastructure`.
- Aggregates guard their invariants. State changes go through methods, not public setters.
- Use the ubiquitous language from the legacy system (student, class, attendance, and so on) in type and member names.
- One repository per aggregate root. Load and save whole aggregates.

## Data access

- Database: Microsoft SQL Server.
- ORM: Entity Framework Core, **database-first**. The schema is the source of truth. Scaffold the `DbContext` and entities with `dotnet ef dbcontext scaffold` and never edit the generated files by hand. Put customizations in `partial` classes so a re-scaffold doesn't lose them.
- Schema changes go into SQL scripts first, then re-scaffold. No EF code-first migrations.
- Queries use LINQ over the `DbContext`, inside `Infrastructure` only. Port legacy stored procedure logic to LINQ instead of calling the procedures or writing raw SQL.
- Use async EF calls, and `AsNoTracking()` for read-only queries.
- Every write use case is atomic: all of it commits or none of it does.
  - Each command ends with a single `SaveChangesAsync` (unit of work, owned by the application layer). EF wraps that in one transaction.
  - If a use case truly needs several saves, wrap them in an explicit `BeginTransactionAsync` and commit or roll back together.
  - Never leave partial writes on failure.
- Handle concurrent edits with optimistic concurrency. Use a `rowversion` concurrency token where the schema has one, added via SQL script if needed. Map `DbUpdateConcurrencyException` to `409 Conflict`.
- Add integration tests proving that a failure mid-use-case persists nothing.

## Security

- No SQL injection: all data access goes through EF Core LINQ, which is parameterized. Never build SQL by string concatenation or interpolation. If raw SQL is ever unavoidable, use `FromSql`/`ExecuteSql` with interpolated parameters, never `FromSqlRaw` on user input.
- Validate all input at the API boundary (types, ranges, lengths, required fields). Reject anything unexpected with `400 ProblemDetails`.
- Prevent over-posting: bind to request DTOs, never to entities or aggregates.
- Never leak internals. Production error responses contain no stack traces, SQL or exception messages.
- No secrets in source control. Connection strings and keys come from configuration (user-secrets locally, environment variables in deployment).
- Enforce HTTPS and HSTS. Configure CORS with explicit origins only.
- Endpoints require authorization unless explicitly marked otherwise. Check per-resource access, not just authentication.
- Keep NuGet packages current, and check for known vulnerabilities with `dotnet list package --vulnerable`.
- Add tests for security-relevant behavior: malicious input, unauthorized access, validation failures.

## Error handling and logging

- One global exception handler (`IExceptionHandler` + `UseExceptionHandler`) turns unhandled exceptions into `500 ProblemDetails` with a `traceId`. No try/catch in endpoints just to rethrow or log.
- Expected failures are not exceptions. Domain and application code return explicit results (for example "not found", "validation failed", "conflict"), and the Api layer maps them to `404`/`400`/`409` `ProblemDetails`.
- Domain invariant violations throw a dedicated domain exception type, which the handler maps to `400`/`409`.
- Catch exceptions only where you can add value: translate, retry, or recover. Never swallow them silently.
- Logging goes through `ILogger<T>`:
  - Use structured message templates (`"Attendance saved for {StudentId}"`), never string interpolation.
  - Levels: `Information` for business events, `Warning` for handled anomalies, `Error` for failures, with the exception attached.
  - Use the built-in request logging and a correlation/trace id on every request.
  - Never log secrets, connection strings or personal student data beyond IDs.
- Add tests for error paths: each mapped failure returns the right status code and `ProblemDetails` shape.

## Scalability, statelessness and resource use

- The API is stateless. No session state, no in-memory per-user data, and no static mutable state. Any instance can serve any request, so it can scale horizontally behind a load balancer.
- Use correct DI lifetimes:
  - `DbContext` and repositories are scoped (one per request).
  - Only thread-safe, stateless services are singletons.
  - A singleton never captures a scoped or transient dependency.
- Avoid memory leaks:
  - Dispose `IDisposable`/`IAsyncDisposable` (`using`/`await using`).
  - Unsubscribe event handlers.
  - Use `IHttpClientFactory`, never `new HttpClient()` per call.
  - No unbounded static caches or collections.
- Async all the way down, with no `.Result`/`.Wait()`. Accept and pass `CancellationToken` from endpoints through to EF calls.
- Keep queries efficient:
  - Paginate list endpoints, with a capped page size.
  - Project to DTOs with `Select` and fetch only the needed columns.
  - Avoid N+1 queries.
  - Use `AsNoTracking()` for reads.
- If caching is added, it must be distributed (for example Redis via `IDistributedCache`) or safe to lose per instance, with size limits and expirations.
- Expose health checks (`/health`) for orchestrators and load balancers.

## Code reuse and complexity

- Don't repeat yourself:
  - Extract shared logic once it appears a second time with the same meaning.
  - Use shared building blocks: a common `Result` type, `ProblemDetails` mapping, endpoint filters for validation, pagination helpers, and base test fixtures.
  - Prefer framework and library features over hand-rolled ones.
- Keep it simple:
  - No speculative abstractions (generic repositories, factories, extra layers) until a real second use exists.
  - Small, single-purpose methods and classes, shallow nesting (early returns), low cyclomatic complexity.
  - Don't merge code that only looks alike but means different things. Duplication beats the wrong abstraction.
- Prefer clear names over comments. Comment only the why that the code can't show.

## Testing

- Use test-driven development, red-green-refactor:
  1. Write a failing test for the next behavior and watch it fail for the right reason.
  2. Write the minimum code that makes it pass.
  3. Refactor with the tests green.
  No production code without a failing test first.
- Every feature ships with both unit and integration tests.
- Unit tests: domain aggregates and application use cases in isolation, with repositories faked.
- Integration tests: drive the real HTTP pipeline through `WebApplicationFactory`, against a real database (Testcontainers SQL Server) seeded per test. Assert status codes, response bodies and persisted state.
- Port legacy stored procedure behavior as test cases so the new API matches the old rules.

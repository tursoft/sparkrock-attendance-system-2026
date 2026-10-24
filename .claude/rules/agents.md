# Parallel agents

Use parallel subagents whenever work splits into independent pieces. Examples:

- Analysing different legacy files at once
- Building separate layers or features that don't share in-progress files
- Writing tests for different aggregates
- Reviewing code along different dimensions

Launch independent agents in a single message so they run concurrently. Keep sequential work, such as a failing test followed by its implementation, in a single thread.

- Give each agent a narrow, explicit scope and the files it owns, so agents never edit the same file at once.
- Agents follow all project rules (architecture, TDD, `legacy/` read-only).
- Review and integrate agent results yourself before committing. Commits still follow the [git workflow](git.md).

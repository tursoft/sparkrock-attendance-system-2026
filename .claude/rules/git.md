# Git workflow

- Commit automatically after each meaningful, self-contained piece of work. Keep one topic per commit and split unrelated changes.
- **Never push.** The user pushes manually.
- Use Conventional Commits: `type(scope): subject`
  - Types: `feat`, `fix`, `docs`, `refactor`, `test`, `chore`, `build`, `ci`, `perf`, `style`
  - Imperative mood, lowercase, no trailing period, subject of 50 characters or less
  - Add a short body only when the reason isn't obvious

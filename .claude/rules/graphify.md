# Graphify knowledge graph

The project's knowledge graph is in `graphify-out/` (gitignored, regenerable). The `graphify` CLI isn't on PATH here, so run it as `python -m graphify`.

- Read `graphify-out/GRAPH_REPORT.md` before reading source files, running grep/glob, or answering codebase questions.
- If `graphify-out/wiki/index.md` exists, navigate it instead of raw files.
- For "how does X relate to Y" questions, use `python -m graphify query "<question>"`, `path "<A>" "<B>"`, or `explain "<concept>"` instead of grep.
- After code changes, run `python -m graphify update .` (AST-only, no API cost). Git post-commit/post-checkout hooks also rebuild it automatically.

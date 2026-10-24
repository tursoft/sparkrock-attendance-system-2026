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

# Decision log

Record every project decision (architecture, technology, scope, conventions) in [docs/decisions.md](../../docs/decisions.md):

- Add a numbered row with the date, the decision, the rationale, and the status (Accepted / Pending / Superseded by #n).
- Never delete a row. When a decision changes, mark the old row superseded and add a new one.
- Update it in the same turn as the work, and stage it in that work's commit, like `docs/ai-logs.md`.
- Ambiguities and missing details (`AMB-n`, `MIS-n`, or new ones found while building) are the user's call. Ask the user and never decide alone. Record each answer as a row, citing the IDs it resolves, and put the decision number in the **Resolution** column of [docs/01.legacy-analysis.md](../../docs/01.legacy-analysis.md). Proposals the user hasn't confirmed stay *Pending*.
- Never make a commit that only changes `docs/decisions.md` (alone or with `docs/ai-logs.md`). If a decision comes with no committable work, leave it uncommitted until the next work commit.

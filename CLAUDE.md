@AGENTS.md

## Claude Code notes

- Rules in `.claude/rules/` load automatically. `architecture.md` always loads; the others load when you work on files under their `paths:`. You do not need to open them by hand from the table above.
- The workflows in `.claude/skills/` are project skills (`create-feature`, `create-api-endpoint`, `ef-migration`, `run-local`, `review-code`). Invoke the matching one instead of improvising the steps.
- The dev machine is Windows. Bash is Git Bash and PowerShell is 5.1. Run `dotnet ef` from `src/backend/Infrastructure`. Start `dotnet run` and `pnpm dev` as background tasks, because they never exit.
- Before you report a task as finished, go through "Keep docs current" in `AGENTS.md`: compare your diff with the table and update the affected READMEs, rules, or skills. Then name the docs you touched in your final message.
- For reviews, `review-code` defines the MyLeague checklist and output format. `/code-review` is fine for generic bug hunting.
- Personal overrides go in `CLAUDE.local.md` and `.claude/settings.local.json`; both are gitignored.

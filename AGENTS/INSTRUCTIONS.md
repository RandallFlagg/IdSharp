# Agent Instructions

## Tool installation
- If a required tool is missing, **ask the user** before installing. Do not install tools on your own.
- **No `npm`**.
- **No `pip`** (the Python package manager).
- `uv` and `pnpm` are **allowed only if the user explicitly grants permission** for the specific use. Do not assume permission.

## Plan files
- Plan files live in `.kilo/plans/`.
- `phase2.md` (deferred work) lives at the repo root.

## Scope discipline
- Do not delete or un-skip tests. Adding a test or making one pass is fine.
- Fix bugs revealed by new tests when feasible. Only mark `[Ignore]` after a real fix attempt and consultation with the user.

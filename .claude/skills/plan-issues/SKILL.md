---
name: plan-issues
description: Turn the owner's feature plan into small, well-scoped GitHub issues labeled `claude` so /work-queue can implement them unattended.
argument-hint: <feature plan text>
---

# Plan issues from the owner's feature list

Input: $ARGUMENTS

1. Read `docs/design/research.md`, `docs/ARCHITECTURE.md` and `docs/ADDING_FEATURES.md`, plus the code the features touch, so each issue names real files and classes.
2. Split the plan into **small** issues that can each be merged independently (one coherent change, ideally under ~200 changed lines). The owner prefers many small issues over a few big ones. Put shared groundwork in its own issue first.
3. Ask the owner only about real product decisions (behaviour they'd notice). Settle technical details yourself and write them into the issue.
4. Create each issue with `gh issue create --label claude --label <enhancement|bug|documentation|testing> --label phase-N --title ... --body-file ...`. Body template:

```
## Goal
<one paragraph: what the user gets>

## Context
<relevant files/classes, current behaviour, code to copy from>

## Acceptance criteria
- [ ] ...
- [ ] Builds with 0 errors and 0 warnings
- [ ] Tests added/updated per docs/TESTING.md
- [ ] Docs/help text updated if user-facing

## Implementation notes
<approach, config fields, UI changes, edge cases>

## Dependencies
Depends on #X   (omit if none)

## Manual test plan
<steps the owner can run in-game, if applicable>
```

5. Reply with a table of the created issues (number, title, dependencies) and offer to start `/work-queue`.

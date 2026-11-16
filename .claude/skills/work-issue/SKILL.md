---
name: work-issue
description: Implement a single GitHub issue end-to-end with no human input — branch, code, build/test, merge to master, push, comment, close. Never opens a PR.
argument-hint: <issue-number>
---

# Work one issue: #$ARGUMENTS

Follow every step. Do not ask the user anything; this runs unattended. Re-read the **Principles** in CLAUDE.md.

## 1. Sync

```
git checkout master
git pull --ff-only origin master
git status --short          # must be clean; if not, stash with a descriptive name and note it in the issue
```

## 2. Understand the issue

- If `%APPDATA%\XIVLauncher\pluginConfigs\TextBasedRPG\selftest-results.json` exists, read it. A FAIL related to this issue's area is evidence. Compare its timestamp with `git log -1 --format=%ci`.
- `gh issue view $ARGUMENTS --comments`
- If it has `needs-human`, or depends on an open issue ("Depends on #X"), skip it and say why.
- Read the relevant docs (`docs/design/research.md` for anything touching rendering/FPS/HUD; `docs/ADDING_FEATURES.md`, `docs/CODING_STANDARDS.md`, `docs/TESTING.md` once they exist) and the code you'll touch.
- `gh issue edit $ARGUMENTS --add-label in-progress`

## 3. Branch and implement

- `git checkout -b issue/$ARGUMENTS-<short-slug>`
- Before writing new game-facing code, look for a Dalamud service or a trusted plugin's tested code that already does it (see CLAUDE.md Principles). Credit copied code.
- Implement the smallest complete change that satisfies the acceptance criteria. Put pure logic in `TextBasedRPG.Core` with unit tests.
- If behaviour depends on the live game, add an in-game self-test step and list its id in `docs/IN_GAME_TEST_PLAN.md`. Self-tests must never send anything to the game server.
- Update docs (README, `docs/*`, in-plugin help text) if behaviour or configuration changes.

## 4. Verify

- `dotnet build TextBasedRPG.slnx -c Release`: 0 errors, 0 warnings.
- `dotnet test TextBasedRPG.slnx -c Release --no-build`: all green.
- Re-read your diff (`git diff master...HEAD`) against the acceptance criteria. Fix anything missing.
- If you can't get it green after a few genuine attempts, comment what you tried, label `needs-human`, remove `in-progress`, switch back to master (leave the branch), and stop this issue.

## 5. Commit, merge, push

```
git add -A
git commit -m "<imperative summary> (#$ARGUMENTS)" -m "<details>" -m "Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>"
git checkout master
git merge --no-ff issue/$ARGUMENTS-<slug> -m "Merge issue #$ARGUMENTS: <title>" -m "Closes #$ARGUMENTS"
dotnet build TextBasedRPG.slnx -c Release      # sanity check on master
git push origin master
git branch -d issue/$ARGUMENTS-<slug>
```

Never `gh pr create`. Then `gh run list --limit 3`; if CI fails, fix it before the next issue.

## 6. Report on the issue

Post one comment with `gh issue comment $ARGUMENTS --body-file <file>` (write the file to the scratchpad):
- **What changed**: files and behaviour.
- **Commit**: merge commit SHA.
- **Verification**: build result and warning count, tests run.
- **Manual in-game test steps**: if behaviour can only be confirmed in FFXIV. If so, add the `needs-in-game-test` label.
- **Follow-ups**: link any new issues you created.

Then `gh issue edit $ARGUMENTS --remove-label in-progress --remove-label claude` and `gh issue close $ARGUMENTS` if it wasn't auto-closed.

## 7. Follow-up work you discover

- Bugs or necessary groundwork: create an issue labeled `claude` (+ `bug`/`enhancement`) so the queue picks it up.
- New feature ideas the owner didn't ask for: create an issue labeled `needs-human` (not `claude`) so the owner can approve it.

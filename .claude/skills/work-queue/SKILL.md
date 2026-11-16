---
name: work-queue
description: Work through every open GitHub issue labeled `claude`, oldest first, unattended, until the queue is empty; then summarize for the owner.
---

# Work the issue queue

Runs with no human input. Follow the Principles in CLAUDE.md.

1. **Get the queue**:
   `gh issue list --label claude --state open --json number,title,labels --limit 200`
   Drop issues labeled `needs-human` or `in-progress` (an `in-progress` issue may be left over from an interrupted run; if no local branch for it has work in progress, clear the label and include it). Sort ascending by number. Respect "Depends on #X": do the dependency first, and skip the issue if the dependency is blocked.

2. **For each issue**, run the full `/work-issue <number>` procedure (`.claude/skills/work-issue/SKILL.md`). Finish one issue completely (merged and pushed, or labeled `needs-human`) before starting the next.

3. **Re-query** after each issue, since work may have created new `claude` issues. Continue until none remain.

4. **Final report to the owner** (in chat):
   - Issues completed (number, title, merge SHA)
   - Issues needing in-game testing, with their test steps linked
   - Issues blocked (`needs-human`) and the exact question for each
   - Suggested features filed for approval

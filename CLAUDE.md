# CLAUDE.md

**Text-Only Mode for FFXIV** (internal name `TextBasedRPG`) is a Dalamud plugin that turns FFXIV into a "text-based RPG" (the joke behind `/trpg`): one toggle ("text-only mode") turns off 3D rendering and hides the whole HUD except the chat window, and caps the frame rate, so the game uses very little CPU while the player is only chatting. The connection stays alive, sound keeps playing, and the chat stays fully usable. Toggle it off to play normally again.

Repo: `pearstopher/ffxiv-text-based-rpg` (owner's own project). Ships from the owner's combined custom repo `pearstopher/ffxiv-plugins`.

The full research and design behind the plugin is in [docs/design/research.md](docs/design/research.md). Read it before touching rendering, the frame limiter or the HUD hider.

## Principles (owner's instructions)

1. **Use Dalamud first.** If Dalamud has a service for it (`IFramework`, `ICommandManager`, `IDtrBar`, `IChatGui`, `ICondition`, `IClientState`, `IGameGui`, `ISelfTestRegistry`, config save/load, `WindowSystem` …), use it. Never imitate or reimplement Dalamud functionality, in code or in tests.
2. **Copy trusted, tested code before writing your own.** Base work on respected plugins (ECommons, ChillFrames, Simple Tweaks, SamplePlugin). Credit the source in a comment and in [THIRD_PARTY_NOTICES.md](THIRD_PARTY_NOTICES.md); respect its license.
3. **Simplicity and performance.** The plugin's whole purpose is to save CPU. Don't add per-frame work that isn't needed; throttle anything that doesn't need to run every frame.
4. **No custom signatures or code patches.** Use FFXIVClientStructs fields/functions (updated by Dalamud each patch). If something truly needs a signature, label the issue `needs-human` first.
5. **Leave nothing behind.** Every change to the game is in memory only and is undone on mode off, logout and plugin unload. Never change the game's saved settings (`IGameConfig` writes) without the owner's approval.

## Autonomous workflow (no human input)

The owner gives feature ideas; Claude turns them into GitHub issues and works through them unattended.

- **`/plan-issues <idea(s)>`**: turn the owner's plan into small, well-scoped issues labeled `claude`.
- **`/work-issue <number>`**: implement one issue end-to-end (branch → code → build/test → merge to master → push → comment → close).
- **`/work-queue`**: loop over every open `claude` issue (oldest first) until the queue is empty, then report back.

| Label | Meaning |
|---|---|
| `claude` | Queued for autonomous work |
| `in-progress` | Claude is currently working on it |
| `needs-human` | Blocked on a decision only the owner can make; skip it and keep going |
| `needs-in-game-test` | Merged, but behaviour must be verified in FFXIV by the owner |
| `testing` | Test infrastructure or coverage |
| `phase-0` … `phase-4` | Plan phase (see docs/design/research.md) |

Don't stop to ask questions mid-queue. If a decision is truly the owner's to make, comment on the issue, label it `needs-human`, and move on. Make reasonable, documented choices for everything else.

## Build & verify

```powershell
dotnet build TextBasedRPG.slnx -c Release             # must be 0 errors
dotnet test TextBasedRPG.slnx -c Release --no-build    # all tests green
```

- Requires the .NET 10 SDK and Dalamud dev files at `%AppData%\XIVLauncher\addon\Hooks\dev` (present on this machine). `Dalamud.NET.Sdk` version is pinned in `global.json` (Dalamud API 15).
- Pure logic lives in `TextBasedRPG.Core` (no Dalamud/game references) so it can be unit-tested. Game glue lives in `TextBasedRPG`.
- Claude can't run FFXIV. Game-dependent behaviour gets an **in-game self-test** (`TextBasedRPG/SelfTests/`), registered with Dalamud's `ISelfTestRegistry` and run by the plugin's dev runner, plus manual steps and the `needs-in-game-test` label.
- **Read the owner's latest in-game results** at `%APPDATA%\XIVLauncher\pluginConfigs\TextBasedRPG\selftest-results.json` before and after working on game-dependent code. A FAIL there is a bug to fix. Results older than the last relevant merge are stale.
- **Warning baseline: 0 warnings.** Don't add warnings.
- CI: `.github/workflows/build.yml` builds and tests on push to `master`, on demand, and daily. After pushing, check it with `gh run list --limit 3`; fix a red run before starting the next issue. Never push release tags (`v*`) unless the owner asks for a specific release.
- In-game dev testing (loading the dev DLL, debug commands, reading `/xllog`): [DEVELOPMENT.md](DEVELOPMENT.md).
- Every feature gets a row in [docs/DALAMUD_RULES.md](docs/DALAMUD_RULES.md).

## Project docs

- [docs/design/research.md](docs/design/research.md): feasibility research, chosen mechanisms, rejected approaches, sources
- [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md), [docs/CODING_STANDARDS.md](docs/CODING_STANDARDS.md), [docs/TESTING.md](docs/TESTING.md), [docs/ADDING_FEATURES.md](docs/ADDING_FEATURES.md)
- [docs/COMMANDS.md](docs/COMMANDS.md) / [docs/SETTINGS.md](docs/SETTINGS.md): user-facing references, kept current by tests
- [DEVELOPMENT.md](DEVELOPMENT.md): building, loading in game, debug commands, CI, releasing

(Some of these are created by the phase-0 issues.)

## Git conventions

- Branch per issue: `issue/<number>-<short-slug>`, from an up-to-date `master`. Branches stay local and are deleted after merge.
- Commit messages: imperative summary, reference the issue `(#N)`, end with the Co-Authored-By trailer.
- Merge with `git merge --no-ff` and a message containing `Closes #N`, then `git push origin master`. No pull requests.
- Keep `<Version>` in the csproj unchanged unless an issue asks for a version bump (tags `vX.Y.Z` must match `X.Y.Z.0`). Patch bumps for fixes, minor for new features.
- Plugin identity: **Text-Only Mode for FFXIV** outside the game (README, GitHub releases); just **Text-Only Mode** inside the game (manifest `<Name>`, so the plugin installer; window title, server info bar, chat), InternalName `TextBasedRPG` (also the repo, projects, namespaces and settings folder), commands `/trpg` and `/trpgtest`. The mode is called "text-only mode"; the "text-based RPG" joke stays in the tagline, the plugin description and `/trpg`. Don't change it unless the owner asks.

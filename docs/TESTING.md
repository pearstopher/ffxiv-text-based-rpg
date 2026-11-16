# Testing

Claude can build and run unit tests but can't run FFXIV. So there are two layers, plus manual checks.

## 1. Unit tests (`TextBasedRPG.Tests`)

- Every Core class has tests. Pure logic (timings, sleep calculation, visibility policy, state machine, parsing, CPU math, report JSON) lives in Core precisely so it can be tested.
- Repository checks: release version format (`ReleaseVersionTests`), docs list every command and setting (`DocumentationTests`).
- Never imitate Dalamud in tests. If something only works inside Dalamud, it belongs in an in-game self-test.
- Run: `dotnet build TextBasedRPG.slnx -c Release`, then `dotnet test TextBasedRPG.slnx -c Release --no-build`.

## 2. In-game self-tests (`TextBasedRPG/SelfTests`)

- Each check is a Dalamud `ISelfTestStep` (with an id and a detail message), registered with `ISelfTestRegistry`, so it also shows in Dalamud's own Self-Test window.
- Dev builds run them with `/trpgtest` or automatically after each load, and write `%APPDATA%\XIVLauncher\pluginConfigs\TextBasedRPG\selftest-results.json`. Claude reads that file.
- Self-tests must **never send anything to the game server**, and must restore anything they change (in `CleanUp()` too).
- Measurement tests (CPU/FPS) pass whenever they complete; the numbers are in the detail.
- Every self-test id is listed in [IN_GAME_TEST_PLAN.md](IN_GAME_TEST_PLAN.md).

## 3. Manual checks

Behaviour only a person can judge (what the screen looks like, how typing feels) goes in the issue's manual test plan and the [in-game test plan](IN_GAME_TEST_PLAN.md), and the issue gets the `needs-in-game-test` label.

## Using the owner's results

Before and after working on game-dependent code, read the results file. Compare its timestamp and build line with the last relevant merge; older results are stale. A FAIL is a bug to fix.

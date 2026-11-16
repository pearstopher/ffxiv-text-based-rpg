# Development guide

How to build the plugin, load it into the game as a **dev plugin**, and check that a change works. Written for the owner and for Claude (who can build and unit-test, but can't run the game).

## 1. One-time setup

- **XIVLauncher with Dalamud.** Launch the game through XIVLauncher once, so Dalamud's files exist in `%APPDATA%\XIVLauncher\addon\Hooks\dev\`. The build reads Dalamud's DLLs from there. If XIVLauncher lives elsewhere, set `DALAMUD_HOME` to its `addon\Hooks\dev` folder.
- **The .NET 10 SDK.** `dotnet --list-sdks` should show `10.0.x` (`winget install Microsoft.DotNet.SDK.10` otherwise).
- **Python 3** (only for regenerating the icon and the release script).

## 2. Build

From the repo root:

```sh
dotnet build TextBasedRPG.slnx                          # Debug: TextBasedRPG\bin\x64\Debug\TextBasedRPG.dll
dotnet build TextBasedRPG.slnx -c Release               # Release: ...\bin\x64\Release\TextBasedRPG\latest.zip
dotnet test TextBasedRPG.slnx -c Release --no-build     # unit tests (after a Release build)
```

A good build ends with `0 Warning(s)` and `0 Error(s)`.

## 3. Load it in game (first time only)

1. Start the game through XIVLauncher.
2. `/xlsettings` → **Experimental** → **Dev Plugin Locations**: paste the **full path to the Debug DLL**, click **+**, then **Save and Close**:
   ```
   C:\Users\<you>\Documents\VSCode\ffxiv-text-based-rpg\TextBasedRPG\bin\x64\Debug\TextBasedRPG.dll
   ```
3. `/xlplugins` → **Dev Tools → Installed Dev Plugins** → enable **Text-Based RPG**.
4. Recommended: in the dev plugin's settings, turn on **automatic reloading**. Dalamud then reloads the plugin shortly after the DLL changes, so `dotnet build` is enough.

> Point the location at the **Debug** DLL. `dotnet build` only updates `bin\x64\Debug\`.

## 4. Check which build is running

`/xllog`, filter on `TextBasedRPG`. On every load the plugin logs:

```
Text-Based RPG loaded (Debug build, built 2026-11-16 10:36:02, from C:\...\bin\x64\Debug\TextBasedRPG.dll)
```

and `Text-Based RPG unloaded` when disabled. **Check this line each time you test.** If the build time is older than your last build, the game is running a stale DLL. Red `ERR` lines mentioning the plugin are bugs. The same log is in `%APPDATA%\XIVLauncher\dalamud.log`.

Settings file: `%APPDATA%\XIVLauncher\pluginConfigs\TextBasedRPG.json`. Delete it to start fresh. The plugin's other files (self-test results, debug dumps) are in `%APPDATA%\XIVLauncher\pluginConfigs\TextBasedRPG\`.

## 5. Edit → test loop

1. Change the code, then `dotnet build TextBasedRPG.slnx`.
2. With auto-reload on, the plugin reloads itself. Otherwise click **reload** in Installed Dev Plugins.
3. Confirm the new `loaded (...)` time in `/xllog`.
4. Test in game, watching `/xllog`.

If the build fails because the DLL is locked, disable the plugin in game, build, and re-enable.

## 6. Debug commands

Dev builds only, hidden from `/xlhelp`.

| Command | What it does |
|---|---|
| `/trpgtest` | Runs the in-game self-tests (see §7). |
| `/trpgdebug` | Lists the debug subcommands below. |

## 7. In-game self-tests (dev builds)

The plugin's in-game checks are registered with Dalamud's own self-test system (`/xldev` → **Self-Test**), and dev builds can also run them by themselves:

- `/trpgtest` runs them all, prints a summary in chat, and writes **`%APPDATA%\XIVLauncher\pluginConfigs\TextBasedRPG\selftest-results.json`**. Claude reads that file, so there's nothing to copy.
- With **Run self-tests after each load** on (dev builds, default on), `dotnet build` → auto-reload → the tests run once you're logged in.
- The tests never send anything to the game server.

The list of tests is in [docs/IN_GAME_TEST_PLAN.md](docs/IN_GAME_TEST_PLAN.md).

## 8. CI

[.github/workflows/build.yml](.github/workflows/build.yml) builds and tests on every push to `master`, on demand, and **daily against the latest Dalamud**, so a game/Dalamud patch that breaks the plugin shows up as a failed run (GitHub emails the owner). `gh run list --limit 3` shows recent runs.

## 9. Releasing

Players install from the owner's combined custom repo, [pearstopher/ffxiv-plugins](https://github.com/pearstopher/ffxiv-plugins):

```
https://raw.githubusercontent.com/pearstopher/ffxiv-plugins/main/pluginmaster.json
```

That repo lists every `pearstopher` repo with the **`dalamud-plugin`** topic (this one has it) that has a **`repo.json`** on its default branch. `repo.json` is created by the first release.

**One-time:** the repo secret **`PLUGIN_REPO_TOKEN`** (the same token the other plugin repos use) must exist so the release can notify `ffxiv-plugins`: `gh secret set PLUGIN_REPO_TOKEN -R pearstopher/ffxiv-text-based-rpg`. Without it the release still publishes, but the last step fails; you can then run *Update plugin list* manually in `ffxiv-plugins`.

**To release** (the owner decides when; Claude never pushes release tags unless asked):

1. `master` has everything, CI is green, the in-game checks pass.
2. Set `<Version>` in `TextBasedRPG/TextBasedRPG.csproj` to the new version plus `.0` (`v1.0.0` → `1.0.0.0`); commit and push. Patch bumps for fixes, minor for new features.
3. `git tag v1.0.0` then `git push origin v1.0.0`.
4. [.github/workflows/release.yml](.github/workflows/release.yml) checks the version, builds, tests, publishes the GitHub Release with `latest.zip`, writes `repo.json` on `master`, and notifies `ffxiv-plugins`.

If the version check fails, nothing was published: delete the tag (`git push origin :v1.0.0`, `git tag -d v1.0.0`), fix the csproj, and tag again.

## Troubleshooting

| Problem | Fix |
|---|---|
| Build error about Dalamud references | Run the game with Dalamud once, or set `DALAMUD_HOME`. |
| Plugin not listed under Installed Dev Plugins | The dev location must point at the `.dll` file itself, and it must exist. |
| My change didn't take effect | Compare the `loaded (...)` build time with your last build. Point the dev location at Debug. |

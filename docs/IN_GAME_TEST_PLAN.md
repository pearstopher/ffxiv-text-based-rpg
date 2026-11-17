# In-game test plan

## 0. Self-tests

Run with `/trpgtest` (dev builds), or let them run after each load. Results: `%APPDATA%\XIVLauncher\pluginConfigs\TextBasedRPG\selftest-results.json`. They also appear in Dalamud's Self-Test window (`/xldev`).

<!-- self-tests:start -->
| Id | Checks |
|---|---|
| `env-loaded` | The plugin loaded; reports the running build and whether a character is logged in. Proves the self-test pipeline and results file work. |
| `chat-visible` | Logged in: the chat window (`ChatLog`) and its main panel (`ChatLogPanel_0`) are loaded, visible and drawn. |
| `render-flag` | Waits until no zone load or cutscene is running, sets the game's 3D render flag, checks it reads back on the next frame, and restores the original value. (The screen may flash for one frame.) |
| `restore-on-off` | Turns the mode on for about 30 frames (at least one UI element must get hidden), then off, and checks every UI element that was drawn before is drawn again, our 3D-off request is gone and the frame-rate cap is off. Skipped (pass) if you already have the mode on. |
| `mode-cycle` | Turns the mode on; after 2 s checks for 3 s: 3D is off, no UI element but the chat is drawn, the chat is drawn, and the frame rate is near the idle cap (≤ 1.5 × cap + 1). Then turns it off and checks our 3D-off request is gone (notes if another plugin still keeps 3D off). Skipped (pass) if you already have the mode on. Don't type during it. |
| `measure-3d` | Measurement (~25 s): CPU % and FPS for 10 s with 3D on, then 3 s settle and 10 s with 3D off (render flag only), then 3D back on. Passes whenever it completes; the numbers and the saving are in the detail. Fails only if 3D can't be turned back on. Don't touch the game while it runs. |
| `measure-mode` | Measurement (~30 s): CPU % and FPS for 10 s of normal play, then the full text-based RPG mode (3 s settle, 15 s measured), then the mode off. Passes whenever it completes; the detail shows both periods and the saving. This is the number the plugin exists for. Skipped (pass) if you already have the mode on. Don't type during it. |
<!-- self-tests:end -->

The whole run takes about 1½ minutes (most of it the two measurements). Stand somewhere quiet, logged in, and don't type or move while it runs. The screen will go blank and come back a few times; that's the tests.

## 1. First session: setup

1. Load the Debug DLL as a dev plugin ([DEVELOPMENT.md §3](../DEVELOPMENT.md#3-load-it-in-game-first-time-only)), with auto-reload on.
2. `/xllog`, filter `TextBasedRPG`: check the `loaded (Debug build, built …)` line matches the latest build.
3. Wait for the self-tests to finish by themselves (chat: `Text-Based RPG self-tests: N/N passed.`). Nothing to copy: Claude reads the results file.
4. `/trpgdebug addons` once with your normal HUD (Claude uses `addons.json` to check the hiding rules).

## 2. Manual checklist

Tick each and note anything odd in the issue (or just tell Claude).

**Phase 1 proof (3D off alone)**
- [ ] `/trpgdebug 3d off`: world becomes a flat colour; HUD and chat still drawn. `/trpgdebug 3d on` brings it back.

**Entering and leaving**
- [ ] `/trpg`: world and all UI except the chat disappear; chat shows the "mode on" message.
- [ ] `/trpg` again: everything back (hotbars, minimap, party list, nameplates, server info bar, world).
- [ ] Click **TRPG** in the server info bar: mode on. `/trpg off`: back.
- [ ] `/trpg status` with the mode on and off: sensible values.

**Chatting in the mode**
- [ ] Press Enter (or click the chat input) and type: text appears normally, without lag that bothers you.
- [ ] Send in `/echo`, then a real channel with a friend (`/say`, `/fc`, `/tell`, linkshell).
- [ ] Incoming messages appear while idle (10 FPS is enough to read).
- [ ] Switch chat tabs, scroll the log, use auto-translate (Tab) if you use it. Note any pop-up that's missing.
- [ ] Extra chat windows (if you use them) stay visible.

**Settings and CPU**
- [ ] `/trpg settings`: change idle/typing FPS; the feel changes; values are kept after a reload.
- [ ] Untick "Show TRPG in the server info bar": entry disappears; tick: back.
- [ ] After using the mode for a bit, the window shows `Normal: x% · Text mode: y%`. Compare with Task Manager if you like.

**Edge cases**
- [ ] Mode on, then teleport or enter a house: no crash; the mode stays on after the load (3D switches a few seconds after loads).
- [ ] Mode on, log out: title screen normal; log back in: normal game, mode off.
- [ ] Mode on, disable the plugin in `/xlplugins`: everything back.
- [ ] Alt-tab away and back in the mode: nothing changes.
- [ ] If you use AutoRetainer or Mini: both still work; turning our mode off doesn't turn 3D on while they want it off.

## 3. Reporting

- Self-test results are read from the results file automatically.
- For manual checks, comment on the related issue (they carry the `needs-in-game-test` label: `gh issue list --label needs-in-game-test`) or tell Claude what you saw. A screenshot of anything odd helps.
- Errors: `/xllog` red lines mentioning TextBasedRPG, or attach `%APPDATA%\XIVLauncher\dalamud.log`.

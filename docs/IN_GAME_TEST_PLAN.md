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
| `measure-3d` | Measurement (~25 s): CPU % and FPS for 10 s with 3D on, then 3 s settle and 10 s with 3D off (render flag only), then 3D back on. Passes whenever it completes; the numbers and the saving are in the detail. Fails only if 3D can't be turned back on. Don't touch the game while it runs. |
<!-- self-tests:end -->

## 1. Manual checklist

(Filled in by #40.)

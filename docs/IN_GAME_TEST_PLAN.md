# In-game test plan

## 0. Self-tests

Run with `/trpgtest` (dev builds), or let them run after each load. Results: `%APPDATA%\XIVLauncher\pluginConfigs\TextBasedRPG\selftest-results.json`. They also appear in Dalamud's Self-Test window (`/xldev`).

<!-- self-tests:start -->
| Id | Checks |
|---|---|
| `env-loaded` | The plugin loaded; reports the running build and whether a character is logged in. Proves the self-test pipeline and results file work. |
| `chat-visible` | Logged in: the chat window (`ChatLog`) and its main panel (`ChatLogPanel_0`) are loaded, visible and drawn. |
| `render-flag` | Waits until no zone load or cutscene is running, sets the game's 3D render flag, checks it reads back on the next frame, and restores the original value. (The screen may flash for one frame.) |
<!-- self-tests:end -->

## 1. Manual checklist

(Filled in by #40.)

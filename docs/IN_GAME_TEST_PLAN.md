# In-game test plan

## 0. Self-tests

Run with `/trpgtest` (dev builds), or let them run after each load. Results: `%APPDATA%\XIVLauncher\pluginConfigs\TextBasedRPG\selftest-results.json`. They also appear in Dalamud's Self-Test window (`/xldev`).

<!-- self-tests:start -->
| Id | Checks |
|---|---|
| `env-loaded` | The plugin loaded; reports the running build and whether a character is logged in. Proves the self-test pipeline and results file work. |
<!-- self-tests:end -->

## 1. Manual checklist

(Filled in by #40.)

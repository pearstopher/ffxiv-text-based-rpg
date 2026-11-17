# Settings

Change them in the settings window (`/trpg settings`). Saved by Dalamud in `%APPDATA%\XIVLauncher\pluginConfigs\TextBasedRPG.json`. Values outside their range are corrected when the plugin loads.

| Setting | Default | Range | What it does |
|---|---|---|---|
| `IdleFps` | 10 | 1–30 | Frame-rate cap while the mode is on and you're not typing. Lower saves more CPU; the chat updates a little less smoothly. |
| `TypingFps` | 30 | 5–60 | Frame-rate cap while you type in chat, and for 3 seconds after, so typing stays responsive. Never lower than `IdleFps`. |
| `ShowDtrEntry` | true | | Shows the **TRPG** entry in the server info bar (click it to turn the mode on). |
| `DevRunSelfTestsOnLoad` | true | | Dev builds only: run the in-game self-tests once after each load, when a character has been logged in for 3 seconds (gives up after 5 minutes without a login). Ignored by release builds. |
| `Version` | 1 | | Settings format version, used to migrate old settings. Not shown in the UI. |

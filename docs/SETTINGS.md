# Settings

Change them in the settings window (`/trpg settings`). Saved by Dalamud in `%APPDATA%\XIVLauncher\pluginConfigs\TextBasedRPG.json`. Values outside their range are corrected when the plugin loads.

| Setting | Default | Range | What it does |
|---|---|---|---|
| `IdleFps` | 10 | 1–30 | Frame-rate cap while the mode is on and you're not typing. Lower saves more CPU; the chat updates a little less smoothly. |
| `TypingFps` | 30 | 5–60 | Frame-rate cap while you type in chat, and for 3 seconds after, so typing stays responsive. Never lower than `IdleFps`. |
| `ShowDtrEntry` | true | | Shows the **TRPG** entry in the server info bar (click it to turn the mode on). |
| `AutoOnWhenAfk` | false | | **Turn on when I go AFK**: only applies while the game marks you AFK (the "Away from Keyboard" status). If the mode is off when you go AFK, it turns on, and off again when you're no longer AFK. If the mode was already on, it stays on either way. If you switch the mode yourself while AFK, it's left as you set it; turning it off while AFK sticks until the next time you go AFK. |
| `WorldOffOnly` | false | | **3D world off only (HUD and frame rate unchanged)**: when on, turning the mode on only switches the 3D world off; the HUD stays visible and the frame rate isn't capped. Off (default) is full text-only mode. Read when the mode is switched on, so a change applies the next time. |
| `DevRunSelfTestsOnLoad` | true | | Dev builds only: run the in-game self-tests once after each load, when a character has been logged in for 3 seconds (gives up after 5 minutes without a login). Ignored by release builds. |
| `Version` | 1 | | Settings format version, used to migrate old settings. Not shown in the UI. |

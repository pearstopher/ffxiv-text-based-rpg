# Text-Based RPG

A Dalamud plugin for FFXIV that turns your game into a text-based RPG.

Type `/trpg` and the 3D world and the whole HUD disappear. Only the chat window stays, and the game's frame rate drops, so it uses very little CPU while you're just chatting with friends. Your connection stays alive, you can keep talking in every chat channel, and sound keeps playing. Type `/trpg` again to go back to playing.

> 🚧 Not released yet. Install instructions below apply once the first release is out.

<!-- Screenshot: add images/screenshot.png (the game in text-based RPG mode) and link it here. -->

## Install

1. In game, open `/xlsettings` → **Experimental**.
2. Under **Custom Plugin Repositories**, add:
   ```
   https://raw.githubusercontent.com/pearstopher/ffxiv-plugins/main/pluginmaster.json
   ```
   then click **+** and **Save and Close**.
3. Open `/xlplugins`, search for **Text-Based RPG** and install it.

## Use

| | |
|---|---|
| `/trpg` | Turn text-based RPG mode on or off. |
| `/trpg on` / `/trpg off` | Turn it on / off. |
| `/trpg status` | Is it on? Frame-rate cap, CPU use. |
| `/trpg settings` | Settings window. |
| **TRPG** in the server info bar | Click to turn the mode on. |

In the mode:

- **Hidden:** the 3D world, and every UI element except the chat window (hotbars, map, party list, nameplates, the server info bar, pop-ups…).
- **Still working:** your connection, the chat in every channel, sound.
- **Frame rate:** capped at 10 FPS, or 30 FPS while you type (both adjustable).

Type `/trpg` to come back. Logging out also turns the mode off, and it always starts off.

Full reference: [commands](docs/COMMANDS.md) · [settings](docs/SETTINGS.md).

## FAQ

**Will I be kicked for being AFK?**
The plugin doesn't change the game's AFK timer. Typing in chat counts as activity, the same as when you play normally.

**How much CPU does it save?**
It depends on your PC. The settings window shows your own numbers after you've used the mode for a few seconds (`Normal: x% · Text mode: y%`).

**Other plugins' windows are still showing.**
Dalamud plugin windows are drawn on top of the game, so they aren't hidden. Close them, or use the plugin's own hide option.

**Does it work with AutoRetainer, Mini, or other plugins that turn off rendering?**
Yes. It uses the same shared system as those plugins (ECommons' render manager), so they don't fight: the 3D world stays off while any of them wants it off.

**Something didn't come back after I left the mode.**
Disable and re-enable the plugin in `/xlplugins`, which restores everything, and please open an issue.

## How it works

The game has its own "skip 3D rendering" switch (it still draws the UI); the plugin turns it on, hides every UI element but the chat, and sleeps a little each frame to cap the frame rate. Nothing is sent to the server and none of your game settings are changed. Details and sources: [docs/design/research.md](docs/design/research.md).

## Development

See [DEVELOPMENT.md](DEVELOPMENT.md) and [docs/](docs/README.md).

## License

AGPL-3.0-or-later. Includes code adapted from other projects; see [THIRD_PARTY_NOTICES.md](THIRD_PARTY_NOTICES.md).

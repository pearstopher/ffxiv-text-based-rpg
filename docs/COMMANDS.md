# Commands

| Command | What it does |
|---|---|
| `/textrpg` | Main command. See the options below. |
| `/trpg` | Short for `/textrpg`. |

## Options

| Typed | What it does |
|---|---|
| `/trpg` (or `/trpg toggle`) | Turns text-based RPG mode on or off. |
| `/trpg on` / `/trpg off` | Turns the mode on / off. |
| `/trpg status` | Shows whether the mode is on, whether 3D is off, the frame-rate cap, how many UI elements are hidden, and CPU use and FPS. |
| `/trpg settings` (or `config`) | Opens or closes the settings window (also the plugin installer's settings button). |
| `/trpg help` (or `?`) | Lists the commands. |

Anything else prints the list of commands.

## Server info bar

The **TRPG** entry in the server info bar (top right of the screen) turns the mode on when clicked. While the mode is on the server info bar is hidden like the rest of the HUD, so type `/trpg` to come back.

## What the mode does

- **On:** the 3D world is no longer drawn, every UI element except the chat window is hidden, and the frame rate is capped low (raised while you type). You stay connected, chat works normally in every channel, and sound keeps playing. Needs a logged-in character.
- **Off:** everything comes back exactly as it was.
- If you turn it on or off during a loading screen or cutscene, the 3D part switches a few seconds after it ends (switching during a load can crash the game). The HUD and frame rate switch right away.
- The mode always starts off when the game or plugin starts, and turns off when you log out.

Developer commands (dev builds only) are in [DEVELOPMENT.md](../DEVELOPMENT.md#6-debug-commands).

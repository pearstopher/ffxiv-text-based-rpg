# Dalamud rules compliance

This plugin is distributed from the owner's **custom repository** (`pearstopher/ffxiv-plugins`). We still record, for every feature, whether it follows [Dalamud's plugin restrictions](https://dalamud.dev/plugin-publishing/restrictions), so any risk is visible.

The rules that matter here:

- **R1:** A plugin must not do anything a human player couldn't do.
- **R2:** A plugin must not interact with the game servers automatically or outside of specification.
- **R3:** A plugin must not collect account IDs of other players' characters.
- **R4:** A plugin must not be useful *only* in out-of-spec scenarios.

This plugin only changes what the client draws and how fast it runs. It never sends anything to the server, and it doesn't touch the AFK timer.

## Features

| Feature | Follows the rules? | Notes |
|---|---|---|
| Load/unload build log line | ✅ Yes | Local log only. |
| Text-based RPG mode (`/trpg`): 3D off, HUD hidden except chat, FPS cap | ✅ Yes | Changes only what the client draws and how often it runs a frame (like the game's own frame-rate limit and hide-UI). Nothing is sent to the server; the AFK timer is untouched. 3D is turned off through the same shared request system AutoRetainer and Mini use. |
| Debug commands (`/trpgdebug`, dev builds) | ✅ Yes | Local only: read CPU/FPS, change what the client draws. |
| In-game self-tests (Dalamud Self-Test window; dev runner in dev builds) | ✅ Yes | Local checks only. Never send anything to the server; restore anything they change. |

## Adding a feature

Add a row. If a feature would break a rule, it needs an owner-approved issue first.

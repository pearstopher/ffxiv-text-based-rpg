# Architecture

The plugin has one job: switch **text-only mode** on and off. While it's on, 3D rendering is off, every UI element except the chat is hidden, and the frame rate is capped. The design reasons are in [design/research.md](design/research.md).

## Projects

| Project | What goes in it |
|---|---|
| `TextBasedRPG.Core` | Pure logic with no Dalamud or game references: when the render flag may change, how long to sleep, which addons to hide, mode state, command parsing, CPU math, self-test reports. Unit-tested. |
| `TextBasedRPG` | The Dalamud plugin: services, game memory access through FFXIVClientStructs, commands, UI, self-tests. Thin glue that asks Core what to do. |
| `TextBasedRPG.Tests` | xunit tests for Core, plus repository checks (docs, release version). |

## Services (plugin project)

Each lands in its own issue; this table is updated as they're added.

| Class | Role |
|---|---|
| `Plugin` | Entry point. Creates everything, disposes everything (in reverse order, each part safely). |
| `Service` | Dalamud services, injected by Dalamud. |
| `Configuration` | Saved settings through Dalamud's config API. |
| `RenderController` | `Want3DOff` set by the mode; each frame applies it through `RenderDisableManager` once `TransitionGuard` (Core) allows: never during a zone load/cutscene or within 3 s after. Holds the current state until then. |
| `RenderDisableManager` | Adapted from ECommons (MIT): cooperative requests for `Render.Manager.Is3DRenderingDisabled`, shared through Dalamud's `GetOrCreateData` with every ECommons plugin (AutoRetainer, Mini, …). 3D is off while any plugin has a request. |
| `RenderFlag`, `GameState`, `UiAddons` | Small read helpers: the render flag (debug/self-tests), "zone load or cutscene in progress", loaded UI addons. |
| `FrameLimiter` | `TargetFps` (0 = off). Sleeps at the end of each `IFramework.Update` for `FramePacer.SleepMs` (Core) to cap the frame rate (ChillFrames technique, no busy-wait). |
| `HudHider` | While active, hides the root node of every drawn addon that `HudVisibilityPolicy` (Core) says to hide (all but the chat), re-scanning every 250 ms; when turned off, shows exactly the nodes it hid that are still loaded under the same name. |
| `ModeController` | Switches the three parts together (on: 3D off, FPS cap, HUD hidden; off: in reverse order) after `ModeState` (Core) allows the request; resets the CPU average on each switch. |
| `CpuMonitor` | Counts frames and samples process CPU % once per second (`CpuSampler` in Core). |
| Self-tests (planned) | `ISelfTestStep`s registered with Dalamud's `ISelfTestRegistry`, plus a dev runner that writes the results file. |

## Threading

Everything that touches game memory runs on the framework thread (`IFramework.Update`, command handlers, `RunOnFrameworkThread`). Per-frame work is kept to cheap checks; anything heavier (addon scans, CPU sampling) is throttled.

## Leaving nothing behind

All changes are in memory. Mode off, logout and plugin unload undo them: show the hidden nodes, remove the render request, stop the frame cap. Nothing is written to the game's saved settings.

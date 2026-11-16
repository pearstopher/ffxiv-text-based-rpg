# Research and design: chat-only mode

Researched 2026-11-16, before any code was written. Approved by the owner the same day.

## Goal

One toggle ("text-based RPG mode") that:

- turns off all graphics except the in-game chat window,
- cuts the game's CPU use as far as possible,
- keeps the game connection, sound and chat working exactly as when playing,
- is undone completely by toggling it off (or by unloading the plugin).

Out of scope (owner decision): pop-ups, prompts and other UI while the mode is on. The player is only chatting; to check anything else they toggle the mode off. Sound stays on.

The plugin never changes the game's AFK timer. Typing in chat counts as activity, the same as when playing.

## Chosen mechanisms

### 1. 3D off: `Render.Manager.Is3DRenderingDisabled`

FFXIVClientStructs documents a flag in the game's render manager:

```csharp
// FFXIVClientStructs/FFXIV/Client/Graphics/Render/Manager.cs
[FieldOffset(0x38358)] public bool Is3DRenderingDisabled;
```

From its PR (aers/FFXIVClientStructs#1834): "Setting this to true will take the render short path: It clears the back buffer with a solid color, runs post-effects and then returns, skipping all 3D world rendering." The author reports GPU load of about 0–3% over two months of use. **The UI is still drawn** (XIV_HolyPlugin: "turns off 3D rendering (UI stays)"), so the chat window stays visible and usable.

In use by respected plugins:

| Plugin | How |
|---|---|
| AutoRetainer (PunishXIV) | Multi mode, through ECommons `RenderDisableManager` |
| Mini (NightmareXIV) | While minimized, through ECommons `RenderDisableManager` |
| MasterOfPuppets | Moved from RenderTrim-style byte patches to this flag |
| Henchman, XIV_HolyPlugin, DailyRoutines, BundleOfTweaks | Direct writes |

**Implementation:** copy ECommons' `RenderDisableManager` (MIT; the maintainer says it is self-contained and may be copy-pasted, NightmareXIV/ECommons#175). It shares requests with other plugins through Dalamud's `GetOrCreateData` (`ECommons.RenderDisableRequests`, `ECommons.RenderDisableProcessingFramecount`), so we cooperate with AutoRetainer/Mini instead of fighting them over the flag.

**Safety:** XIV_HolyPlugin reports that *flipping* the flag while the render thread loads a zone or cutscene can crash the game (`SetPixelShader` on the render thread). AutoRetainer *holds* the flag through zone changes and logins without problems. So: only change the flag when not `BetweenAreas`/`BetweenAreas51`/cutscene conditions, and not within ~3 s after one ends. Holding it is fine.

### 2. Frame cap: sleep in `IFramework.Update` (ChillFrames)

With 3D off, each frame is very cheap, so an uncapped game would run hundreds of frames per second and keep the CPU busy. The frame cap is the biggest CPU lever and is required.

ChillFrames (MidoriKami, official repo) caps FPS by sleeping in `IFramework.Update`:

```csharp
var delayTime = (int)(targetFrametime - timer.ElapsedMilliseconds);
if (delayTime - 1 > 0) Thread.Sleep(delayTime - 1);
while (timer.ElapsedTicks <= preciseFrameTickTime) { }   // busy-wait for precision
```

We copy the sleep, but **not** the busy-wait: precision doesn't matter here and spinning burns CPU, the opposite of our goal. Default cap: low (about 10 FPS) while idle, raised (about 30 FPS) while the chat input is active (`AtkModule.IsTextInputActive()`), so typing stays responsive.

### 3. HUD hidden, chat kept: `RootNode->ToggleVisibility` (Simple Tweaks)

Simple Tweaks' "Hide Chat" tweak hides chat with `unitBase->RootNode->ToggleVisibility(false)` on `ChatLog` and `ChatLogPanel_0..3`. We do the reverse: hide every other loaded addon's root node and keep the chat addons. This only affects drawing; the game's own show/hide logic and state are untouched, and we restore exactly the nodes we hid. Addons that the game re-creates (e.g. after a zone change) are re-checked on a throttle, not every frame.

### 4. Toggle

A chat command (the chat stays visible, so the player can always type it to leave the mode), plus a server info bar entry (`IDtrBar`) to enter it.

### Measuring

Nobody has published CPU numbers for this. The plugin measures its own effect: process CPU % (from `Process.TotalProcessorTime` deltas, normalised by core count) with the mode off and on. Self-tests record it in the results file, so savings are measured, not guessed.

## Rejected approaches

| Approach | Why not |
|---|---|
| Skip the whole DX11 frame (Dhog Potato System's no-render gate on `Device` post-tick) | The UI is not drawn either, so chat freezes. Needs a custom signature. |
| RenderTrim byte patches (`cmp [Render::Manager+0x3834C], -1` → 1) | Code patches with custom signatures break on game patches; ClientStructs' flag does the same job without patching. RenderTrim notes that writing 0 to that field also suppresses the UI. |
| Per-subsystem renderer hooks (RenderTrim/DPS trims) | Many signatures, several "unverified"/"tradeoff"; not needed once 3D is off. Possible later, only if measurements show leftover CPU. |
| Lowering game graphics settings through `IGameConfig` | Changes the player's saved settings; a crash would leave them changed. Not needed. |
| Game's hide-UI (Scroll Lock) | Hides the chat too. |

## Sources

- FFXIVClientStructs `Render/Manager.cs` and PR #1834 — https://github.com/aers/FFXIVClientStructs
- ECommons `RenderDisableManager` and issue #175 — https://github.com/NightmareXIV/ECommons
- AutoRetainer `MultiMode.cs` — https://github.com/PunishXIV/AutoRetainer
- Mini `RenderSuppressionManager.cs` — https://github.com/NightmareXIV/Mini
- ChillFrames `FrameLimiterController.cs` — https://github.com/MidoriKami/ChillFrames
- Simple Tweaks `Tweaks/Chat/HideChat.cs` — https://github.com/Caraxi/SimpleTweaksPlugin
- XIV_HolyPlugin `RenderingHandler.cs` — https://github.com/AlexFlipnote/XIV_HolyPlugin
- Henchman `Tweaks/Rendering.cs` — https://github.com/Knightmore/Henchman
- MasterOfPuppets `GameRenderManager.cs` — https://github.com/zunetrix/MasterOfPuppets
- Dhog Potato System — https://github.com/McVaxius/DPS
- RenderTrim — https://github.com/thesupporthero/Rendertrim
- Dalamud `ISelfTestRegistry`, `IFramework`, `IDtrBar` — https://github.com/goatcorp/Dalamud

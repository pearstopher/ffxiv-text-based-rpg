# Third-party notices

This plugin adapts code and techniques from the projects below. Each adapted file says so in a comment.

| Project | License | Used for |
|---|---|---|
| [ECommons](https://github.com/NightmareXIV/ECommons) by NightmareXIV | MIT | `TextBasedRPG/RenderDisableManager.cs`: cooperative requests for the game's 3D render flag, shared with other ECommons plugins; unique instance id |
| [ChillFrames](https://github.com/MidoriKami/ChillFrames) by MidoriKami | AGPL-3.0 (same as this plugin) | `TextBasedRPG.Core/FramePacer.cs` and `TextBasedRPG/FrameLimiter.cs`: capping the frame rate by sleeping in `IFramework.Update` (`FrameLimiterController.PerformLimiting`), without its busy-wait |
| [Simple Tweaks](https://github.com/Caraxi/SimpleTweaksPlugin) by Caraxi and contributors | AGPL-3.0 (same as this plugin) | Technique: `TextBasedRPG/HudHider.cs` hides addons with `RootNode->ToggleVisibility`, as the "Hide Chat" tweak (`Tweaks/Chat/HideChat.cs`) does; the chat addon names in `TextBasedRPG.Core/ChatAddons.cs` come from it |
| [XIV_HolyPlugin](https://github.com/AlexFlipnote/XIV_HolyPlugin) by AlexFlipnote | MIT | Technique only (no code copied): `TextBasedRPG.Core/TransitionGuard.cs` waits out zone loads/cutscenes plus 3 s before changing the render flag, as its `RenderingHandler` does |

## ECommons — MIT License

```
MIT License

Copyright (c) 2023 NightmareXIV

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
```

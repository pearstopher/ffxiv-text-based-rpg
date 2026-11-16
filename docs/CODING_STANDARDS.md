# Coding standards

## Language & project

- C# (latest), `net10.0` (Core/Tests) and the Dalamud SDK's target (plugin). Nullable reference types on. Annotate honestly; `null!` only for `[PluginService]` properties.
- **0 warnings.** Core treats warnings as errors. Don't add `<NoWarn>`.
- New NuGet packages need a reason in the issue. Dalamud already provides FFXIVClientStructs, ImGui bindings, Newtonsoft.Json and Lumina: don't add those as packages.

## Style

- File-scoped namespaces, Allman braces, 4-space indentation, LF line endings (`.editorconfig`).
- `PascalCase` types/members/constants, `camelCase` private fields and locals (no `_` prefix).
- `var` when the type is obvious. XML doc comments on members you add; comments say **why**.
- One type per file; file name matches the type.
- No commented-out code.

## Rules

- **Dalamud first** (CLAUDE.md principle 1). Use `Service.*`; don't stash `IDalamudPluginInterface` elsewhere.
- **Copied code** keeps a header comment naming the source repo, file and license, and gets a row in `THIRD_PARTY_NOTICES.md`.
- **No signatures, hooks or code patches** without an owner-approved issue. Use FFXIVClientStructs.
- **Pure logic goes in Core** so it can be tested. The plugin side reads game state, calls Core, applies the result.
- **Performance:** the plugin exists to save CPU. No allocations or LINQ in per-frame paths; throttle scans; don't log every frame.
- **Game memory** only on the framework thread. Check pointers for null; the game can unload addons at any time.
- **Every subscription has a matching unsubscription** in `Dispose` (framework events, commands, DTR entries, client-state events).
- **Undo everything** the plugin changed in the game when the mode turns off, on logout and on unload.

## Configuration compatibility

- New properties need a default initializer; older JSON must still load.
- Don't rename or remove saved properties. If you must, migrate and bump `Configuration.Version`.
- `Save()` right after changing settings. Clamp values on load (Core).

## Logging

- `Service.Log` (`IPluginLog`). `Debug` for flow, `Information` for rare noteworthy events (mode on/off, load/unload), `Warning` for recoverable problems, `Error` with the exception for failures.
- Never log chat message contents.

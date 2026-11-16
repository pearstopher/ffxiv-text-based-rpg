# Adding a feature

1. **Look for existing work first.** A Dalamud service? A trusted plugin that already does it (ECommons, ChillFrames, Simple Tweaks, SamplePlugin)? Copy and credit it rather than writing new code.
2. **Split** pure logic (Core, unit-tested) from game glue (plugin project).
3. **Performance check:** what does it cost per frame? Throttle anything that doesn't need every frame.
4. **Undo path:** what does it change in the game, and how is that undone on mode off, logout and unload?
5. **Tests:** unit tests for Core; an in-game self-test if it depends on the game (add its id to IN_GAME_TEST_PLAN.md).
6. **Docs:** COMMANDS.md / SETTINGS.md for anything user-facing (tests enforce this), ARCHITECTURE.md for a new service, a DALAMUD_RULES.md row, README if players should know.
7. **Copied code:** header comment + THIRD_PARTY_NOTICES.md row.
8. Build with 0 warnings, all tests green.

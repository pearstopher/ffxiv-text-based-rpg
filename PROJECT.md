# Project Specs

## Goal

- A Dalamud plugin that turns FFXIV into a "text-based RPG": one toggle removes all graphics except the in-game chat to massively reduce CPU load while the player isn't playing, only chatting with friends. (The name is a joke: it turns a regular RPG into a text-based one.)

## Features

- Toggle mode on/off easily (chat command, server info bar).
- While on: 3D rendering off, entire HUD hidden except the chat window, frame rate capped low (higher while typing).
- Connection stays alive and chat works exactly as when playing. Sound stays on.
- Nothing is left changed when the mode is turned off, the player logs out or the plugin unloads.
- Shows how much CPU the mode saves.

## Tech stack

- C# Dalamud plugin (`Dalamud.NET.Sdk`, API 15), FFXIVClientStructs via Dalamud.
- Dalamud functions whenever possible; code copied from trusted plugins before writing our own (see docs/design/research.md).

## End Result

- Player installs from the owner's custom repo (`pearstopher/ffxiv-plugins`), types `/trpg`, and the game becomes a chat window that barely uses the CPU. `/trpg` again to play.

# how-to-FISH-HACK

# Angler's Toolkit v1.0.0

A quality-of-life and cheat menu for **How to Fish**.

- **Game:** How to Fish (Unity Mono)
- **Loader:** BepInEx 5.4.23.5 x64 (included)
- **Menu key:** `F8`
- **Plugin ID:** `dazed.howtofish.anglerstoolkit`

The menu uses a modern sidebar layout with switches, sliders, cards, and a live catch
overlay. It is designed to be readable at 1080p and works with the game's built-in
developer cheats enabled.

---

## Installation

### Option A: Extract into the game folder (recommended)

1. Close **How to Fish**.
2. Extract the contents of this zip directly into the game folder — the folder that
   contains `How to Fish.exe`.
3. Launch the game.
4. Press **F8** to open or close the menu.

The final layout should look like this:

```text
How to Fish/
├─ How to Fish.exe
├─ winhttp.dll
├─ doorstop_config.ini
├─ .doorstop_version
└─ BepInEx/
   ├─ core/
   └─ plugins/
      └─ AnglersToolkit/
         └─ AnglersToolkit.dll
```

If BepInEx is already installed, you can instead copy only:

```text
BepInEx/plugins/AnglersToolkit/AnglersToolkit.dll
```

into the game's existing `BepInEx/plugins/AnglersToolkit` folder.

### Option B: Use the install script

Extract the zip somewhere safe, open PowerShell in the extracted folder, and run:

```powershell
powershell -ExecutionPolicy Bypass -File .\install.ps1
```

Enter the full path to the game folder when prompted. The script copies the loader
and plugin into that folder. Close the game first.

---

## Features

### Fishing
- **Auto Reel** — automatically reels in anything that bites.
- **Catch HUD** — shows the hooked item's name, weight, value, distance, and shiny/boss tags.
- **Instant Bite** — fish bite as soon as the bait is underwater. *Host only.*
- **Always Shiny** — newly spawned creatures are the rare shiny variant. *Host only.*
- **Auto Kill Catches** — caught creatures arrive dead and ready to pick up. *Host only.*
- **Auto Collect** — picks up dead catches when they are close. *Host only.*
- **Weight Multiplier** — multiplies the randomized weight of new creatures from 1× to 50×.
- **Sell Value Multiplier** — multiplies the value used when items are sold from 1× to 50×.

### Money
- View your dollar and euro balances.
- Add money in convenient amounts: $1,000 up to $1,000,000, or €100 up to €100,000.
  *Host only.*

### Unlocks
- Unlock all islands.
- Unlock the boat, radar, and motor.
- Unlock the grill.
- Kill the current boss. *Danger: this is permanent for the current boss fight.*
- Unlock all skins, achievements, and complete the fish journal.
- God Mode and One Shot toggles are on the **Player** tab.

### Spawn
- Search all items and creatures loaded from the game's `Resources/Items` folder.
- Spawn the selected prefab 2.2 m in front of the camera.
- Spawn creatures dead for immediate pickup.
- The list shows whether an entry is a creature, boss, or item type.

### Player
- Heal and feed your local character.
- Travel to the previous or next island. *Host only.*
- Toggle God Mode and One Shot. *Host only.*

### Built-in developer cheats
The plugin keeps the game's own developer cheats enabled. These also work outside the menu:

| Key / command | Effect |
|---|---|
| `M` | Add $99,999 |
| `Ctrl+M` | Add €999 |
| `N` | Remove money |
| `O` | Next island |
| `Shift+O` | Random mini island |
| `T` | Toggle slow motion |
| `G` | Test damage |
| `H` | Heal |
| `/spawn <name>` | Spawn an item/creature |
| `/spawndead <name>` | Spawn a dead creature |
| `/spawndrip <name>` | Spawn a shiny creature |
| `/addmoney` | Add money |
| `/nextisland` | Travel to the next island |
| `/previsland` | Travel to the previous island |
| `/godmode` | Toggle god mode |
| `/oneshot` | Toggle one-shot damage |
| `/killboss` | Kill the current boss |
| `/allskins` | Unlock all skins |
| `/unlockachievements` | Unlock all achievements |
| `/finishgame` | Finish the game |

---

## Host vs. client

Server-side actions require you to be **hosting a lobby or playing solo**. In client
mode, the menu still opens, but host-only buttons show a warning and do nothing.
This is intentional: FishNet does not allow clients to run server-authoritative
actions such as spawning, changing weights, or adding money.

---

## Configuration

Settings are saved to:

```text
BepInEx/config/dazed.howtofish.anglerstoolkit.cfg
```

The menu hotkey can be changed there under `[General] -> Menu Hotkey`. You can also
turn the built-in developer cheats on or off with `Enable Developer Cheats`.

---

## Building from source

The `source` folder contains the complete C# project:

```text
source/
├─ AnglersToolkit.csproj
├─ Plugin.cs
├─ Patches.cs
├─ MenuActions.cs
└─ MenuGUI.cs
```

Requirements:
- .NET SDK 8 or newer
- A local copy of **How to Fish** (for `Assembly-CSharp.dll`, FishNet, and UnityEngine references)

Build:

```powershell
dotnet build .\source\AnglersToolkit.csproj -c Release
```

If the game is not in the default SteamRip location, pass the game folder:

```powershell
dotnet build .\source\AnglersToolkit.csproj -c Release -p:GameDir="C:\Path\To\How to Fish"
```

The compiled DLL is written to:

```text
source/bin/Release/netstandard2.1/AnglersToolkit.dll
```

Copy it to `BepInEx/plugins/AnglersToolkit/` in the game folder.

---

## Troubleshooting

### The menu does not open

- Make sure the game is fully loaded before pressing `F8`.
- Check `BepInEx/LogOutput.log` for:
  - `Loading [Angler's Toolkit 1.0.0]`
  - `Harmony patches applied.`
  - `Angler's Toolkit 1.0.0 loaded.`
- Verify that `winhttp.dll`, `doorstop_config.ini`, and the `BepInEx` folder are in the
  same folder as `How to Fish.exe`.
- If the hotkey was changed, check `BepInEx/config/dazed.howtofish.anglerstoolkit.cfg`.

### Buttons say host only

Start a solo game or host a lobby. Client-side lobbies cannot run server-authoritative
cheat actions.

### The GUI is too small or too large

The menu sizes itself from the game's reported DPI and screen resolution. If it looks
wrong after changing resolution, close and reopen the menu with `F8`.

### Save safety

This is a cheat mod. Back up your save before using unlock, boss, or money options.
The mod does not add its own save file; it uses the game's existing save system.

---

## Notes

- Built and tested against the SteamRip release of **How to Fish** with Unity 6000.4.4.
- BepInEx 5.4.23.5 x64 is included for convenience.
- The plugin only runs in the local game process. It does not modify the game's
  installation files beyond adding the loader and plugin.
- Use at your own risk, especially in multiplayer lobbies. Host-only actions are
  guarded, but other players may still notice spawned items or modified catches.

Enjoy the fishing trip.

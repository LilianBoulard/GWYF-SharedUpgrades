# Shared Upgrades

A small BepInEx 5 mod for **Gamble With Your Friends** v1.0.7 that turns upgrade items into a team resource: when any player consumes an Upgrade item (Gambler's Confidence, Insurance, Stakeholder, or Bonus Draw), the same upgrade is applied to every other player in the lobby.

> **Host-side only.** The mod hooks a server-only method, so it only has an effect when the **lobby host** has it installed. Players who don't have the mod can still join a host who does - they'll receive shared upgrades just like everyone else, with no client install needed.

---

## Installation

### 1. Install BepInEx 5

1. Download **BepInEx 5+ (win x64)** (tested with 5.4.23.5) from the [official releases page](https://github.com/BepInEx/BepInEx/releases).
2. Extract the zip directly into your game folder. You can find it via Steam -> right-click *Gamble With Your Friends* -> **Manage** -> **Browse local files**. After extraction the folder should contain a `BepInEx/` directory and a `winhttp.dll` next to `Gamble With Your Friends.exe`.
3. Launch the game once from Steam, wait until the main menu, then quit. This generates `BepInEx/config/` and confirms BepInEx loaded (look for `Chainloader started` in `BepInEx/LogOutput.log`).

### 2. Install Shared Upgrades

1. Download the latest release zip from this repo's releases page or the [Nexus mod page](https://www.nexusmods.com/gamblewithyourfriends/mods/8).
2. Extract it into your game folder. The internal layout is `BepInEx/plugins/SharedUpgrades/SharedUpgrades.dll`, so extracting at the game root drops the DLL in the right place.
3. Launch the game. Open `BepInEx/LogOutput.log` and look for:
   ```
   [Info   :Shared Upgrades] Shared Upgrades 1.0.0 loaded (enabled=True).
   ```

---

## Configuration

After your first launch, edit `<game>/BepInEx/config/com.lilianb.sharedupgrades.cfg` as needed.

Config is reloaded on next launch.

---

## How it works

Upgrades in this game are stored server-side in a `Dictionary<ulong, PlayerUpgradeData>` keyed by Steam ID. When you consume an Upgrade item, `UpgradeManager.ChangeUpgradeData(yourSteamId, type, amount)` runs on the host. The mod adds a Harmony postfix that, after the original method runs, iterates every player in the lobby and re-invokes `ChangeUpgradeData` with their Steam ID, letting the existing per-player math (including Insurance's diminishing-returns formula) run naturally for each player. The host's own `RpcOnDataChanged` then syncs each player's UI through the game's normal networking.

A re-entrancy flag prevents the postfix from recursing into the calls it generates.

---

## Building from source

You need **.NET 8 SDK** on Windows.

```powershell
# From the repo root
dotnet build src\SharedUpgrades.csproj -c Release
```

The build automatically deploys the DLL to `<GameDir>\BepInEx\plugins\SharedUpgrades\`. The default `GameDir` is hard-coded in `src/SharedUpgrades.csproj`; override on the command line if your install lives elsewhere:

```powershell
dotnet build src\SharedUpgrades.csproj -c Release -p:GameDir="D:\Steam\steamapps\common\Gamble With Your Friends"
```

### Producing a release zip

```powershell
.\scripts\Build-Release.ps1
```

Builds in Release mode and writes a versioned zip to `dist/SharedUpgrades-<version>.zip`.

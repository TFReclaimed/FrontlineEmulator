# Frontline Server Emulator

This is a server emulator for the cancelled Titanfall card collection game, Frontline.
Versions 1.0.15816, 1.0.16585 and 1.0.17334 are supported with later versions requiring newer asset bundles and API changes.
(If you still have the game installed, please contact me so we can back them up!)
This project is a work in progress and is not yet feature complete.

## How to play

### Unofficial official server

1. Download a supported version of the game. (Latest one recommended)
   - [1.0.17334](https://apkpure.com/titanfall-frontline/com.nexonm.tffl/download/1.0.17334)
   - [1.0.16585](https://apkpure.com/titanfall-frontline/com.nexonm.tffl/download/1.0.16585)
   - [1.0.15816](https://www.apkmirror.com/apk/nexon-m-inc/titanfall-frontline/titanfall-frontline-1-0-15816-release/)
2. Create a patched APK with the default options using [FrontlinePatcher](https://github.com/TFInternal/FrontlinePatcher).
3. Install the APK on your device.

Note: The official server is currently only accessible to whitelisted users.
This is to prevent users with the game still installed from overwriting their files.

### Your own server

You will need to create your own patched APK with [FrontlinePatcher](https://github.com/TFInternal/FrontlinePatcher).
Use the `--frontline-url` and `--opentoy-url` options to point to your server's URL.

The server requires `ruleset.json` and `PvEData.json` files to be present in the root directory.
Use AssetRipper to extract these files from the Unity project.

## Features

- ✅ FUE (Tutorial)
- ✅ Authentication
  - ✅ Registration/login 
  - ✅ Get user data
  - ✅ Update profile
- ✅ Store
  - ✅ Get products
  - ✅ Purchase
    - Works, however due to session header not being present, can't implement securely
- ✅ Chat
  - ✅ Frontier
  - ✅ Guild
  - ✅ Private messages (Challenges)
  - ✅ Persistence
- ✅ Hangar
  - ✅ Booster packs 
    - ✅ Get booster packs
    - ✅ Open booster pack
  - ✅ Cards
    - ✅ Get cards
    - ✅ Retire card
  - ✅ Dropships
    - ✅ Get dropships
    - ✅ Edit dropship
    - ✅ Set active dropship
    - ✅ Custom dropships
- ✅ Battles
  - ✅ Matchmaking
    - ✅ Create game request
    - ✅ Cancel game request
  - ✅ Mulligan
  - ✅ Deploy
  - ✅ Move
  - ✅ Attack
  - ✅ End turn
  - ✅ Surrender
  - ✅ Emote
  - ✅ Match results
- 🚧 Missions
  - ✅ Get mission list
  - 🚧 Start mission
    - Missing various checks with some likely being incorrect
  - ✅ Cancel mission
  - ✅ Complete mission
  - ✅ Revive card
  - ⛔ Reputation
    - I have no idea how this mechanic is supposed to work
  - ⛔ Supply auto generation
- ✅ Leaderboards
  - ✅ PvP
  - ⛔ PvE
    - I don't think you can even access this in the game
- ✅ Guilds
  - ✅ Search guilds
  - ✅ Create guild
  - ✅ Join guild
  - ✅ Leave guild
  - ✅ Kick from guild
  - ✅ Change member rank
  - ✅ Edit guild
  - ✅ XP gift
# Frontline Server Emulator

This is a server emulator for the cancelled Titanfall card collection game, Frontline.
Versions 1.0.15816, 1.0.16585 and 1.0.17334 are supported with later versions requiring newer asset bundles and API changes.

## How to play

### Unofficial official server

Download a pre-patched version of Frontline from [our website](https://tfflinternal.com/download).

### Your own server

You will need to create your own patched APK with [FrontlinePatcher](https://github.com/TFInternal/FrontlinePatcher).
Modify the StreamingAssets/serverAddresses.json file with your own address.

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
- ✅ Missions
  - ✅ Get mission list
  - ✅ Start mission
  - ✅ Cancel mission
  - ✅ Complete mission
  - ✅ Revive card
  - ⛔ Reputation
    - Not enough clues in the game code on how to implement this
  - ✅ Supply auto gain
- ✅ Leaderboards
  - ✅ PvP
- ✅ Guilds
  - ✅ Search guilds
  - ✅ Create guild
  - ✅ Join guild
  - ✅ Leave guild
  - ✅ Kick from guild
  - ✅ Change member rank
  - ✅ Edit guild
  - ✅ XP gift

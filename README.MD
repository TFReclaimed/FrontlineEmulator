# Frontline Server Emulator

This is a server emulator for the cancelled Titanfall card collection game, Frontline.
Currently the server only supports version 1.0.15816 of the game due to the asset bundles not being backed up.
(If you still have the game installed, please contact me so we can back them up!)
This project is a work in progress and is not yet feature complete.

## How to play

1. Download the 1.0.15816 APK from [APKMirror](https://www.apkmirror.com/apk/nexon-m-inc/titanfall-frontline/titanfall-frontline-1-0-15816-release/).
2. Install the APK on your device.

No further changes needed as we're in control of the original server domain and S3 bucket!

## Features

- ✅ Asset hosting
- ✅ FUE (Tutorial)
- ✅ Authentication
  - ✅ Registration/login 
  - ✅ Get user data
  - ✅ Update profile
- 🚧 Store
  - ✅ Get products
  - 🚧 Purchase (Session header not present, can't implement securely)
- ⛔ Chat
  - ⛔ Frontier
  - ⛔ Guild
- 🚧 Hangar
  - 🚧 Booster packs 
    - ✅ Get booster packs
    - 🚧 Open booster pack
  - ⛔ Cards
    - 🚧 Get cards
    - ⛔ Retire card
  - ⛔ Dropships
    - ⛔ Get dropships
    - ⛔ Set active dropship
- ⛔ Matchmaking
  - ⛔ Create game request
  - ⛔ Cancel game request
- ⛔ Missions
- 🚧 Leaderboards
  - ✅ PvP
  - ⛔ PvE
  - ⛔ AWS S3 upload
- 🚧 Guilds
  - ✅ Search guilds
  - ✅ Create guild
  - ✅ Join guild
  - ✅ Leave guild
  - ✅ Kick from guild
  - ✅ Change member rank
  - ✅ Edit guild
  - ⛔ XP gift

## Credits

[@snuutti](https://github.com/snuutti) - Bulk of the work

[AssetRipper](https://github.com/AssetRipper/AssetRipper) - Great asset extraction tool!

[PDALIFE](https://pdalife.com/titanfall-frontline-android-a23450.html) - Seemingly the only place to have backed up the assets
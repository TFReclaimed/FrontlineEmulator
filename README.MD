# Frontline Server Emulator

This is a server emulator for the cancelled Titanfall card collection game, Frontline.
Versions 1.0.15816, 1.0.16585 and 1.0.17334 are supported with later versions requiring newer asset bundles.
(If you still have the game installed, please contact me so we can back them up!)
This project is a work in progress and is not yet feature complete.

## How to play

### Unofficial official server

1. Download a supported version of the game. (Latest one recommended)
   - [1.0.17334](https://apkpure.com/titanfall-frontline/com.nexonm.tffl/download/1.0.17334)
   - [1.0.16585](https://apkpure.com/titanfall-frontline/com.nexonm.tffl/download/1.0.16585)
   - [1.0.15816](https://www.apkmirror.com/apk/nexon-m-inc/titanfall-frontline/titanfall-frontline-1-0-15816-release/)
2. Install the APK on your device.

No further changes needed as we're in control of the original server domain and S3 bucket!

### Your own server

You'll need to either modify the `assets/serverAddresses.json` file in the APK to point to your server
or modify your DNS/hosts file to point to your server.

The following domains need to be pointed to your server:
- prod-us-east-1-gameserver-lb.tfflinternal.com
- prod-us-east-1-store-lb.tfflinternal.com
- prod-us-east-1-virtu2-lb.tfflinternal.com
- prod-us-east-1-chat-lb.tfflinternal.com
- conference.prod-us-east-1-chat-lb.tfflinternal.com
- pc-tffl-leaderboard.s3-website-us-east-1.amazonaws.com
- pc-tffl-html.s3-website-us-east-1.amazonaws.com

You will also need to modify the TOY server address contained in the `AWS_SERVER` variable
in `smali/kr/co/nexon/toy/api/request/NXToyRequestType.smali` to point to your server.
Alternatively point `m-api.nexon.com` to your server.

The server requires `ruleset.json` and `PvEData.json` files to be present in the root directory.
Use AssetRipper to extract these files from the Unity project.

## Features

- ✅ Asset hosting
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
  - ⛔ Persistence
  - ⛔ TLS
    - Not required, but would be nice to have. From my testing the game always tries to use TLSv1.0 and OpenSSL doesn't support this.
- 🚧 Hangar
  - 🚧 Booster packs 
    - ✅ Get booster packs
    - 🚧 Open booster pack
  - ✅ Cards
    - ✅ Get cards
    - ✅ Retire card
  - 🚧 Dropships
    - ✅ Get dropships
    - ✅ Edit dropship
    - ✅ Set active dropship
    - ⛔ Unlock as you level up
- ⛔ Matchmaking
  - ⛔ Create game request
  - ⛔ Cancel game request
- 🚧 Missions
  - ✅ Get mission list
  - 🚧 Start mission
    - Missing various checks with some likely being incorrect
  - ✅ Cancel mission
  - 🚧 Complete mission
    - Cards are not yet rewarded
    - What is a synergy?
  - ✅ Revive card
  - ⛔ Reputation
    - I have no idea how this mechanic is supposed to work
- 🚧 Leaderboards
  - ✅ PvP
  - ⛔ PvE
    - I don't know if you even can access this in the game
  - ⛔ AWS S3 upload
    - I'd prefer not to wake up to a 1k€ bill so just update the serverAddresses.json file
- ✅ Guilds
  - ✅ Search guilds
  - ✅ Create guild
  - ✅ Join guild
  - ✅ Leave guild
  - ✅ Kick from guild
  - ✅ Change member rank
  - ✅ Edit guild
  - ✅ XP gift

## Credits

[@snuutti](https://github.com/snuutti) - Bulk of the work

[AssetRipper](https://github.com/AssetRipper/AssetRipper) - Great asset extraction tool!

[PDALIFE](https://pdalife.com/titanfall-frontline-android-a23450.html) - Seemingly the only place to have backed up the assets
# Frontline Server Emulator

This is a server emulator for the cancelled Titanfall card collection game, Frontline.
Currently the server only supports version 1.0.15816 of the game due to the asset bundles not being backed up.
(If you still have the game installed, please contact me so we can back them up!)
This project is a work in progress and is not yet feature complete.

## How to play

### Unofficial official server

1. Download the 1.0.15816 APK from [APKMirror](https://www.apkmirror.com/apk/nexon-m-inc/titanfall-frontline/titanfall-frontline-1-0-15816-release/).
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

If you didn't modify the APK, you'll need to listen on port 443 with a valid SSL certificate.
A reverse proxy, such as Caddy or Nginx can be used to achieve this.

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
- ⛔ Chat
  - ⛔ Frontier
  - ⛔ Guild
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
  - ✅ Revive card
- 🚧 Leaderboards
  - ✅ PvP
  - ⛔ PvE
  - ⛔ AWS S3 upload
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
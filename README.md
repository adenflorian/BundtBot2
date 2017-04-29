BundtBot
========

I am Bundt

Clone repo

[Visual Studio Code](https://code.visualstudio.com/) is the recommended editor

Run `node setup.js` (Only the dev bot token is required to run locally)

Run `gulp run` to build and run locally

Run `gulp test` to run the tests

If you have a remote Ubuntu 16.04 server setup with ssh, and you have the proper fields in secret.json filled out, then you can run `gulp setup-server` to setup that server

Then `gulp deploy` to deploy and run, then `gulp rlogs` to monitor

Joining servers
---------------

Use this link to add your bot to servers that you are an owner of. You must replace the `BOT_CLIENT_ID` part with your bot's client id which you get from [here](https://discordapp.com/developers/applications/me).

`https://discordapp.com/oauth2/authorize?&client_id=BOT_CLIENT_ID&scope=bot&permissions=70376448`

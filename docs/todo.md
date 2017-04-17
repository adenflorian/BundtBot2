# BundtBot
[√] Fix the gateway SendAsync from sending messages when internet is disconnected
  - not sure if possible to fix because of nature of UDP
  - fixed using retry logic
[√] handle opcode 9 invalid session
[√] make LogError and LogCritical use Console.Error.Write

[√] change trace json log to output raw json

[√] hello world voice
  [√] create handler for !hello in BundtBot
  [√] voice hello world
    - bundtbot should join the voice channel of the requesting user and play a clip that says, "bundtbot said, Hello, world"

[ ] !yt
[ ] !dog

[ ] make a json file to configure what actions bundtbot does on startup
  - for running tests and such

[ ] edit the same message continuously to do a ascii art animation

## Voice
[√] put the voice udp client on its own thread
[ ] need to disconnect from voice properly when leaving a voice channel

# BundtBot Web API
[√] show last logged exception in website
[ ] make logs accessable from webserver
  - like how many of each exception
  - when each exception was last thrown
[ ] make guilds display on website again
[ ] lock down website
[ ] change log level
[ ] change dog level

# gulp
[ ] instead of deleting previous version on test server, store it in a previous version folder so we can quickly rollback if need be
[ ] get more logging from node do tar and grunt sftp:deploy
  - i want to see progress logs

# build system
[ ] find out how to make a smaller build

# vscode
[ ] make it so when i hit f5, if the build errors then it doesn't continue to run the previous successful build
[ ] can't run task because it says a task is running, but i dont see one running
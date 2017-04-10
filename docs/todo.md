# BundtBot
[√] Fix the gateway SendAsync from sending messages when internet is disconnected
  - not sure if possible to fix because of nature of UDP
  - fixed using retry logic
[√] handle opcode 9 invalid session
[√] make LogError and LogCritical use Console.Error.Write

[√] change trace json log to output raw json

[ ] !hello
  [√] create handler for !hello in BundtBot
  [ ] voice hello world
    - bundtbot should join the voice channel of the requesting user and play a clip that says, "Hello, world! This is bundtbot. No relation to the cake"

[ ] !yt

[ ] make a json file to configure what actions bundtbot does on startup

[ ] edit the same message continuously to do a ascii art animation

# BundtBot Web API
[√] show last logged exception in website
[ ] make logs accessable from webserver
  - like how many of each exception
  - when each exception was last thrown
[ ] make guilds display on website again
[ ] lock down website
[ ] change log level
[ ] change dog level

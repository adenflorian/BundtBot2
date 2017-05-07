# BundtBot ToDo

- [x] make command stuff its own project
- [x] allow playing audio to two different servers at once
  - works but has issues
- [x] more fine grained logging control
  - be able to change log levels by class
  - be able to define in a json
- [ ] allow multiple different text responses to certain events
  - maybe utilize BundtFig for this
- [ ] edit the same message continuously to do a ascii art animation

## Bugs

- [ ] not handling new users to the server correctly

## Tech Debt

- [ ] make websocket not throw exception when closing
- [ ] look into Cake build system

## TesterBot

- [x] Make TesterBot
  - For doing functional testing on BundtBot
  - Will be a separate discord bot
- [ ] add some asserts

## BundtFig

- [x] make a json file to configure what actions bundtbot does on startup
  - for running tests and such
- [ ] have a global config file with defaults

## Gateway

- [ ] Dispatch
  - [ ] Ready
  - [ ] Resumed
  - [ ] Channel Create
  - [ ] Channel Update
  - [ ] Channel Delete
  - [ ] Guild Create
  - [ ] Guild Update
  - [ ] Guild Delete
  - [ ] Guild Ban Add
  - [ ] Guild Ban Remove
  - [ ] Guild Emojis Update
  - [ ] Guild Integrations Update
  - [ ] Guild Member Add
  - [ ] Guild Member Remove
  - [ ] Guild Member Update
  - [ ] Guild Members Chunk
  - [ ] Guild Role Create
  - [ ] Guild Role Update
  - [ ] Guild Role Delete
  - [ ] Message Create
  - [ ] Message Update
  - [ ] Message Delete
  - [ ] Message Delete Bulk
  - [ ] Presence Update
  - [ ] Typing Start
  - [ ] User Update
  - [ ] Voice State Update
  - [ ] Voice Server Update
- [ ] Heartbeat
- [ ] Identify
- [ ] StatusUpdate
- [ ] VoiceStateUpdate
- [ ] VoiceServerPing
- [ ] Resume
- [ ] Reconnect
- [ ] RequestGuildMembers
- [ ] InvalidSession
- [ ] Hello
- [ ] HeartbeatAck

## Optimizations

- [ ] make easy way for measuring stuff

## !commands

- [x] !bugreport
- [x] !dog
- [ ] !restart
  - restart the bot
- [ ] !uploaddog
- [ ] !setloglevel
- [ ] multiple command names
- [ ] print similar commands when someone typos a command
- [ ] auto select command if small typo
- [ ] !github

## !yt

- [x] put youtube audio in a youtube audio folder
- [x] name audio file using video id
- [x] create and use a temp youtube folder
- [x] cache audio files and reuse them
  - How?
    - Name the audio files after the youtube video id
    - What about videos not from youtube?
    - Store a prefix for what site they're from?
  - What do we need to know to be able to check the cache
    - video id
- [x] todo have different sets of youtube dl args classes, one public, one internal
- [x] create script to upgrade youtube-dl binaries
- [x] log youtube-dl stdout
- [x] refactor youtube code
- [x] make it so bundtbot doesnt leave and rejoin channel in between clips
- [ ] !volume
- [ ] check for currently downlloading clips, aand dont start another download of the same thing
- [ ] cache search string to video ids
- [ ] make it so we can keep the same stream and stay speaking and just swap out audio streams with absolutely no interruption
- [ ] make the youtube classes their own library
- [ ] !ytrandom
- [ ] automatically duck audio when people talk
- [ ] Allow multiple url's in one command?
- [ ] Auto update youtube-dl.exe
  - When tho? On startup?
- [ ] store audio as opus instead of wav
- [ ] read opus files directly without having to compress them
- [ ] --postprocessor-args ARGS
  - maybe can nromalize audio
- [ ] stream live youtube
- [ ] maybe only allow a certain number of youtube downloads at once
- [ ] don't read the wav file so sloppily
- [ ] !upnext
- [ ] option to play for certain amount of time
  - --maxlength
  - --starttime
  - --endtime

## DJ

- [x] make it not leave channel in between clips
- [x] !fastforward
  - [ ] fix being able to stack !ff
- [x] !slomo
- [x] one DJ per server
- [ ] change the Playing Game field to song name
- [ ] Warp the filestream with a BufferedStream
  - How to tell if this is even necessary?
- [ ] looping

## Voice

## BundtBot Web API

- [x] change log level
- [ ] make logs accessable from webserver
  - like how many of each exception
  - when each exception was last thrown
- [ ] make guilds display on website again
- [ ] lock down website
- [ ] change dog level
- [ ] show uptime
- [ ] authentication
- [ ] send users a dm with a unique token for accessing bundtbot api
- [ ] let users sned commands by clicking buttons

## gulp

- [x] BUG: It gets stuck after deploy and you have to ctrl + c to exit
- [ ] instead of deleting previous version on test server, store it in a previous version folder so we can quickly rollback if need be
- [ ] get more logging from node do tar and grunt sftp:deploy
  - i want to see progress logs

## build system

- [ ] find out how to make a smaller build

## vscode

- [ ] make it so when i hit f5, if the build errors then it doesn't continue to run the previous successful build
- [ ] can't run task because it says a task is running, but i dont see one running

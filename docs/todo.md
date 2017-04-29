# BundtBot ToDo

[ ] create script to upgrade youtube-dl binaries
[ ] refactor youtube code
[ ] make a json file to configure what actions bundtbot does on startup
  for running tests and such
[ ] allow playing audio to two different servers at once
[ ] allow multiple different text responses to certain events
[ ] look into Cake build system
[ ] edit the same message continuously to do a ascii art animation

## TesterBot

[√] Make TesterBot
  For doing functional testing on BundtBot
  Will be a separate discord bot
[ ] add some asserts

## Gateway

[ ] Dispatch
  [ ] Ready
  [ ] Resumed
  [ ] Channel Create
  [ ] Channel Update
  [ ] Channel Delete
  [ ] Guild Create
  [ ] Guild Update
  [ ] Guild Delete
  [ ] Guild Ban Add
  [ ] Guild Ban Remove
  [ ] Guild Emojis Update
  [ ] Guild Integrations Update
  [ ] Guild Member Add
  [ ] Guild Member Remove
  [ ] Guild Member Update
  [ ] Guild Members Chunk
  [ ] Guild Role Create
  [ ] Guild Role Update
  [ ] Guild Role Delete
  [ ] Message Create
  [ ] Message Update
  [ ] Message Delete
  [ ] Message Delete Bulk
  [ ] Presence Update
  [ ] Typing Start
  [ ] User Update
  [ ] Voice State Update
  [ ] Voice Server Update
[ ] Heartbeat
[ ] Identify
[ ] StatusUpdate
[ ] VoiceStateUpdate
[ ] VoiceServerPing
[ ] Resume
[ ] Reconnect
[ ] RequestGuildMembers
[ ] InvalidSession
[ ] Hello
[ ] HeartbeatAck

## Bugs

[ ] not handling new users to the server correctly

## Optimizations

## !commands

[ ] !restart
  restart the bot
[ ] !dog
[ ] !uploaddog
[ ] !setloglevel

## !yt

[√] put youtube audio in a youtube audio folder
[√] name audio file using video id
[ ] create and use a temp youtube folder
[ ] cache audio files and reuse them
  How?
  Name the audio files after the youtube video id
  What about videos not from youtube?
  Store a prefix for what site they're from?
[ ] !volume
[ ] make the youtube classes their own library
[ ] !ytrandom
[ ] automatically duck audio when people talk
[ ] Allow multiple url's in one command?
[ ] Auto update youtube-dl.exe
  When tho? On startup?
[ ] log youtube-dl stdout
[ ] store audio as opus instead of wav
[ ] read opus files directly without having to compress them
[ ] --postprocessor-args ARGS
  maybe can nromalize audio
[ ] stream live youtube

## DJ

[ ] make it not leave channel in between clips
[√] !fastforward
  [ ] fix being able to stack !ff
[ ] !slomo
[ ] change the Playing Game filed to song name
[ ] Warp the filestream with a BufferedStream
  How to tell if this is even necessary?

## Voice

## BundtBot Web API

[√] change log level
[ ] make logs accessable from webserver
  like how many of each exception
  when each exception was last thrown
[ ] make guilds display on website again
[ ] lock down website
[ ] change dog level
[ ] show uptime
[ ] authentication

## gulp

[√] BUG: It gets stuck after deploy and you have to ctrl + c to exit
[ ] instead of deleting previous version on test server, store it in a previous version folder so we can quickly rollback if need be
[ ] get more logging from node do tar and grunt sftp:deploy
  i want to see progress logs

## build system

[ ] find out how to make a smaller build

## vscode

[ ] make it so when i hit f5, if the build errors then it doesn't continue to run the previous successful build
[ ] can't run task because it says a task is running, but i dont see one running
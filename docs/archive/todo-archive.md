# BundtBot ToDo

[√] Fix the gateway SendAsync from sending messages when internet is disconnected
  not sure if possible to fix because of nature of UDP
  fixed using retry logic
[√] handle opcode 9 invalid session
[√] make LogError and LogCritical use Console.Error.Write
[√] change trace json log to output raw json
[√] hello world voice
  [√] create handler for !hello in BundtBot
  [√] voice hello world
    bundtbot should join the voice channel of the requesting user and play a clip that says, "bundtbot said, Hello, world"
[√] move to csproj
[√] upgrade xunit
[√] refactor WebSocketClient
[√] Create a SoundManager for queueing audio
[√] New way to manage commands

## Optimizations

[√] uses too much memory on !yt
  Keeping an entire audio clip's pcm data takes up a lot of space
  400 MB for a 30 minute wav clip
  need to use streams instead
  only feed in a little bit at a time

## !commands

## !yt

[√] !yt

## DJ

[√] enqueue audio to play
[√] !pause
[√] !resume
[√] !stop
[√] !next

## Voice

[√] put the voice udp client on its own thread
[√] need to get rid of old voice client when leveing voice in server
  [√] need to disconnect from voice properly when leaving a voice channel
  [√] no exceptions should be thrown when disposing stuff
  Disposal looks better now

## BundtBot Web API

[√] show last logged exception in website

## gulp

## build system

## vscode
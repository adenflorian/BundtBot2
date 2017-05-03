# BundtBot ToDo

- [x] Fix the gateway SendAsync from sending messages when internet is disconnected
  - not sure if possible to fix because of nature of UDP
  - fixed using retry logic
- [x] handle opcode 9 invalid session
- [x] make LogError and LogCritical use Console.Error.Write
- [x] change trace json log to output raw json
- [x] hello world voice
  - [x] create handler for !hello in BundtBot
  - [x] voice hello world
    - bundtbot should join the voice channel of the requesting user and play a clip that says, "bundtbot said, Hello, world"
- [x] move to csproj
- [x] upgrade xunit
- [x] refactor WebSocketClient
- [x] Create a SoundManager for queueing audio
- [x] New way to manage commands

## Optimizations

- [x] uses too much memory on !yt
  - Keeping an entire audio clip's pcm data takes up a lot of space
  - 400 MB for a 30 minute wav clip
  - need to use streams instead
  - only feed in a little bit at a time

## !commands

## !yt

- [x] !yt

## DJ

- [x] enqueue audio to play
- [x] !pause
- [x] !resume
- [x] !stop
- [x] !next

## Voice

- [x] put the voice udp client on its own thread
- [x] need to get rid of old voice client when leveing voice in server
  - [x] need to disconnect from voice properly when leaving a voice channel
  - [x] no exceptions should be thrown when disposing stuff
  - Disposal looks better now

## BundtBot Web API

- [x] show last logged exception in website

## gulp

## build system

## vscode
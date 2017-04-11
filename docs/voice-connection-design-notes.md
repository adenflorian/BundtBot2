Voice Connection Design Notes
=============================

## TODO
[√] Maybe do one voice client per guild?
[√] voice ready handler
[√] voice op6 handler
[√] don't start heartbeat until after sending identify or summthn
[ ] IP Discovery

Possible Classes:
[ ] OpusStuff

## Class Dependency Tree
- DiscordVoiceClient
    - VoiceWebsocketClient
        - ClientWebsocketWrapper
    - VoiceUdpClient

## Connecting to Voice
### Retrieving Voice Server Information
[√] Send Voice State Update opcode to Gateway (join a voice channel)
    [√] Create SendVoiceStateUpdateAsync in DiscordGatewayClient
[√] Receive Voice State Update and VoiceServer Update events
### Establishing a Voice Websocket Connection
[ ] 
### Establishing a Voice UDP Connection
[ ] 

Models:
[ ] VoiceDataPacket
    - Encrypted voice data packet header
    - Encrypted Opus audio data
[x] VoiceState

Structs:
[ ] RtpHeader

## Opus
- 2 channels (stereo)
- 48Khz

## Voice Websocket Events
0	Identify            used to begin a voice websocket connection
1	Select Protocol     used to select the voice protocol
2	Ready               used to complete the websocket handshake
3	Heartbeat           used to keep the websocket connection alive
4	Session             Description	used to describe the session
5	Speaking            used to indicate which users are speaking

Encrypted Voice Packet

Required Libraries:
[√] Opus
[ ] libsodium

Things To Look Into:
[ ] UDP Hole Punching
[ ] RTP Header
    - RTP Data Transfer Protocol
    - Real-time Transport Protocol


## Questions
When do I connect to voice server?
    Whenever I join a voice channel?
    When I join a voice channel and I don't already have a connection?
What will the interface look like for sending audio data?
> DiscordClient.SendAudio?
Can I be connected to multiple voice servers at once?
Can I send audio data to multiple voice channels at once?
What exactly is the session id?
    Is it for my entire client's session?
    If I had multiple instances running with the same bot token would they be the same or different ids?

Looks like you have once voice server websocket connection per guild that you want to talk in
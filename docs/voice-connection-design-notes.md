Voice Connection Design Notes
=============================

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
[ ] Opus
[ ] libsodium

Things To Look Into:
[ ] UDP Hole Punching
[ ] RTP Header
    - RTP Data Transfer Protocol
    - Real-time Transport Protocol



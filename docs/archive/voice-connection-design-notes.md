Voice Connection Design Notes
=============================

https://discordapp.com/developers/docs/topics/voice-connections

## TODO
[√] Maybe do one voice client per guild?
[√] voice ready handler
[√] voice op6 handler
[√] don't start heartbeat until after sending identify or summthn
[√] IP Discovery
[√] Send Select Protocol to Voice Gateway
[√] Receive OP4 Session Description
[√] Start encrypting and sending voice data
    [√] libsodium
    [√] opus

## Class Dependency Tree
- DiscordVoiceClient
    - VoiceGatewayClient
        - WebSocketClient
    - VoiceUdpClient

## Opus
- 2 channels (stereo)
- 48Khz

Things To Look Into:
[√] RTP Header
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
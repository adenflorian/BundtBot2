# Streaming Audio To Multiple Servers Simultaneously

## Definitions

- Speaking
> Bot's icon is lit up in discord to show that it is speaking

## Issue

one bot
request song in one server
request song in another server
they are both playing simultaneously
do !stop in one server
the stopped one stops playing, stops speaking, and disconnects from voice
the other one stays in channel but stops speaking

### Possible Causes

- ~Speaking~
  - ~Sending speaking incorrectly~
  - ~Sending speaking causes bot to stop speaking on all servers~
- Leaving Voice Channel
  - Leaving channel incorrectly
  - Leaving channel causes bot on all servers to stop speaking

- How to rule each one above out?
  - send speaking true at startup and never send it again for that voice connection
  - should rule out anything to do with the speaking op code

- Not an issue involving the Speaking op code
  - I changed it to only send speaking true once received session op code, and never send speaking again for that connection, but issue still occurred

  - Has something to do with leaving a channel
  - Follow the $leave comand
    - What happens on a $leave command?
      - VoiceStateUpdate to leave channel
      - Dispose DiscordVoiceClient and set to null
        - Disposes websocket connection
    - No issue when not sending the voice state update

- [it's apparently a bug in discord](https://github.com/Rapptz/discord.py/issues/477)

### Possible Workarounds

- Never leave voice
- Send speaking is ture on all servers when bot leaves any voice channel
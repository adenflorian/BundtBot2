# Streaming Audio To Multiple Servers Simultaneously

## Issue

one bot
request song in one server
request song in another server
tey are both playing simultaneously
do !stop in one server
the stopped one stops playing, stops speaking, and disconnects from voice
the other on stays in channel but stops speaking

### Possible Causes

- Sending speaking incorrectly
- Sending speaking causes bot to stop speaking on all servers
- Leaving channel incorrectly
- LEaving channel causes bot on all servers to stop speaking
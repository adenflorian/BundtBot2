# ClientWebSocketWrapper
- Instead of having it invoke an event when it receives a message, it could just store messages it receives in it's own buffer and let people retrieve the next message in it's buffer
- This will make implementing the procedural connecting code simpler

The process for connecting to the gateway is strictly procedural, so the code should be too
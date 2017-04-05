# ClientWebSocketWrapper
- Instead of having it invoke an event when it receives a message, it could just store messages it receives in it's own buffer and let people retrieve the next message in it's buffer
- This will make implementing the procedural connecting code simpler

The process for connecting to the gateway is strictly procedural, so the code should be too

## Current Connect Code Design
ConnectAsync()
awaits _clientWebSocketWrapper.ConnectAsync();
ConnectAsync()
awaits _clientWebSocket.ConnectAsync(_serverUri, CancellationToken.None);
StartReceiveLoop();
StartSendLoop();


## Notes
I think part of the issue is that I want to track whether I connected, disconnected or reconnecting

Why?

To implement better reconnecting code

Connect
Connected
disconnected
cleanup connection
connect
...

but I have to do something special for a reconnect (Resume opcode)

so i need to track whether ive ever had a successful connection

> when can i use Resume instead of Identify?
> when I've finished processing the Ready event?
*Yes!*
> how do i currently know when i've finished processing the ready event?
*bool _hasReadyEventBeenInvoked = false;*
> where do i use that?
*OnHelloReceived*

can/should the code for sending messages care about the connection state?

maybe make a queue for inprogress outgoing messages

or make the send massage code be very robust

[√] log sequence number

which opcodes have sequence numbers?

CloseStatus
CloseStatusDescription
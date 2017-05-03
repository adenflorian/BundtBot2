- [x] The client should now begin sending OP 1 Heartbeat payloads every heartbeat_interval milliseconds, until the connection is eventually closed or terminated.
- [ ] Clients can detect zombied or failed connections by listening for OP 11 Heartbeat ACK.
- [ ] If a client does not receive a heartbeat ack between its attempts at sending heartbeats, it should immediately terminate the connection with a non 1000 close code, reconnect, and attempt to resume.

only start hearbeat in response to heartbeat ack?

start a zombie timer on every heartbeat ACK where if it hits 0 we close connection rand reconnect?
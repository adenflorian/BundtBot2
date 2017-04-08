[√] Fix the gateway SendAsync from sending messages when internet is disconnected
  - not sure if possible to fix because of nature of UDP
  - fixed using retry logic

Why is "System.Net.Http.WinHttpException: The operation timed out" thrown from "System.Net.WebSockets.WinHttpWebSocket.<ReceiveAsync>" after unplugging the ethernet?

[√] handle opcode 9 invalid session

[√] make LogError and LogCritical use Console.Error.Write

[√] show last logged exception in website

[ ] make logs accessable from webserver
  - like how many of each exception
  - when each exception was last thrown

[ ] make guilds display on website again

[ ] lock down website

[ ] !hello
  - voice hello world
  - bundtbot should join the voice channel of the requesting user and play a clip that says, "Hello, world! This is bundtbot. No relation to the cake"

[ ] !yt
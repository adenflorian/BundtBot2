[√] Fix the gateway SendAsync from sending messages when internet is disconnected
  - not sure if possible to fix because of nature of UDP
  - fixed using retry logic

Why is "System.Net.Http.WinHttpException: The operation timed out" thrown from "System.Net.WebSockets.WinHttpWebSocket.<ReceiveAsync>" after unplugging the ethernet?

[√] handle opcode 9 invalid session

[ ] make LogError use Console.WriteError

[ ] make logs accessable from webserver
  - like how many of each exception
  - when each exception was last thronw
|   | Project |
|---|----------|
| - | BundtBot |
| * | BundtCord |
| + | DiscordApiWrapper |

- Program
  - WebServer
  - BundtBot
    * DiscordClient
      + DiscordGatewayClient
        + ClientWebSocketWrapper
      + DiscordRestClientProxy
        + RateLimitedClient
          + DiscordRestClient
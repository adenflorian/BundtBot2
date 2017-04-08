|   | Project |
|---|----------|
| - | BundtBot |
| * | BundtCord |
| + | DiscordApiWrapper |
|   | *Proposed* |

- Program
    - WebServer
    - BundtBot
        * DiscordClient
            + DiscordGatewayClient
                + ClientWebSocketWrapper
            + DiscordRestClientProxy
                + RateLimitedClient
                    + DiscordRestClient
            + *DiscordVoiceWebSocketClient*
                + *ClientWebSocketWrapper*

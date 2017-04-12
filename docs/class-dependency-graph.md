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
                + WebSocketClient
            + DiscordRestClientProxy
                + RateLimitedClient
                    + DiscordRestClient
            + DiscordVoiceClient
                + DiscordVoiceGatewayClient
                    + WebSocketClient
                + VoiceUdpClient

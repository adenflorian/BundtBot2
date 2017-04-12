using System.Collections.Generic;

namespace DiscordApiWrapper.Gateway
{
    static class CloseCodes
    {
        public static Dictionary<string, string> Codes = new Dictionary<string, string>
        {
            {"4000", "unknown error - We're not sure what went wrong. Try reconnecting?"},
            {"4001", "unknown opcode - You sent an invalid Gateway OP Code. Don't do that!"},
            {"4002", "decode error - You sent an invalid payload to us. Don't do that!"},
            {"4003", "not authenticated - You sent us a payload prior to identifying."},
            {"4004", "authentication failed - The account token sent with your identify payload is incorrect."},
            {"4005", "already authenticated - You sent more than one identify payload. Don't do that!"},
            {"4007", "invalid - The sequence sent when resuming the session was invalid. Reconnect and start a new session."},
            {"4008", "rate limited - Woah nelly! You're sending payloads to us too quickly. Slow it down!"},
            {"4009", "session - Your session timed out. Reconnect and start a new one."},
            {"4010", "invalid shard - You sent us an invalid shard when identifying."},
            {"4011", "sharding required - The session would have handled too many guilds - you are required to shard your connection in order to connect."}
        };
    }
}
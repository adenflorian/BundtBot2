using System.Collections.Generic;
using DiscordApiWrapper.Voice;

namespace BundtCord.Discord
{
    public interface IServer
    {
        ulong Id { get; }
        IEnumerable<ITextChannel> TextChannels { get; }
        IEnumerable<IVoiceChannel> VoiceChannels { get; }
        IEnumerable<IServerMember> Members { get; }
        DiscordVoiceClient VoiceClient { get; }
    }
}
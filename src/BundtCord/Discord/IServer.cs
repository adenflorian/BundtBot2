using System.Collections.Generic;

namespace BundtCord.Discord
{
    public interface IServer
    {
        ulong Id { get; }
        IEnumerable<ITextChannel> TextChannels { get; }
        IEnumerable<VoiceChannel> VoiceChannels { get; }
    }
}
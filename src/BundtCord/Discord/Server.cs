using System.Collections.Generic;

namespace BundtCord.Discord
{
    public class Server : IServer
    {
        public IList<ITextChannel> TextChannels { get; } = new List<ITextChannel>();
    }
}
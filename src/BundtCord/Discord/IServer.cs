using System.Collections.Generic;

namespace BundtCord.Discord
{
    public interface IServer
    {
         IList<ITextChannel> TextChannels { get; }
    }
}
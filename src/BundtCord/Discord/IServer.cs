using System.Collections.Generic;
using BundtBot.Discord;

namespace BundtCord.Discord
{
    public interface IServer
    {
         IList<ITextChannel> TextChannels { get; }
    }
}
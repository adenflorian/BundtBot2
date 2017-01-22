

namespace BundtBot.Discord
{
    public interface IMessage
	{
        IUser Author { get; }
        ITextChannel TextChannel {get;}
    }
}

using System.Threading.Tasks;

namespace BundtCord.Discord
{
    public interface ITextChannelMessage
	{
        IServerMember Author { get; }
        string Content { get; }
        ITextChannel TextChannel {get;}
        
        Task ReplyAsync(string messageContent);
    }
}

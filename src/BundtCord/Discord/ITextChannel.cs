using System.Threading.Tasks;

namespace BundtCord.Discord
{
    public interface ITextChannel
	{
        Task<IMessage> SendMessageAsync(string content);
    }
}

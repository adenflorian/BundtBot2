using System.Threading.Tasks;

namespace BundtBot.Discord
{
    public interface ITextChannel
	{
        Task<IMessage> SendMessageAsync(string content);
    }
}

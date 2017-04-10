using System.Threading.Tasks;

namespace BundtCord.Discord
{
    public interface IVoiceChannel
    {
        ulong Id { get; }
        string Name { get; }
        ulong ServerId { get; }

        Task JoinAsync();
        Task LeaveAsync();
    }
}
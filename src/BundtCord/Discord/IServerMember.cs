namespace BundtCord.Discord
{
    public interface IServerMember
    {
        IServer Server { get; }
        IUser User { get; }
        IVoiceChannel VoiceChannel { get; }
    }
}
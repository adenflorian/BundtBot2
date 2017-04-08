namespace BundtCord.Discord
{
    public interface IUser
	{
        ulong Id { get; }
        VoiceChannel VoiceChannel { get; }
    }
}

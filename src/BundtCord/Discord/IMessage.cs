namespace BundtCord.Discord
{
    public interface IMessage
	{
        IUser Author { get; }
        string Content { get; }
        ITextChannel TextChannel {get;}
    }
}

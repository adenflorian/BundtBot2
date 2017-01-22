using BundtBot.Discord.Models;

namespace BundtBot.Discord
{
    public class User : IUser
    {
        public ulong Id { get; }

        public User(DiscordUser discordUser)
        {
            Id = discordUser.Id;
        }
    }
}

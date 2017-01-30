namespace DiscordApiWrapper.RestApi
{
    public class RateLimit
    {
        public int Limit { get; }
        public int Remaining;
        public int Reset { get; }

        public RateLimit(int limit, int remaining, int reset)
        {
            Limit = limit;
            Remaining = remaining;
            Reset = reset;
        }
    }
}
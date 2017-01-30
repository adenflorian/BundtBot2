namespace BundtBot.Discord.Models
{
    public interface IRestApiRequest
    {
        /// <summary>
        /// Example: channels/123/messages
        /// </summary>
        string requestUri { get; }
        RestRequestType requestType { get; }
    }

    public enum RestRequestType
    {
        Get,
        Post
    }
}
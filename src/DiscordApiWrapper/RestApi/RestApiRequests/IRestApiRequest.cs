namespace DiscordApiWrapper.RestApi.RestApiRequests
{
    public interface IRestApiRequest
    {
        /// <summary>
        /// Example: channels/123/messages
        /// </summary>
        string RequestUri { get; }
        RestRequestType RequestType { get; }
    }

    public enum RestRequestType
    {
        Get,
        Post
    }
}
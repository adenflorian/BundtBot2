namespace DiscordApiWrapper.RestApi.RestApiRequests
{
    public class GetRequest : IRestApiRequest
    {
        public RestRequestType RequestType => RestRequestType.Get;

        public string RequestUri { get; }

        public GetRequest(string requestUri)
        {
            RequestUri = requestUri;
        }
    }
}
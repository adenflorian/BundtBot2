namespace DiscordApiWrapper.RestApi.RestApiRequests
{
    class GetRequest : RestApiRequest
    {
        internal override RestRequestType RequestType => RestRequestType.Get;

        internal override string RequestUri { get; }

        internal GetRequest(string requestUri)
        {
            RequestUri = requestUri;
        }
    }
}
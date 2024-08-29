using Elastic.Transport;

namespace BAL.ElasticSearch.Helper
{
    public class ApiKeyAuthenticationHeader : AuthorizationHeader
    {
        private readonly string _apiKey;

        public ApiKeyAuthenticationHeader(string apiKey)
        {
            _apiKey = apiKey;
        }

        public override string AuthScheme => "ApiKey";

        public override bool TryGetAuthorizationParameters(out string value)
        {
            value = _apiKey;
            return true;
        }
    }
}

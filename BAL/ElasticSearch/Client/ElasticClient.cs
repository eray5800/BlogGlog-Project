using BAL.ElasticSearch.Helper;
using Elastic.Clients.Elasticsearch;
using Microsoft.Extensions.Configuration;
using System;

namespace BAL.ElasticSearch.Client
{
    public class ElasticClient
    {
        private readonly ElasticsearchClient _elasticClient;

        public ElasticClient(IConfiguration configuration)
        {
            var settings = CreateSettings(configuration);
            _elasticClient = new ElasticsearchClient(settings);
        }

        private static ElasticsearchClientSettings CreateSettings(IConfiguration configuration)
        {
            var elasticSettings = configuration.GetSection("ElasticSettings");

            var apiKey = elasticSettings["API_KEY"];
            var elasticUri = elasticSettings["Uri"];

            var settings = new ElasticsearchClientSettings(new Uri(elasticUri))
                .DefaultIndex("blogs")
                .Authentication(new ApiKeyAuthenticationHeader(apiKey))
                .ServerCertificateValidationCallback((sender, certificate, chain, sslPolicyErrors) => true);

            return settings;
        }

        public ElasticsearchClient GetClient()
        {
            return _elasticClient;
        }
    }
}

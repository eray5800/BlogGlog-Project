using BAL.ElasticSearch;
using BAL.ElasticSearch.Client;
using DAL.Models;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Microsoft.Extensions.Logging;

public class ElasticSearchService : IElasticSearchService
{
    private readonly ElasticsearchClient _elasticClient;
    private readonly ILogger<ElasticSearchService> _logger;

    public ElasticSearchService(ElasticClient elasticClient, ILogger<ElasticSearchService> logger)
    {
        _elasticClient = elasticClient.GetClient();
        _logger = logger;
    }

    public async Task<bool> CreateBlogAsync(Blog blog)
    {
        try
        {
            var createRequest = new IndexRequest<Blog>("blogs", blog.BlogId.ToString())
            {
                Document = blog
            };
            var createResponse = await _elasticClient.IndexAsync(createRequest);
            return createResponse.IsValidResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating blog");
            return false;
        }
    }

    public async Task<bool> UpdateBlogAsync(Blog requestBlog)
    {
        try
        {
            var updateRequest = new UpdateRequest<Blog, Blog>("blogs", requestBlog.BlogId)
            {
                Doc = requestBlog
            };
            var updateResponse = await _elasticClient.UpdateAsync(updateRequest);
            return updateResponse.IsValidResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating blog");
            return false;
        }
    }

    public async Task<bool> DeleteBlogAsync(Guid blogId)
    {
        try
        {
            var deleteResponse = await _elasticClient.DeleteAsync<Blog>("blogs", blogId.ToString());
            return deleteResponse.IsValidResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting blog");
            return false;
        }
    }

    public async Task<IEnumerable<Blog>> SearchBlogsAsync(string query)
    {
        try
        {
            var searchRequest = new SearchRequest<Blog>("blogs")
            {
                Size = 10,
                Query = new BoolQuery
                {
                    Must = new Query[]{
                       new BoolQuery{
                            Should = new Query[]{
                                new WildcardQuery(new Field("blogTitle")) { Value = $"*{query}*" },
                                new WildcardQuery(new Field("content")) { Value = $"*{query}*" }
                            }
                       }
                    },
                    Filter = new Query[]
                    {
                        new TermQuery(new Field("isActive")) { Value = true } // Skorlama yapılmaz, filtre uygulanır
                    }
                }

            };
            var response = await _elasticClient.SearchAsync<Blog>(searchRequest);
            return response.Documents.Count != 0 ? response.Documents : Enumerable.Empty<Blog>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching blogs");
            return Enumerable.Empty<Blog>();
        }
    }

    public async Task<IEnumerable<Blog>> SearchBlogCategoryAsync(string category)
    {
        if (string.IsNullOrWhiteSpace(category))
        {
            return Enumerable.Empty<Blog>();
        }

        try
        {
            var searchRequest = new SearchRequest<Blog>("blogs")
            {
                Size = 10,
                Query = new BoolQuery
                {
                    Must = new Query[]
                    {
                    new MatchQuery(new Field("category.categoryName"))
                    {
                        Query = category
                    }
                    },
                    Filter = new Query[]
                    {
                    new TermQuery(new Field("isActive")) { Value = true }
                    }
                }
            };
            var response = await _elasticClient.SearchAsync<Blog>(searchRequest);
            return response.Documents.Count != 0 ? response.Documents : Enumerable.Empty<Blog>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching blogs by category");
            return Enumerable.Empty<Blog>();
        }
    }
}

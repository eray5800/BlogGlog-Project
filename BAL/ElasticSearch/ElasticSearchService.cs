using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DAL.Models;
using DAL.Models.DTO.Blog;
using BAL.ElasticSearch.Client;
using BAL.ElasticSearch;

public class ElasticSearchService
{
    private readonly ElasticsearchClient _elasticClient;

    public ElasticSearchService(ElasticClient elasticClient)
    {
        _elasticClient = elasticClient.GetClient();
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
            // Use a logging framework instead of Console.WriteLine
            Console.WriteLine($"Error creating blog: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> UpdateBlogAsync(Blog requestBlog)
    {
        try
        {

            // Ensure BlogDTO is mapped correctly to Blog if needed
            var updateRequest = new UpdateRequest<Blog,Blog>("blogs", requestBlog.BlogId)
            {
                Doc = requestBlog
            };
            var updateResponse = await _elasticClient.UpdateAsync(updateRequest);
            return updateResponse.IsValidResponse;
        }
        catch (Exception ex)
        {
            // Use a logging framework instead of Console.WriteLine
            Console.WriteLine($"Error updating blog: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> DeleteBlogAsync(Guid blogId)
    {
        try
        {
            var deleteResponse = await _elasticClient.DeleteAsync<Blog>("blogs",blogId.ToString());
            return deleteResponse.IsValidResponse;
        }
        catch (Exception ex)
        {
            // Use a logging framework instead of Console.WriteLine
            Console.WriteLine($"Error deleting blog: {ex.Message}");
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
                    Should = new Query[]
                    {
                        new WildcardQuery(new Field("blogTitle")) { Value = $"*{query}*" },
                        new WildcardQuery(new Field("content")) { Value  = $"*{query}*" }
                    }
                }
            };
            var response = await _elasticClient.SearchAsync<Blog>(searchRequest);
            return response.Documents;
        }
        catch (Exception ex)
        {
            // Use a logging framework instead of Console.WriteLine
            Console.WriteLine($"Error searching blogs: {ex.Message}");
            return new List<Blog>();
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
                Query = new MatchQuery(new Field("category.categoryName"))
                {
                    Query = category
                }
            };

            var response = await _elasticClient.SearchAsync<Blog>(searchRequest);
            return response.Documents;
        }
        catch (Exception ex)
        {
            // Use a logging framework instead of Console.WriteLine
            Console.WriteLine($"Error searching blogs by category: {ex.Message}");
            return new List<Blog>();
        }
    }

}

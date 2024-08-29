using DAL.Models;
using DAL.Models.DTO.Blog;
using GenericRepoAndUnitOfWork.Core.IConfiguration;

public class BlogService
{
    private IUnitOfWork unitOfWork { get; set; }
    private readonly ElasticSearchService _elasticSearchService;

    public BlogService(IUnitOfWork unitOfWork, ElasticSearchService elasticSearchService)
    {
        this.unitOfWork = unitOfWork;
        _elasticSearchService = elasticSearchService;
    }

    public async Task<bool> CreateBlogAsync(BlogDTO blogDto, AppUser user)
    {
        Category category = await unitOfWork.Categories.GetByIDAsync(blogDto.SelectedCategoryId);

        if (category == null || !category.IsActive)
        {
            return false;
        }

        var blogTags = blogDto.BlogTags.Contains(',')
            ? blogDto.BlogTags.Split(',').Select(tag => tag.Trim()).ToList()
            : new List<string> { blogDto.BlogTags.Trim() };

        var result = await unitOfWork.Blogs.AddAsync(blogDto, category, user);
        if (result != null)
        {
            await unitOfWork.CompleteAsync();

            
            var elasticResult = await _elasticSearchService.CreateBlogAsync(result);
            if (!elasticResult)
            {
                return false;
            }
        }
        return true;
    }

    public async Task<bool> UpdateBlogAsync(Guid blogID, BlogDTO blogDto)
    {
        Category category = await unitOfWork.Categories.GetByIDAsync(blogDto.SelectedCategoryId);
        if (category == null || !category.IsActive)
        {
            return false;
        }

        bool result = await unitOfWork.Blogs.UpdateAsync(blogID, blogDto, category);

        if (result)
        {
            Blog BlogTagResult = await UpdateBlogTagsAsync(blogID, blogDto.BlogTags);

            if (result != null)
            {
                await unitOfWork.CompleteAsync();

                var elasticResult = await _elasticSearchService.UpdateBlogAsync(BlogTagResult);
                if (!elasticResult)
                {
                    return false;
                }
            }
        }
        return result;
    }

    public async Task<bool> DeleteBlogAsync(Guid blogId)
    {
        bool result = await unitOfWork.Blogs.DeleteAsync(blogId);
        if (result)
        {
            await unitOfWork.CompleteAsync();

            var elasticResult = await _elasticSearchService.DeleteBlogAsync(blogId);
            if (!elasticResult)
            {
                
                return false;
            }
        }
        return result;
    }

    public async Task<IEnumerable<Blog>> GetAllUserBlogsAsync(string userID)
    {
        var result = await unitOfWork.Blogs.GetAllUserBlogsByIDAsync(userID);
        if (result == null)
        {
            return Enumerable.Empty<Blog>();
        }
        return result;
    }

    public async Task<IEnumerable<Blog>> GetAllBlogsAsync()
    {
        return await unitOfWork.Blogs.GetAllAsync();
    }

    public async Task<Blog> GetBlogByIDAsync(Guid blogID)
    {
        Blog blog = await unitOfWork.Blogs.GetByIDAsync(blogID);
        if (blog == null)
        {
            return new Blog();
        }
        return blog;
    }

    public async Task<IEnumerable<Blog>> Search(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return Enumerable.Empty<Blog>();
        }

        return await _elasticSearchService.SearchBlogsAsync(text);
    }

    public async Task<IEnumerable<Blog>> SearchBlogCategoryAsync(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return Enumerable.Empty<Blog>();
        }

        return await _elasticSearchService.SearchBlogCategoryAsync(text);
    }

    private async Task<Blog> UpdateBlogTagsAsync(Guid blogID, string blogTags)
    {
        var existingBlog = await unitOfWork.Blogs.GetByIDAsync(blogID);

        if (existingBlog == null)
        {
            return new Blog() ;
        }

        var newTagNames = blogTags.Contains(',')
            ? blogTags.Split(',').Select(tag => tag.Trim()).ToList()
            : new List<string> { blogTags.Trim() };

        await unitOfWork.BlogTags.RemoveAllTagsByBlogIDAsync(blogID);

        var newTags = newTagNames.Select(tag => new BlogTagDTO
        {
            BlogTagID = Guid.NewGuid(),
            TagName = tag,
            Blog = existingBlog
        }).ToList();

        foreach (var tag in newTags)
        {
            await unitOfWork.BlogTags.AddAsync(tag);
        }

        return existingBlog;
    }
}

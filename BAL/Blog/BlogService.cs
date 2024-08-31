using DAL.Models;
using DAL.Models.DTO.BlogDTO;
using GenericRepoAndUnitOfWork.Core.IConfiguration;

public class BlogService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ElasticSearchService _elasticSearchService;

    public BlogService(IUnitOfWork unitOfWork, ElasticSearchService elasticSearchService)
    {
        _unitOfWork = unitOfWork;
        _elasticSearchService = elasticSearchService;
    }

    public async Task<bool> CreateBlogAsync(BlogDTO blogDto, AppUser user)
    {
        try
        {
            var category = await _unitOfWork.Categories.GetByIDAsync(blogDto.SelectedCategoryId);
            if (category == null || !category.IsActive)
            {
                return false;
            }

            var result = await _unitOfWork.Blogs.AddAsync(blogDto, category, user);
            if (result != null)
            {
                await _unitOfWork.CompleteAsync();

                var elasticResult = await _elasticSearchService.CreateBlogAsync(result);
                if (!elasticResult)
                {
                    return false;
                }
            }
            return true;
        }
        catch (Exception ex)
        {
            // Log the exception
            Console.WriteLine(ex.Message);
            throw;
        }
    }

    public async Task<bool> UpdateBlogAsync(Guid blogID, BlogDTO blogDto)
    {
        var category = await _unitOfWork.Categories.GetByIDAsync(blogDto.SelectedCategoryId);
        if (category == null || !category.IsActive)
        {
            return false;
        }

        var result = await _unitOfWork.Blogs.UpdateAsync(blogID, blogDto, category);

        if (result != null)
        {
            // Handle blog tags
            var blogTagResult = await UpdateBlogTagsAsync(blogID, blogDto.BlogTags);

            // Handle blog images
            var blogImageResult = await UpdateBlogImagesAsync(blogID, blogDto.BlogImages);

            if (blogTagResult != null && blogImageResult != null)
            {
                await _unitOfWork.CompleteAsync();

                var elasticResult = await _elasticSearchService.UpdateBlogAsync(blogTagResult);
                if (!elasticResult)
                {
                    return false;
                }
                return true;
            }
        }
        return false;
    }



    public async Task<bool> DeleteBlogAsync(Guid blogId)
    {
        var result = await _unitOfWork.Blogs.DeleteAsync(blogId);
        if (result)
        {
            await _unitOfWork.BlogImages.DeleteAllAsync(img => img.Blog.BlogId == blogId);
            await _unitOfWork.CompleteAsync();

            var elasticResult = await _elasticSearchService.DeleteBlogAsync(blogId);
            if (!elasticResult)
            {
                return false;
            }
        }
        return result;
    }

    private async Task<Blog> UpdateBlogTagsAsync(Guid blogID, string blogTags)
    {
        var existingBlog = await _unitOfWork.Blogs.GetByIDAsync(blogID);

        if (existingBlog == null)
        {
            return null;
        }

        var newTagNames = blogTags.Contains(',')
            ? blogTags.Split(',').Select(tag => tag.Trim()).ToList()
            : new List<string> { blogTags.Trim() };

        await _unitOfWork.BlogTags.DeleteAllAsync(tag => tag.Blog.BlogId == blogID);

        var newTags = newTagNames.Select(tag => new BlogTag
        {
            BlogTagID = Guid.NewGuid(),
            TagName = tag,
            Blog = existingBlog
        }).ToList();

        foreach (var tag in newTags)
        {
            await _unitOfWork.BlogTags.AddAsync(tag);
        }

        return existingBlog;
    }

    private async Task<Blog> UpdateBlogImagesAsync(Guid blogID, List<BlogImageDTO> blogImageDtos)
    {
        // Retrieve the existing blog
        var existingBlog = await _unitOfWork.Blogs.GetByIDAsync(blogID);

        if (existingBlog == null)
        {
            return null;
        }

        // Delete existing images associated with the blog
        await _unitOfWork.BlogImages.DeleteAllAsync(img => img.Blog.BlogId == blogID);

        // Map BlogImageDTOs to BlogImage entities
        var newImages = blogImageDtos.Select(dto => new BlogImage
        {
            BlogImageID = Guid.NewGuid(),
            BlogImageName = dto.BlogImageName, // Assuming BlogImageDTO has BlogImageName
            Blog = existingBlog
        }).ToList();

        // Add new images to the repository
        foreach (var image in newImages)
        {
            await _unitOfWork.BlogImages.AddAsync(image);
        }

        return existingBlog;
    }


    public async Task<IEnumerable<Blog>> GetAllUserBlogsAsync(string userID)
    {
        var result = await _unitOfWork.Blogs.GetAllUserBlogsByIDAsync(userID);
        return result ?? Enumerable.Empty<Blog>();
    }

    public async Task<IEnumerable<Blog>> GetAllBlogsAsync()
    {
        return await _unitOfWork.Blogs.GetAllAsync();
    }

    public async Task<Blog> GetBlogByIDAsync(Guid blogID)
    {
        var blog = await _unitOfWork.Blogs.GetByIDAsync(blogID);
        return blog ?? new Blog();
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
}

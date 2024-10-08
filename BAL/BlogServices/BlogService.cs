﻿using BAL.ElasticSearch;
using DAL.Models;
using DAL.Models.DTO.BlogDTO;
using GenericRepoAndUnitOfWork.Core.IConfiguration;

namespace BAL.BlogServices
{
    public class BlogService : IBlogService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IElasticSearchService _elasticSearchService;

        public BlogService(IUnitOfWork unitOfWork, IElasticSearchService elasticSearchService)
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
                var blog = new Blog
                {
                    BlogId = Guid.NewGuid(),
                    User = user,
                    BlogTitle = blogDto.BlogTitle,
                    BlogTags = blogDto.BlogTags.Contains(',')
                       ? blogDto.BlogTags.Split(',')
                           .Select(tag => new BlogTag
                           {
                               BlogTagID = Guid.NewGuid(),
                               TagName = tag.Trim()
                           })
                           .ToList()
                       : new List<BlogTag>
                       {
                    new BlogTag
                    {
                        BlogTagID = Guid.NewGuid(),
                        TagName = blogDto.BlogTags.Trim()
                    }
                       },
                    Content = blogDto.Content,
                    Category = category,
                    Created_At = DateTime.Now,
                    Updated_At = DateTime.Now,
                    IsActive = blogDto.IsActive,
                    BlogImages = blogDto.BlogImages?.Select(imageDto => new BlogImage
                    {
                        BlogImageID = Guid.NewGuid(),
                        BlogImageName = imageDto.BlogImageName
                    }).ToList() ?? new List<BlogImage>()
                };
                var result = await _unitOfWork.Blogs.AddAsync(blog, category, user);
                if (result != null)
                {
                    await _unitOfWork.CompleteAsync();

                    var elasticResult = await _elasticSearchService.CreateBlogAsync(blog);
                    if (!elasticResult)
                    {
                        return false;
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
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
            var blog = await _unitOfWork.Blogs.GetByIDAsync(blogID);
            if(blog == null || blog.BlogId == Guid.Empty)
            {
                return false;
            }
            var result = await _unitOfWork.Blogs.UpdateAsync(blog, blogDto, category);

            if (result != null)
            {
                var blogTagResult = await UpdateBlogTagsAsync(blog, blogDto.BlogTags);

                var blogImageResult = await UpdateBlogImagesAsync(blog, blogDto.BlogImages);

                if (blogTagResult != null && blogImageResult != null)
                {
                    await _unitOfWork.CompleteAsync();

                    var elasticResult = await _elasticSearchService.UpdateBlogAsync(blog);
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
                await _unitOfWork.BlogTags.DeleteAllAsync(tag => tag.Blog.BlogId == blogId);
                await _unitOfWork.BlogLikes.DeleteAllAsync(like => like.Blog.BlogId == blogId);
                await _unitOfWork.CompleteAsync();

                var elasticResult = await _elasticSearchService.DeleteBlogAsync(blogId);
                if (!elasticResult)
                {
                    return false;
                }
            }
            return result;
        }

        private async Task<Blog> UpdateBlogTagsAsync(Blog existingBlog, string blogTags)
        {

            var newTagNames = blogTags.Contains(',')
                ? blogTags.Split(',').Select(tag => tag.Trim()).ToList()
                : new List<string> { blogTags.Trim() };

            await _unitOfWork.BlogTags.DeleteAllAsync(tag => tag.Blog.BlogId == existingBlog.BlogId);

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

        private async Task<Blog> UpdateBlogImagesAsync(Blog existingBlog, List<BlogImageDTO> blogImageDtos)
        {
            await _unitOfWork.BlogImages.DeleteAllAsync(img => img.Blog.BlogId == existingBlog.BlogId);

            var newImages = blogImageDtos.Select(dto => new BlogImage
            {
                BlogImageID = Guid.NewGuid(),
                BlogImageName = dto.BlogImageName,
                Blog = existingBlog
            }).ToList();

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
        public async Task<IEnumerable<Blog>> GetAllActiveBlogsAsync()
        {
            return await _unitOfWork.Blogs.GetAllActiveAsync();
        }

        public async Task<Blog> GetBlogByIDAsync(Guid blogID)
        {
            var blog = await _unitOfWork.Blogs.GetByIDAsync(blogID);
            return blog;
        }

        public async Task<Blog> GetActiveBlogByIDAsync(Guid blogID)
        {
            var blog = await _unitOfWork.Blogs.GetByIDActiveAsync(blogID);
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
            var result = await _elasticSearchService.SearchBlogCategoryAsync(text);
            return await _elasticSearchService.SearchBlogCategoryAsync(text);
        }
    }
}

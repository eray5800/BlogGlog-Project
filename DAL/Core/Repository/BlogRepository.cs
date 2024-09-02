using DAL.Context;
using DAL.Core.IRepository;
using DAL.Models;
using DAL.Models.DTO.BlogDTO;
using GenericRepoAndUnitOfWork.Core.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DAL.Core.Repository
{
    public class BlogRepository : GenericRepository<Blog, BlogDTO>, IBlogRepository
    {
        public BlogRepository(AppIdentityDBContext context, ILogger logger) : base(context, logger)
        {
        }

        // CREATE
        public async Task<Blog> AddAsync(BlogDTO blogDto, Category category, AppUser user)
        {
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

            await dbSet.AddAsync(blog);
            return blog;
        }


        public override async Task<Blog> GetByIDAsync(Guid blogID)
        {
            var blog = await dbSet
                .Include(b => b.BlogTags)
                .Include(b => b.Category)
                .Include(b => b.User)
                .Include(b => b.BlogImages) // Include images
                .FirstOrDefaultAsync(x => x.BlogId == blogID);

            return blog;
        }

        // READ - Get All
        public override async Task<IEnumerable<Blog>> GetAllAsync()
        {
            try
            {
                return await dbSet
                    .Include(b => b.Category)
                    .Include(b => b.BlogTags)
                    .Include(b => b.User)
                    .Include(b => b.BlogImages) // Include images
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Blog Repository GetAllAsync Method Error");
                return new List<Blog>();
            }
        }

        public async Task<IEnumerable<Blog>> GetAllUserBlogsByIDAsync(string userID)
        {
            return await dbSet
                .Include(b => b.BlogTags)
                .Include(b => b.Category)
                .Include(b => b.User)
                .Include(b => b.BlogImages) // Include images
                .Where(x => x.User.Id == userID)
                .ToListAsync();
        }

        public async Task<Blog> UpdateAsync(Guid blogID, BlogDTO blogDto, Category category)
        {
            var existingBlog = await dbSet
                .Include(b => b.Category)
                .Include(b => b.User)
                .Include(b => b.BlogImages) // Include images
                .FirstOrDefaultAsync(x => x.BlogId == blogID);

            if (existingBlog == null) return null;

            try
            {
                // Update properties
                existingBlog.BlogTitle = blogDto.BlogTitle;
                existingBlog.Content = blogDto.Content;
                existingBlog.IsActive = blogDto.IsActive;

                // Update Category
                if (category != null)
                {
                    existingBlog.Category = category;
                }


                existingBlog.Updated_At = DateTime.Now;

                return existingBlog;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                logger.LogError(ex, "Concurrency error occurred while updating blog.");
                return null;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while updating the blog.");
                return null;
            }
        }

        // DELETE
        public async Task<bool> DeleteAsync(Guid blogId)
        {
            var blog = await dbSet.FindAsync(blogId);
            if (blog == null) return false;

            dbSet.Remove(blog);
            return true;
        }

        public async Task<IEnumerable<Blog>> SearchAsync(string text)
        {
            try
            {
                return await dbSet
                    .Include(b => b.Category)
                    .Include(b => b.BlogTags)
                    .Include(b => b.User)
                    .Include(b => b.BlogImages) // Include images
                    .Where(x => x.Content.Contains(text) || x.BlogTitle.Contains(text))
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Blog Repository SearchAsync Method Error");
                return new List<Blog>();
            }
        }

        public async Task<IEnumerable<Blog>> SearchBlogCategoryAsync(string text)
        {
            try
            {
                return await dbSet
                    .Include(b => b.Category)
                    .Include(b => b.BlogTags)
                    .Include(b => b.User)
                    .Include(b => b.BlogImages) // Include images
                    .Where(b => b.Category.CategoryName == text)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Blog Repository SearchBlogCategoryAsync Method Error");
                return new List<Blog>();
            }
        }
    }
}
